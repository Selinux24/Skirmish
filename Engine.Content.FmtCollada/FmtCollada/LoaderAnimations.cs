using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Content.FmtCollada
{
    using Engine.Animation;
    using Engine.Collada;
    using Engine.Collada.Types;
    using Engine.Content.Persistence;

    /// <summary>
    /// Animation loader
    /// </summary>
    public class LoaderAnimations : IAnimationLoader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LoaderAnimations()
        {

        }

        /// <summary>
        /// Gets the loader delegate
        /// </summary>
        /// <returns>Returns a delegate wich creates a loader</returns>
        public Func<IAnimationLoader> GetLoaderDelegate()
        {
            return () => { return new LoaderAnimations(); };
        }

        /// <summary>
        /// Gets the extensions list which this loader is valid
        /// </summary>
        /// <returns>Returns a extension array list</returns>
        public IEnumerable<string> GetExtensions()
        {
            return new string[] { ".dae" };
        }

        /// <summary>
        /// Loads animation from a collada file
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="content">Conten description</param>
        /// <returns>Returns the loaded contents</returns>
        public AnimationLibContentData Load(string contentFolder, AnimationLibContentDataFile content)
        {
            string fileName = content.AnimationFileName;

            var modelList = ContentManager.FindContent(contentFolder, fileName);
            if (modelList?.Any() == true)
            {
                AnimationLibContentData res = new AnimationLibContentData();

                foreach (var model in modelList)
                {
                    var dae = Collada.Load(model);

                    //Animations
                    var info = ProcessLibraryAnimations(dae);
                    res.AddAnimation(info);

                    //Release the stream
                    model.Flush();
                    model.Dispose();
                }

                return res;
            }
            else
            {
                throw new EngineException(string.Format("Model not found: {0}", fileName));
            }
        }
        /// <summary>
        /// Process animations
        /// </summary>
        /// <param name="dae">Dae object</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="animation">Animation description</param>
        private static Dictionary<string, IEnumerable<AnimationContent>> ProcessLibraryAnimations(Collada dae)
        {
            Dictionary<string, IEnumerable<AnimationContent>> res = new Dictionary<string, IEnumerable<AnimationContent>>();

            if (dae.LibraryAnimations?.Length > 0)
            {
                for (int i = 0; i < dae.LibraryAnimations.Length; i++)
                {
                    var animationLib = dae.LibraryAnimations[i];

                    var info = ProcessAnimation(animationLib);

                    res.Add(animationLib.Id, info);
                }
            }

            return res;
        }

        /// <summary>
        /// Process animation
        /// </summary>
        /// <param name="animationLibrary">Animation library</param>
        /// <returns>Retuns animation content list</returns>
        public static IEnumerable<AnimationContent> ProcessAnimation(Animation animationLibrary)
        {
            if (animationLibrary?.Channels?.Any() != true)
            {
                return Enumerable.Empty<AnimationContent>();
            }

            List<AnimationContent> res = new List<AnimationContent>();

            foreach (var channel in animationLibrary.Channels)
            {
                var targetParts = channel.Target.Split("/".ToCharArray());

                string jointName = targetParts.ElementAtOrDefault(0) ?? animationLibrary.Id ?? animationLibrary.Name;
                string transformType = targetParts.ElementAtOrDefault(1) ?? "transform";

                foreach (var sampler in animationLibrary.Samplers)
                {
                    var info = BuildAnimationContent(jointName, transformType, sampler, animationLibrary);

                    res.Add(info);
                }
            }

            return res.ToArray();
        }
        /// <summary>
        /// Reads animation data from the specified sampler and builds an AnimationContent instance
        /// </summary>
        /// <param name="jointName">Joint name</param>
        /// <param name="jointTransformType">Transform type</param>
        /// <param name="sampler">Sampler</param>
        /// <param name="animationLibrary">Animation library</param>
        /// <returns>Returns an AnimationContent instance for the Joint</returns>
        private static AnimationContent BuildAnimationContent(string jointName, string jointTransformType, Sampler sampler, Animation animationLibrary)
        {
            //Keyframe times
            var times = ReadTime(sampler, animationLibrary);

            //Keyframe transform matrix
            var transforms = ReadMatrix(sampler, animationLibrary);

            //Keyframe curve positions
            var positions = ReadPosition(sampler, animationLibrary);

            //Keyframe interpolation types
            var interpolations = ReadInterpolations(sampler, animationLibrary);

            List<Keyframe> keyframes = new List<Keyframe>();

            for (int i = 0; i < times.Count(); i++)
            {
                float time = times.ElementAt(i);
                KeyframeInterpolations interpolation = i < interpolations.Count() ? interpolations.ElementAt(i) : KeyframeInterpolations.None;
                float position = -1;
                Matrix transform = Matrix.Identity;
                if (interpolation == KeyframeInterpolations.Linear && i < transforms.Count())
                {
                    transform = transforms.ElementAt(i);
                }
                else if (interpolation == KeyframeInterpolations.Bezier && i < positions.Count())
                {
                    position = positions.ElementAt(i);
                }
                else
                {
                    throw new NotImplementedException($"{interpolation} interpolation not supported.g");
                }

                Keyframe keyframe = new Keyframe()
                {
                    Time = time,
                    Position = position,
                    Transform = transform,
                    Interpolation = interpolation,
                };

                keyframes.Add(keyframe);
            }

            return new AnimationContent()
            {
                JointName = jointName,
                TransformType = jointTransformType,
                Keyframes = keyframes.ToArray(),
            };
        }
        /// <summary>
        /// Reads the time input
        /// </summary>
        /// <param name="sampler">Sampler</param>
        /// <param name="animationLibrary">Animation library</param>
        /// <returns>Returns the keyframe time list</returns>
        private static IEnumerable<float> ReadTime(Sampler sampler, Animation animationLibrary)
        {
            var input = sampler[EnumSemantics.Input];
            if (input != null)
            {
                return animationLibrary[input.Source].ReadFloat();
            }

            return new float[] { };
        }
        /// <summary>
        /// Reads the curve position output
        /// </summary>
        /// <param name="sampler">Sampler</param>
        /// <param name="animationLibrary">Animation library</param>
        /// <returns>Returns the keyframe curve position list</returns>
        private static IEnumerable<float> ReadPosition(Sampler sampler, Animation animationLibrary)
        {
            var input = sampler[EnumSemantics.Output];
            if (input != null)
            {
                return animationLibrary[input.Source].ReadFloat();
            }

            return new float[] { };
        }
        /// <summary>
        /// Reads the linear transformation output
        /// </summary>
        /// <param name="sampler">Sampler</param>
        /// <param name="animationLibrary">Animation library</param>
        /// <returns>Returns the keyframe transform list</returns>
        private static IEnumerable<Matrix> ReadMatrix(Sampler sampler, Animation animationLibrary)
        {
            var input = sampler[EnumSemantics.Output];
            if (input == null)
            {
                return Enumerable.Empty<Matrix>();
            }

            var source = animationLibrary[input.Source];
            if (source.TechniqueCommon.Accessor.Stride == 16)
            {
                return source.ReadMatrix();
            }

            return Enumerable.Empty<Matrix>();
        }
        /// <summary>
        /// Reads the interpolation modes
        /// </summary>
        /// <param name="sampler">Sampler</param>
        /// <param name="animationLibrary">Animation library</param>
        /// <returns>Returns the keyframe interpolation mode list</returns>
        private static IEnumerable<KeyframeInterpolations> ReadInterpolations(Sampler sampler, Animation animationLibrary)
        {
            var input = sampler[EnumSemantics.Interpolation];
            if (input != null)
            {
                return ParseInterpolations(animationLibrary[input.Source].ReadNames());
            }

            return new KeyframeInterpolations[] { };
        }
        /// <summary>
        /// Parses a interpolation list from string to enumeration
        /// </summary>
        /// <param name="interpolations">Interpolation list</param>
        /// <returns>Returns the parsed interpolation list</returns>
        private static IEnumerable<KeyframeInterpolations> ParseInterpolations(IEnumerable<string> interpolations)
        {
            List<KeyframeInterpolations> result = new List<KeyframeInterpolations>();

            foreach (var interpolation in interpolations)
            {
                result.Add(ParseInterpolation(interpolation));
            }

            return result.ToArray();
        }
        /// <summary>
        /// Parse a interpolation from string to enumeration
        /// </summary>
        /// <param name="interpolation">Interpolation</param>
        /// <returns>Returns the parsed interpolation value</returns>
        private static KeyframeInterpolations ParseInterpolation(string interpolation)
        {
            if (string.Equals(interpolation, "BEZIER", StringComparison.OrdinalIgnoreCase))
            {
                return KeyframeInterpolations.Bezier;
            }
            else if (string.Equals(interpolation, "LINEAR", StringComparison.OrdinalIgnoreCase))
            {
                return KeyframeInterpolations.Linear;
            }
            else if (string.Equals(interpolation, "HERMITE", StringComparison.OrdinalIgnoreCase))
            {
                return KeyframeInterpolations.Hermite;
            }
            else if (string.Equals(interpolation, "BSPLINE", StringComparison.OrdinalIgnoreCase))
            {
                return KeyframeInterpolations.BSpline;
            }

            return KeyframeInterpolations.None;
        }
    }
}

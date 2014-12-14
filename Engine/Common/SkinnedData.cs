using System.Collections.Generic;
using SharpDX;

namespace Engine.Common
{
    public class SkinnedData
    {
        public const string DEFAULTCLIP = "default";

        private Dictionary<string, AnimationClip> animations = new Dictionary<string, AnimationClip>();
        private Matrix[] boneOffsets = null;
        private int[] boneHierarchy = null;
        private Matrix[] toParentTransforms = null;
        private Matrix[] toRootTransforms = null;

        public string[] Skins = null;
        //TODO: diccionario de clips calculados
        public Matrix[] FinalTransforms = null;
        public int BoneCount
        {
            get
            {
                return this.boneOffsets.Length;
            }
        }
        public AnimationClip Default
        {
            get
            {
                return this.animations[DEFAULTCLIP];
            }
        }
        public AnimationClip this[string clipName]
        {
            get
            {
                return this.animations[clipName];
            }
        }
        public float AnimationVelocity = 1f;

        /// <summary>
        /// Nombre del clip a aplicar
        /// </summary>
        public string ClipName { get; private set; }
        /// <summary>
        /// Posición temporal en el clip
        /// </summary>
        public float Time { get; set; }
        /// <summary>
        /// Obtiene o establece si el clip se repite o no
        /// </summary>
        public bool Loop { get; set; }

        public static SkinnedData Create(string[] skins, int[] boneHierarchy, Matrix[] boneOffsets, Dictionary<string, AnimationClip> animations)
        {
            return new SkinnedData
            {
                animations = animations,
                boneOffsets = boneOffsets,
                boneHierarchy = boneHierarchy,
                toParentTransforms = GenerateMatrixArray(boneOffsets.Length, Matrix.Identity),
                toRootTransforms = GenerateMatrixArray(boneOffsets.Length, Matrix.Identity),

                Skins = skins,
                FinalTransforms = GenerateMatrixArray(boneOffsets.Length, Matrix.Identity),

                ClipName = DEFAULTCLIP,
                Time = 0f,
                Loop = true,
            };
        }
        private static Matrix[] GenerateMatrixArray(int length, Matrix defaultValue)
        {
            Matrix[] matrixArray = new Matrix[length];

            for (int i = 0; i < length; i++)
            {
                matrixArray[i] = defaultValue;
            }

            return matrixArray;
        }

        /// <summary>
        /// Actualiza el estado interno de la instancia
        /// </summary>
        /// <param name="gameTime">Tiempo de juego</param>
        public virtual void Update(GameTime gameTime)
        {
            if (this.ClipName != null)
            {
                AnimationClip clip = this[this.ClipName];
                float endTime = clip.EndTime;

                if (this.Time == endTime && this.Loop == false)
                {
                    //Do Nothing
                    return;
                }
                else
                {
                    this.Time += gameTime.ElapsedSeconds * this.AnimationVelocity;

                    this.UpdateFinalTransforms(clip, this.Time);

                    if (this.Time > endTime)
                    {
                        if (this.Loop)
                        {
                            //Loop
                            this.Time -= endTime;
                        }
                        else
                        {
                            //Stop
                            this.Time = endTime;
                        }
                    }
                }
            }
        }
        private void UpdateFinalTransforms(AnimationClip clip, float time)
        {
            int numBones = this.boneOffsets.Length;

            this.toParentTransforms = GenerateMatrixArray(numBones, Matrix.Identity);
            this.toRootTransforms = GenerateMatrixArray(numBones, Matrix.Identity);

            clip.Interpolate(time, ref this.toParentTransforms);

            this.toRootTransforms[0] = this.toParentTransforms[0];

            for (int i = 1; i < numBones; i++)
            {
                int parentIndex = this.boneHierarchy[i];

                Matrix toParent = this.toParentTransforms[i];
                Matrix parentToRoot = this.toRootTransforms[parentIndex];

                this.toRootTransforms[i] = toParent * parentToRoot;
            }

            for (int i = 0; i < numBones; i++)
            {
                Matrix offset = this.boneOffsets[i];
                Matrix toRoot = this.toRootTransforms[i];

                this.FinalTransforms[i] = offset * toRoot;
            }
        }
    }
}

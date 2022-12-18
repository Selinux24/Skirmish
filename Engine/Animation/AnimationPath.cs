using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Animation
{
    /// <summary>
    /// Animation path
    /// </summary>
    public class AnimationPath : IHasGameState
    {
        /// <summary>
        /// Animation path items list
        /// </summary>
        private readonly List<AnimationPathItem> items = new List<AnimationPathItem>();
        /// <summary>
        /// Current item index
        /// </summary>
        private int currentItemIndex;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets the total duration of the path
        /// </summary>
        public float TotalDuration
        {
            get
            {
                return items.Sum(i => i.TotalDuration);
            }
        }
        /// <summary>
        /// Gets whether the animation path's items were updated or not
        /// </summary>
        public bool Updated { get; private set; }

        /// <summary>
        /// Gets the current path item
        /// </summary>
        /// <returns>Returns the current path item</returns>
        public AnimationPathItem CurrentItem
        {
            get
            {
                return items.ElementAtOrDefault(currentItemIndex);
            }
        }
        /// <summary>
        /// Gets if the animation path is running
        /// </summary>
        public bool Playing { get; private set; } = true;
        /// <summary>
        /// Path elapsed time
        /// </summary>
        public float PathElapsedTime { get; private set; } = 0f;
        /// <summary>
        /// Path item elapsed time
        /// </summary>
        public float PathItemInterpolationValue { get; private set; } = 0f;
        /// <summary>
        /// Path item partial time (without loops)
        /// </summary>
        public float PartialPathItemInterpolationValue
        {
            get
            {
                float itemDuration = CurrentItem?.Duration ?? 0f;
                if (itemDuration > 0f)
                {
                    return PathItemInterpolationValue % itemDuration;
                }

                return 0f;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AnimationPath()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pathItems">Path items</param>
        public AnimationPath(IEnumerable<AnimationPathItem> pathItems)
        {
            items.AddRange(pathItems);
        }

        /// <summary>
        /// Sets path time
        /// </summary>
        /// <param name="time">Time to set</param>
        public void SetTime(float time)
        {
            PathElapsedTime = time;
            Playing = true;
        }
        /// <summary>
        /// Adds a new animation item to the animation path
        /// </summary>
        /// <param name="clipName">Clip name to play</param>
        /// <param name="timeDelta">Delta time to apply on this animation clip</param>
        public void Add(string clipName, float timeDelta = 1f)
        {
            Add(clipName, false, 1, timeDelta);
        }
        /// <summary>
        /// Adds a new animation item to the animation path, wich repeats N times
        /// </summary>
        /// <param name="clipName">Clip name to play</param>
        /// <param name="repeats">Number of iterations</param>
        /// <param name="timeDelta">Delta time to apply on this animation clip</param>
        public void AddRepeat(string clipName, int repeats, float timeDelta = 1f)
        {
            Add(clipName, false, repeats, timeDelta);
        }
        /// <summary>
        /// Adds a new animation item to the animation path, wich loops for ever!!!
        /// </summary>
        /// <param name="clipName">Clip name to play</param>
        /// <param name="timeDelta">Delta time to apply on this animation clip</param>
        public void AddLoop(string clipName, float timeDelta = 1f)
        {
            Add(clipName, true, 1, timeDelta);
        }
        /// <summary>
        /// Adds a new animation item to the animation path
        /// </summary>
        /// <param name="clipName">Clip name to play</param>
        /// <param name="loop">Loops</param>
        /// <param name="repeats">Number of iterations</param>
        /// <param name="timeDelta">Delta time to apply on this animation clip</param>
        private void Add(string clipName, bool loop, int repeats, float timeDelta)
        {
            //Adds the new item to the path
            var newItem = new AnimationPathItem(clipName, loop, repeats, timeDelta);

            items.Add(newItem);

            Updated = false;
        }

        /// <summary>
        /// Connects the specified path to the current path adding transitions between them
        /// </summary>
        /// <param name="skData">Skinning data</param>
        /// <param name="animationPath">Animation path to connect with current path</param>
        public AnimationPath ConnectTo(ISkinningData skData, AnimationPath animationPath)
        {
            if (animationPath?.items?.Any() != true)
            {
                return new AnimationPath();
            }

            var clonedPath = animationPath.Clone();

            clonedPath.UpdateItems(skData);

            return clonedPath;
        }
        /// <summary>
        /// Sets the items to terminate and end
        /// </summary>
        public void End()
        {
            if (currentItemIndex < 0)
            {
                return;
            }

            if (!items.Any())
            {
                return;
            }

            int index = currentItemIndex;

            var current = items[index];

            //Remove items from current to end
            if (items.Count > index + 1)
            {
                items.RemoveRange(index + 1, items.Count - (index + 1));
            }

            //Calcs total duration of all clips except current clip
            float duration = items.Where(i => i != current).Sum(i => i.TotalDuration);

            //Fix time and item time
            PathElapsedTime = duration + PartialPathItemInterpolationValue;
            PathItemInterpolationValue = PartialPathItemInterpolationValue;

            //Set current item for ending
            current.End();
        }

        /// <summary>
        /// Integrates internal state
        /// </summary>
        /// <param name="skData">Skinning data</param>
        /// <param name="elapsedSeconds">Elapsed seconds</param>
        /// <param name="atEnd">Returns true when the internal path ends</param>
        /// <returns>Returns true when the internal path index changes</returns>
        public bool Integrate(ISkinningData skData, float elapsedSeconds, out bool atEnd)
        {
            atEnd = false;

            if (skData == null)
            {
                return false;
            }

            UpdateItems(skData);

            if (elapsedSeconds == 0f)
            {
                return false;
            }

            int itemIndex = 0;

            float nextTime = PathElapsedTime + elapsedSeconds;
            float acumDuration = 0;
            float itemInterpolationValue = 0;

            //Find the path item, base in the current path time
            for (int i = 0; i < items.Count; i++)
            {
                //Set current item index
                itemIndex = i;
                bool isLast = i == items.Count - 1;

                var current = items[i];

                var lookUpResult = TimeInItem(nextTime, acumDuration, current, isLast);
                acumDuration += lookUpResult.PathItemDuration;
                itemInterpolationValue = lookUpResult.PathItemInterpolationValue;
                atEnd = lookUpResult.AtEnd;

                if (lookUpResult.Result == TimeInItemResults.Found)
                {
                    break;
                }
            }

            PathElapsedTime = atEnd ? acumDuration : nextTime;
            Playing = !atEnd;
            PathItemInterpolationValue = itemInterpolationValue;

            if (currentItemIndex != itemIndex)
            {
                currentItemIndex = itemIndex;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Finds whether the specified time is into the specified path item 
        /// </summary>
        /// <param name="targetTime">Target time</param>
        /// <param name="acumDuration">Accumulated duration</param>
        /// <param name="item">Path item</param>
        /// <param name="isLastItem">It's the last item in the path</param>
        /// <param name="pathDuration">Path duration</param>
        /// <param name="interpolationValue">Path item interpolation value</param>
        /// <param name="atEnd">Returns whether the path is at end or not</param>
        /// <returns>Returns true if the time is into the path</returns>
        private TimeInItemData TimeInItem(float targetTime, float acumDuration, AnimationPathItem item, bool isLastItem)
        {
            var itemTotalDuration = item.TotalDuration * item.TimeDelta;
            if (itemTotalDuration == 0)
            {
                return new TimeInItemData
                {
                    Result = TimeInItemResults.NotFound,
                    PathItemDuration = 0,
                    PathItemInterpolationValue = 0,
                    AtEnd = false,
                    LoopCount = 0,
                };
            }

            if (targetTime - acumDuration < itemTotalDuration)
            {
                float interpolationValue = (targetTime - acumDuration) / item.TimeDelta;

                //This is the item
                return new TimeInItemData
                {
                    Result = TimeInItemResults.Found,
                    PathItemDuration = targetTime - acumDuration,
                    PathItemInterpolationValue = interpolationValue % item.Duration,
                    AtEnd = false,
                    LoopCount = (int)Math.Ceiling(interpolationValue / item.Duration),
                };
            }
            else if (item.Loop)
            {
                float interpolationValue = (targetTime - acumDuration) / item.TimeDelta;

                //Do loop, continue path
                return new TimeInItemData
                {
                    Result = TimeInItemResults.InLoop,
                    PathItemDuration = targetTime - acumDuration,
                    PathItemInterpolationValue = interpolationValue % item.Duration,
                    AtEnd = false,
                    LoopCount = (int)Math.Ceiling(interpolationValue / item.Duration),
                };
            }
            else if (targetTime - acumDuration >= itemTotalDuration && isLastItem)
            {
                //Item passed, it's the end item
                return new TimeInItemData
                {
                    Result = TimeInItemResults.Found,
                    PathItemDuration = targetTime - acumDuration,
                    PathItemInterpolationValue = item.TotalDuration,
                    AtEnd = true,
                    LoopCount = 0,
                };
            }
            else
            {
                //Not in time
                return new TimeInItemData
                {
                    Result = TimeInItemResults.NotFound,
                    PathItemDuration = itemTotalDuration,
                    PathItemInterpolationValue = 0,
                    AtEnd = false,
                    LoopCount = 0,
                };
            }
        }
        /// <summary>
        /// Time in item method results
        /// </summary>
        private struct TimeInItemData
        {
            /// <summary>
            /// Look up result
            /// </summary>
            public TimeInItemResults Result { get; set; }
            /// <summary>
            /// Processed path item duration
            /// </summary>
            public float PathItemDuration { get; set; }
            /// <summary>
            /// Processed path item interpolation value
            /// </summary>
            public float PathItemInterpolationValue { get; set; }
            /// <summary>
            /// Gets whether the path is at end or not
            /// </summary>
            /// <remarks>The processed path item is the last, and the time is greater of it's total duration</remarks>
            public bool AtEnd { get; set; }
            /// <summary>
            /// Processed path item loop count
            /// </summary>
            public int LoopCount { get; set; }
        }
        /// <summary>
        /// Time in item results
        /// </summary>
        private enum TimeInItemResults
        {
            /// <summary>
            /// Item found
            /// </summary>
            Found,
            /// <summary>
            /// Item not found
            /// </summary>
            NotFound,
            /// <summary>
            /// Currently in loop
            /// </summary>
            InLoop,
        }

        /// <summary>
        /// Updates the internal item list
        /// </summary>
        /// <param name="skData">Skinning data</param>
        public void UpdateItems(ISkinningData skData)
        {
            if (Updated)
            {
                return;
            }

            if (skData == null)
            {
                return;
            }

            Updated = true;

            if (!items.Any())
            {
                return;
            }

            items.ForEach(i => i.Update(skData));
        }

        /// <summary>
        /// Gets the clip names into a comma separated list
        /// </summary>
        /// <returns>Returns the clip names of the path</returns>
        public string GetItemList()
        {
            return items
                .Select(i => $"{i.ClipName}{(i.Loop ? ".Loop" : "")}{(i.Repeats > 1 ? $".R{i.Repeats}" : "")} x t{i.TimeDelta:0.00}")
                .Join(" => ");
        }
        /// <summary>
        /// Creates a copy of the current path
        /// </summary>
        /// <returns>Returns the path copy instance</returns>
        public AnimationPath Clone()
        {
            var clonedItems = items.Select(i => i.Clone()).ToArray();

            return new AnimationPath(clonedItems)
            {
                Name = Name,
                currentItemIndex = currentItemIndex,
                PathItemInterpolationValue = PathItemInterpolationValue,
                Playing = Playing,
                PathElapsedTime = PathElapsedTime,
            };
        }

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new AnimationPathState
            {
                Name = Name,
                CurrentIndex = currentItemIndex,
                Playing = Playing,
                Time = PathElapsedTime,
                TotalItemTime = PathItemInterpolationValue,
                PathItems = items.Select(i => i.GetState()).ToArray(),
            };
        }
        /// <inheritdoc/>
        public void SetState(IGameState state)
        {
            if (!(state is AnimationPathState animationPathState))
            {
                return;
            }

            Name = animationPathState.Name;
            currentItemIndex = animationPathState.CurrentIndex;
            Playing = animationPathState.Playing;
            PathElapsedTime = animationPathState.Time;
            PathItemInterpolationValue = animationPathState.TotalItemTime;
            for (int i = 0; i < animationPathState.PathItems.Count(); i++)
            {
                var itemState = animationPathState.PathItems.ElementAt(i);
                items[i].SetState(itemState);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (CurrentItem == null)
            {
                return $"{Name ?? "NoName"} - {items.Count} items.";
            }
            else if (CurrentItem.Loop)
            {
                return $"{Name ?? "NoName"} - {items.Count} items. Time: {PathElapsedTime:00.00}; Interpolation: {PartialPathItemInterpolationValue:00.00} of {PathItemInterpolationValue:00.00}; Item: {CurrentItem}";
            }
            else
            {
                return $"{Name ?? "NoName"} - {items.Count} items. Time: {PathElapsedTime:00.00}; Interpolation: {PartialPathItemInterpolationValue:00.00}; Item: {CurrentItem}";
            }
        }
    }
}

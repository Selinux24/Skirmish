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
        /// Gets if the animation path is running
        /// </summary>
        public bool Playing { get; private set; } = true;
        /// <summary>
        /// Path time
        /// </summary>
        public float Time { get; private set; } = 0f;
        /// <summary>
        /// Total item time
        /// </summary>
        public float TotalItemTime { get; private set; } = 0f;
        /// <summary>
        /// Item time
        /// </summary>
        public float ItemTime
        {
            get
            {
                float itemDuration = CurrentItem?.Duration ?? 0f;
                if (itemDuration > 0f)
                {
                    return TotalItemTime % itemDuration;
                }

                return 0f;
            }
        }
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
            Time = time;
            TotalItemTime = time;
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
            //Gets last item
            var prevItem = items.LastOrDefault();
            if (prevItem != null)
            {
                //Adds a transition from the last item to the new item
                var transition = new AnimationPathItem(prevItem.ClipName + clipName, false, 1, timeDelta, true);

                items.Add(transition);
            }

            //Adds the new item to the path
            var newItem = new AnimationPathItem(clipName, loop, repeats, timeDelta, false);

            items.Add(newItem);
        }

        /// <summary>
        /// Connects the specified path to the current path adding transitions between them
        /// </summary>
        /// <param name="animationPath">Animation path to connect with current path</param>
        /// <param name="timeDelta">Delta time to apply on this animation clip</param>
        public void ConnectTo(AnimationPath animationPath, float timeDelta = 1f)
        {
            var lastItem = items.Last();
            var nextItem = animationPath.items.First();

            if (lastItem.ClipName != nextItem.ClipName)
            {
                var newItem = new AnimationPathItem(lastItem.ClipName + nextItem.ClipName, false, 1, timeDelta, true);

                animationPath.items.Insert(0, newItem);
            }
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

            if (items[index].IsTranstition)
            {
                index++;
            }

            var current = items[index];

            //Remove items from current to end
            if (items.Count > index + 1)
            {
                items.RemoveRange(index + 1, items.Count - (index + 1));
            }

            //Calcs total time of all clips except current clip
            float t = items.Where(i => i != current).Sum(i => i.TotalDuration);

            //Fix time and item time
            Time = t + ItemTime;
            TotalItemTime = ItemTime;

            //Set current item for ending
            current.End();
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="skData">Skinning data</param>
        /// <param name="delta">Delta time</param>
        /// <param name="updated">Returns true when the internal path index change</param>
        /// <param name="atEnd">Returns true when the internal path ends</param>
        public void Update(ISkinningData skData, float delta, out bool updated, out bool atEnd)
        {
            updated = false;
            atEnd = false;

            if (skData == null)
            {
                return;
            }

            int itemIndex = 0;

            float time = 0;
            float nextTime = 0;
            float clipTime = 0;

            if (delta > 0)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    //Set current item index
                    itemIndex = i;
                    bool isLast = i == items.Count - 1;

                    var current = items[i];

                    clipTime = nextTime = Time + (delta * current.TimeDelta);

                    //Update current item
                    current.Update(skData);

                    bool? continuePath = UpdateItem(current, isLast, nextTime, ref time, out atEnd, out float t);
                    if (continuePath.HasValue)
                    {
                        if (!continuePath.Value)
                        {
                            break;
                        }
                    }
                    else
                    {
                        clipTime -= t;
                    }
                }
            }

            Time = atEnd ? time : nextTime;
            Playing = !atEnd;
            TotalItemTime = Math.Max(0, clipTime);

            if (currentItemIndex != itemIndex)
            {
                currentItemIndex = itemIndex;

                updated = true;
            }
        }
        /// <summary>
        /// Updates the current item
        /// </summary>
        /// <param name="item">Item to update</param>
        /// <param name="isLastItem">It's the last item in the path</param>
        /// <param name="nextTime">Next time</param>
        /// <param name="time">Current time</param>
        /// <param name="atEnd">Returns if the path is at end</param>
        /// <param name="t">Evaluated time</param>
        /// <returns>Returns false if the path has to stop or true if it has to continue</returns>
        private bool? UpdateItem(AnimationPathItem item, bool isLastItem, float nextTime, ref float time, out bool atEnd, out float t)
        {
            atEnd = false;
            t = item.TotalDuration;
            if (t == 0) return true;

            if (item.Loop)
            {
                //Adjust time in the loop
                float d = nextTime - time;
                t = d % t;
            }

            time += t;

            if (time - t <= nextTime && time > nextTime)
            {
                //This is the item, stop the path
                return false;
            }
            else if (item.Loop)
            {
                //Do loop, continue path
                return true;
            }
            else if (nextTime >= time && isLastItem)
            {
                //Item passed, it's the end item
                atEnd = true;
                return false;
            }

            return null;
        }

        /// <summary>
        /// Gets the clip names into a comma separated list
        /// </summary>
        /// <returns>Returns the clip names of the path</returns>
        public string GetItemList()
        {
            string[] itemList = items.Select(i => i.ClipName).ToArray();

            return itemList.Join(", ");
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
                currentItemIndex = currentItemIndex,
                TotalItemTime = TotalItemTime,
                Playing = Playing,
                Time = Time,
            };
        }

        /// <inheritdoc/>
        public IGameState GetState()
        {
            return new AnimationPathState
            {
                CurrentIndex = currentItemIndex,
                Playing = Playing,
                Time = Time,
                TotalItemTime = TotalItemTime,
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

            currentItemIndex = animationPathState.CurrentIndex;
            Playing = animationPathState.Playing;
            Time = animationPathState.Time;
            TotalItemTime = animationPathState.TotalItemTime;
            for (int i = 0; i < animationPathState.PathItems.Count(); i++)
            {
                var itemState = animationPathState.PathItems.ElementAt(i);
                items[i].SetState(itemState);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Items: {items.Count}; Time: {Time:00.00}; Item Time: {ItemTime:00.00}";
        }
    }
}

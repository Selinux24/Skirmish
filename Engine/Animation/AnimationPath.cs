using System;
using System.Collections.Generic;

namespace Engine.Animation
{
    /// <summary>
    /// Animation path
    /// </summary>
    public class AnimationPath
    {
        /// <summary>
        /// Animation path items list
        /// </summary>
        private List<AnimationPathItem> items = new List<AnimationPathItem>();
        /// <summary>
        /// Current item index
        /// </summary>
        private int currentIndex;

        /// <summary>
        /// Gets if the animation path is running
        /// </summary>
        public bool Playing { get; private set; }
        /// <summary>
        /// Path time
        /// </summary>
        public float Time { get; private set; }
        /// <summary>
        /// Total item time
        /// </summary>
        public float TotalItemTime { get; private set; }
        /// <summary>
        /// Item time
        /// </summary>
        public float ItemTime
        {
            get
            {
                var item = this.GetCurrentItem();
                if (item != null && item.Duration > 0f)
                {
                    return this.TotalItemTime % item.Duration;
                }

                return 0;
            }
        }
        /// <summary>
        /// Gets the total duration of the path
        /// </summary>
        public float TotalDuration
        {
            get
            {
                float d = 0;

                for (int i = 0; i < this.items.Count; i++)
                {
                    d += this.items[i].TotalDuration;
                }

                return d;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AnimationPath()
        {
            this.Time = 0f;
            this.TotalItemTime = 0f;
            this.Playing = true;
        }

        /// <summary>
        /// Sets path time
        /// </summary>
        /// <param name="time">Time to set</param>
        public void SetTime(float time)
        {
            this.Time = time;
            this.TotalItemTime = time;
            this.Playing = true;
        }
        /// <summary>
        /// Adds a new animation item to the animation path
        /// </summary>
        /// <param name="clipName">Clip name to play</param>
        /// <param name="timeDelta">Delta time to apply on this animation clip</param>
        public void Add(string clipName, float timeDelta = 1f)
        {
            this.Add(clipName, false, 1, timeDelta);
        }
        /// <summary>
        /// Adds a new animation item to the animation path, wich repeats N times
        /// </summary>
        /// <param name="clipName">Clip name to play</param>
        /// <param name="repeats">Number of iterations</param>
        /// <param name="timeDelta">Delta time to apply on this animation clip</param>
        public void AddRepeat(string clipName, int repeats, float timeDelta = 1f)
        {
            this.Add(clipName, false, repeats, timeDelta);
        }
        /// <summary>
        /// Adds a new animation item to the animation path, wich loops for ever!!!
        /// </summary>
        /// <param name="clipName">Clip name to play</param>
        /// <param name="timeDelta">Delta time to apply on this animation clip</param>
        public void AddLoop(string clipName, float timeDelta = 1f)
        {
            this.Add(clipName, true, 1, timeDelta);
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
            AnimationPathItem prev = null;
            if (this.items.Count > 0)
            {
                //Gets last item
                prev = this.items[this.items.Count - 1];
            }

            if (prev != null)
            {
                var transition = new AnimationPathItem(prev.ClipName + clipName, false, 1, timeDelta, true);

                items.Add(transition);
            }

            var newItem = new AnimationPathItem(clipName, loop, repeats, timeDelta, false);

            items.Add(newItem);
        }

        /// <summary>
        /// Connects the specified path to the current path adding transitions between them
        /// </summary>
        /// <param name="animationPath">Animation path to connect with current path</param>
        public void ConnectTo(AnimationPath animationPath)
        {
            var lastItem = this.items[this.items.Count - 1];
            var nextItem = animationPath.items[0];

            if (lastItem.ClipName != nextItem.ClipName)
            {
                var newItem = new AnimationPathItem(lastItem.ClipName + nextItem.ClipName, false, 1, 1f, true);

                animationPath.items.Insert(0, newItem);
            }
        }
        /// <summary>
        /// Sets the items to terminate and end
        /// </summary>
        public void End()
        {
            var index = this.currentIndex;
            if (index >= 0)
            {
                if (this.items[index].IsTranstition) index++;

                var current = this.items[index];

                //Remove items from current to end
                if (this.items.Count > index + 1)
                {
                    this.items.RemoveRange(index + 1, this.items.Count - (index + 1));
                }

                //Calcs total time of all clips
                float t = 0;
                this.items.ForEach(i => t += i != current ? i.TotalDuration : 0f);

                //Fix time and item time
                this.Time = t + this.ItemTime;
                this.TotalItemTime = this.ItemTime;

                //Set current item for ending
                current.End();
            }
        }

        /// <summary>
        /// Updates internal state
        /// </summary>
        /// <param name="delta">Delta time</param>
        /// <param name="skData">Skinning data</param>
        public void Update(float delta, SkinningData skData)
        {
            int itemIndex = 0;

            float nextTime = this.Time + delta;
            float clipTime = nextTime;

            float time = 0;
            bool atEnd = false;

            if (nextTime > 0)
            {
                for (int i = 0; i < this.items.Count; i++)
                {
                    //Set current item index
                    itemIndex = i;
                    bool isLast = (i == this.items.Count - 1);

                    var current = this.items[i];

                    //Update current item
                    current.UpdateSkinningData(skData);

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

            this.Time = atEnd ? time : nextTime;
            this.Playing = !atEnd;
            this.TotalItemTime = Math.Max(0, clipTime);

            this.currentIndex = itemIndex;
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
            else if (nextTime >= time && isLastItem /*i == this.items.Count - 1*/)
            {
                //Item passed, it's the end item
                atEnd = true;
                return false;
            }

            return null;
        }
        /// <summary>
        /// Gets the current path item
        /// </summary>
        /// <returns>Returns the current path item</returns>
        public AnimationPathItem GetCurrentItem()
        {
            if (this.currentIndex >= 0)
            {
                return this.items[this.currentIndex];
            }

            return null;
        }
        /// <summary>
        /// Gets the clip names into a strings array
        /// </summary>
        /// <returns>Returns the clip names of the path</returns>
        public string[] GetItemList()
        {
            string[] res = new string[this.items.Count];
            for (int i = 0; i < this.items.Count; i++)
            {
                res[i] = this.items[i].ClipName;
            }
            return res;
        }

        /// <summary>
        /// Creates a copy of the current path
        /// </summary>
        /// <returns>Returns the path copy instance</returns>
        public AnimationPath Clone()
        {
            List<AnimationPathItem> clonedItems = new List<AnimationPathItem>();

            foreach (var item in this.items)
            {
                clonedItems.Add(item.Clone());
            }

            return new AnimationPath()
            {
                items = clonedItems,
                currentIndex = this.currentIndex,
                TotalItemTime = this.TotalItemTime,
                Playing = this.Playing,
                Time = this.Time,
            };
        }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            return string.Format("Items: {0}; Time: {1:00.00}; Item Time: {2:00.00}",
                this.items.Count,
                this.Time,
                this.ItemTime);
        }
    }
}

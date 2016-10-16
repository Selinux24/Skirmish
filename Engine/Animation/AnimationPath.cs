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
        /// Path time
        /// </summary>
        public float Time { get; set; }
        /// <summary>
        /// Current item time
        /// </summary>
        public float ItemTime { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public AnimationPath()
        {
            this.Time = 0f;
            this.ItemTime = 0f;
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
                items.Add(new AnimationPathItem()
                {
                    ClipName = prev.ClipName + clipName,
                    TimeDelta = timeDelta,
                    Loop = false,
                    Repeats = 1,
                });
            }

            items.Add(new AnimationPathItem()
            {
                ClipName = clipName,
                TimeDelta = timeDelta,
                Loop = loop,
                Repeats = repeats,
            });
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
                    itemIndex = i;

                    //Gets total time in this clip
                    int clipIndex = skData.GetClipIndex(this.items[i].ClipName);
                    float t = skData.GetClipDuration(clipIndex) * this.items[i].Repeats / this.items[i].TimeDelta;
                    if (t == 0) continue;

                    if (this.items[i].Loop)
                    {
                        //Adjust time in the loop
                        float d = nextTime - time;
                        t = d % t;
                    }

                    time += t;

                    if (time - t <= nextTime && time > nextTime)
                    {
                        //This is the item
                        break;
                    }
                    else if (this.items[i].Loop)
                    {
                        //Do loop
                        continue;
                    }
                    else if (nextTime >= time && i == this.items.Count - 1)
                    {
                        //Item passed, it's end item
                        atEnd = true;
                        break;
                    }
                 
                    clipTime -= t;
                }
            }

            if (atEnd)
            {
                this.Time = time;
            }
            else
            {
                this.Time = nextTime;
            }

            this.ItemTime = clipTime;

            this.currentIndex = itemIndex;
        }
        /// <summary>
        /// Gets the current path item
        /// </summary>
        /// <returns>Returns the current path item</returns>
        public AnimationPathItem GetCurrentItem()
        {
            return this.items[this.currentIndex];
        }
    }
}

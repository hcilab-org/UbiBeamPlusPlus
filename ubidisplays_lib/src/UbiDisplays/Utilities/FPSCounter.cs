using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace UbiDisplays.Utilities
{
    /// <summary>
    /// A simple FPS counter which keeps track of how many times 'newFrame' has been called each second.
    /// This is an accurate FPS counter which actually tracks how many frames have been processed in the last second.
    /// </summary>
    /// <author>John Hardy</author>
    /// <date>1st November 2011</date>
    public class FPSCounter
    {
        /// <summary>
        /// The current frame count.
        /// </summary>
        private int iFrameCount = 0;

        /// <summary>
        /// The last frame count.
        /// </summary>
        private int iLastFrameCount = 0;

        /// <summary>
        /// The last time we did a frame count update.
        /// </summary>
        private DateTime mLastTime;

        /// <summary>
        /// The last accessed number of frames.
        /// </summary>
        private int iLastFPS = 0;

        /// <summary>
        /// A mutex which manages access to iLastFPS.
        /// </summary>
        private Mutex mLock = new Mutex();

        /// <summary>
        /// Get the number of frames which were processed in the last second.
        /// </summary>
        public int FPS
        {
            get
            {
                mLock.WaitOne();
                int iTmp = iLastFPS;
                mLock.ReleaseMutex();
                return iTmp;
            }
            protected set
            {
                mLock.WaitOne();
                iLastFPS = value;
                mLock.ReleaseMutex();
            }
        }

        /// <summary>
        /// Create a new FPS counter.
        /// </summary>
        public FPSCounter()
        {
            // Reset all the settings.
            this.Reset();
        }

        /// <summary>
        /// Reset all the FPS counters back to 0.
        /// </summary>
        public void Reset()
        {
            this.iFrameCount = 0;
            this.iLastFPS = 0;
            this.iLastFrameCount = 0;
            this.mLastTime = DateTime.Now;
            this.FPS = 0;
        }

        /// <summary>
        /// Call this to tell the FPS counter that we have recieved a new frame.
        /// This call is threadsafe, but may block as it only uses a single mutex.
        /// </summary>
        public void NewFrame()
        {
            // Increment the frame count.
            ++iFrameCount;

            // Settings.
            DateTime mCurrent = DateTime.Now;
            if (mCurrent.Subtract(mLastTime) > TimeSpan.FromSeconds(1))
            {
                // Update the new FPS.
                this.FPS = iFrameCount - iLastFrameCount;

                // Modify the counters.
                iLastFrameCount = iFrameCount;
                mLastTime = mCurrent;
            }
        }
    }
}
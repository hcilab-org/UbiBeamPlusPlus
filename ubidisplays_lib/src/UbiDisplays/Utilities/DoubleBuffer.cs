using System;
using System.Threading;

namespace UbiDisplays.Utilities
{
    /// <summary>
    /// This class implements a lockable double buffered resource.
    /// </summary>
    /// <author>John Hardy</author>
    /// <date>14th Nov 2012</date>
    public class DoubleBuffer<T>
    {
        /// <summary>
        /// A pointer to the active resource.
        /// </summary>
        private T pActive;

        /// <summary>
        /// A pointer to the next resource.
        /// </summary>
        private T pNext;

        /// <summary>
        /// A mutex which lets us ensure the buffer is not flipped accidentally.
        /// </summary>
        private Mutex mCurrent = new Mutex();

        /// <summary>
        /// A mutex which lets us ensure the buffer is not flipped accidentally.
        /// </summary>
        private Mutex mNext = new Mutex();

        /// <summary>
        /// A boolean so we know state.  This variable is not threadsafe.
        /// </summary>
        private bool bCurrentLocked = false;

        /// <summary>
        /// Is access to the next buffer locked.
        /// </summary>
        private bool bNextLocked = false;

        /// <summary>
        /// Create a double buffer with both items.
        /// </summary>
        public DoubleBuffer(T pActive, T pNext)
        {
            this.pActive    = pActive;
            this.pNext      = pNext;
        }

        /// <summary>
        /// Flip the pointers if the active resource is not locked (blocking otherwise), making the active array the next array and visa-versa.
        /// The calling thread will block until both UnlockCurrent and LockCurrent are called.
        /// </summary>
        public void Flip()
        {
            // Acquire the mutices required to flip the buffer.
            mCurrent.WaitOne();
            mNext.WaitOne();
            this.bNextLocked = true;
            this.bCurrentLocked = true;

            // A since, simple swap of the references.
            T pTemp = pActive;
            pActive = pNext;
            pNext = pTemp;

            // Release the mutices now we are done.
            this.bNextLocked = false;
            this.bCurrentLocked = false;
            mNext.ReleaseMutex();
            mCurrent.ReleaseMutex();
        }

        /// <summary>
        /// Get the 'active' resource.  For example, the queue we are using to render.
        /// </summary>
        /// <returns>A reference to the 'active' resource.</returns>
        public T Current { get {return pActive;} }

        /// <summary>
        /// Get the 'next' resource.   For example, the queue we are populating while rendering takes place.
        /// </summary>
        /// <returns>A reference to the 'next' resource.</returns>
        public T Next { get {return pNext;} }

        /// <summary>
        /// Lock the state of the double buffer so that it cannot be flipped.
        /// Note: You MUST call "unlockState" so that the buffer can be flipped.
        /// </summary>
        public void LockCurrent()
        {
            // Lock the mutex required to flip the buffer so it cannot be flipped.
            mCurrent.WaitOne();
            this.bCurrentLocked = true;
        }

        /// <summary>
        /// Attempt to lock the state of the double buffer so that it cannot be flipped.
        /// Note: You MUST call "unlockState" so that the buffer can be flipped.
        /// </summary>
        /// <param name="iLockWait">The number of milliseconds to wait before giving up on being able to establish a lock.</param>
        public bool TryLockCurrent(int iLockWait = 100)
        {
            // Lock the mutex required to flip the buffer so it cannot be flipped.
            if (!mCurrent.WaitOne(iLockWait))
                return false;
            this.bCurrentLocked = true;
            return true;
        }

        /// <summary>
        /// Unlock the state of the double buffer so that it can now be flipped.
        /// </summary>
        public void UnlockCurrent()
        {
            // Set the variable before we release the mutex... hehe otherwise we create a veritable breeding ground for bugs!
            this.bCurrentLocked = false;
            mCurrent.ReleaseMutex();
        }

        /// <summary>
        /// Lock the state of the double buffer so that it cannot be flipped.
        /// Note: You MUST call "unlockState" so that the buffer can be flipped.
        /// </summary>
        public void LockNext()
        {
            // Lock the mutex required to flip the buffer so it cannot be flipped.
            mNext.WaitOne();
            this.bNextLocked = true;
        }

        /// <summary>
        /// Unlock the state of the double buffer so that it can now be flipped.
        /// </summary>
        public void UnlockNext()
        {
            // Set the variable before we release the mutex... hehe otherwise we create a veritable breeding ground for bugs!
            this.bNextLocked = false;
            mNext.ReleaseMutex();
        }

        /// <summary>
        /// Unlock the state of the double buffer after flipping.
        /// </summary>
        public void UnlockNextAndFlip()
        {
            // We already have the next mutex, so acquire the current one too.
            mCurrent.WaitOne();
            this.bCurrentLocked = true;

            // Do the flip!
            T pTemp = pActive;
            pActive = pNext;
            pNext = pTemp;

            // Set the variable before we release the mutex... hehe otherwise we create a veritable breeding ground for bugs!
            this.bNextLocked = false;
            this.bCurrentLocked = false;
            mNext.ReleaseMutex();
            mCurrent.ReleaseMutex();
        }

        /// <summary>
        /// Return the state of the double buffer.
        /// </summary>
        /// <returns>True if it is locked (and cannot be flipped) or False if it is unlocked and can be flipped.</returns>
        public bool IsLocked { get { return this.bCurrentLocked || this.bNextLocked; } }
    }
}

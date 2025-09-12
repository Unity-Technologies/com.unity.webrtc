using System;

namespace Unity.WebRTC
{
    /// <summary>
    /// Base class for objects managed with reference and disposal.
    /// </summary>
    public abstract class RefCountedObject : IDisposable
    {
        internal IntPtr self;

        /// <summary>
        /// Indicates whether the object has been disposed.
        /// </summary>
        internal protected bool disposed;

        internal RefCountedObject(IntPtr ptr)
        {
            self = ptr;
            WebRTC.Context.AddRefPtr(self);
        }

        internal IntPtr GetSelfOrThrow()
        {
            if (self == IntPtr.Zero)
            {
                throw new ObjectDisposedException(
                    GetType().FullName, "This instance has been disposed.");
            }
            return self;
        }

        /// <summary>
        /// Releases resources used by the object.
        /// </summary>
        public virtual void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                WebRTC.Context.DeleteRefPtr(self);
                self = IntPtr.Zero;
            }

            this.disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}

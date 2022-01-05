using System;

namespace Unity.WebRTC
{
    public class RefCountedObject : IDisposable
    {
        internal IntPtr self;

        protected bool disposed;

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

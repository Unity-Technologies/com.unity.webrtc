using System;

namespace Unity.WebRTC
{
    public class RefCounterObject : IDisposable
    {
        internal IntPtr self;

        protected bool disposed;

        internal RefCounterObject(IntPtr ptr)
        {
            self = ptr;
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

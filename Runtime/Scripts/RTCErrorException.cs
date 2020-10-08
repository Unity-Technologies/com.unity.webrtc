using System;

namespace Unity.WebRTC
{
    /// <summary>
    /// This exception class contains properties which determines
    /// a error type occurred while handling WebRTC operations.
    /// </summary>
    public class RTCErrorException : Exception
    {
        private RTCError m_error;

        internal RTCErrorException(ref RTCError error) : base(error.message)
        {
            m_error = error;
        }

        /// <summary>
        /// This property specifies the WebRTC-specific error code.
        /// </summary>
        public RTCErrorType ErrorType { get { return m_error.errorType; } }
    }
}

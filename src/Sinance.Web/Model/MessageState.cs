namespace Sinance.Web.Model
{
    /// <summary>
    /// Enumeration for message states
    /// </summary>
    public enum MessageState
    {
        /// <summary>
        /// No message state
        /// </summary>
        None = 0,

        /// <summary>
        /// Success message state
        /// </summary>
        Success = 1,

        /// <summary>
        /// Info message state
        /// </summary>
        Info = 2,

        /// <summary>
        /// Warning message state
        /// </summary>
        Warning = 3,

        /// <summary>
        /// Error message state
        /// </summary>
        Error = 4
    }
}
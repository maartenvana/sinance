namespace Sinance.Business.Classes
{
    /// <summary>
    /// Standard json result
    /// </summary>
    public class SinanceJsonResult
    {
        /// <summary>
        /// Optional error message to display if not successfull
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Optional data to return if successfull
        /// </summary>
        public object ObjectData { get; set; }

        /// <summary>
        /// If the operation completed succesfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public SinanceJsonResult() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="success">If the call is successfull</param>
        public SinanceJsonResult(bool success)
        {
            Success = success;
        }
    }
}
namespace Sinance.Application.Exceptions
{
    [Serializable]
    public sealed class SinanceCommandValidationException : Exception
    {
        public SinanceCommandValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        private SinanceCommandValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

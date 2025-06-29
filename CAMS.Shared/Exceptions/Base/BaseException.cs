namespace CAMS.Shared.Exceptions.Base
{
    public class BaseException : ApplicationException
    {
        public int ErrorCode { get; set; }

        public BaseException() { }

        public BaseException(string message) : base(message) { }

        public BaseException(int errorCode) : this($"Error occurred with code: {errorCode}", errorCode) { }

        public BaseException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public BaseException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}

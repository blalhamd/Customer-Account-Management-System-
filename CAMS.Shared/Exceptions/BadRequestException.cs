using CAMS.Shared.Exceptions.Base;

namespace CAMS.Shared.Exceptions
{
    public class BadRequestException : BaseException
    {
        public BadRequestException() : base("Bad Request Exception"){ }

        public BadRequestException(string message) : base(message) { }

        public BadRequestException(int errorCode): base(errorCode) { }
        

        public BadRequestException(string message, int errorCode) : base(message, errorCode) { }

        public BadRequestException(string? message, Exception? innerException) : base(message, innerException) { }

    }
}

using CAMS.Shared.Exceptions.Base;

namespace CAMS.Shared.Exceptions
{
    public class ItemAlreadyExistException : BaseException
    {
        public ItemAlreadyExistException() : base("Item Already Exist") { }

        public ItemAlreadyExistException(string message) : base(message) { }

        public ItemAlreadyExistException(int errorCode) : base(errorCode) { }

        public ItemAlreadyExistException(string message, int errorCode) : base(message, errorCode) { }

        public ItemAlreadyExistException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}

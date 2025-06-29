using CAMS.Shared.Exceptions.Base;

namespace CAMS.Shared.Exceptions
{
    public class ItemNotFoundException : BaseException
    {
        public ItemNotFoundException() : base("Item Not Found") { }

        public ItemNotFoundException(string message) : base(message) { }

        public ItemNotFoundException(int errorCode) : base(errorCode) { } 

        public ItemNotFoundException(string message, int errorCode) : base(message, errorCode) { }

        public ItemNotFoundException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}

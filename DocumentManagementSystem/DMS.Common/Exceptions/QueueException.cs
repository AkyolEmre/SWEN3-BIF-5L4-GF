namespace DMS.Common.Exceptions
{
    public class QueueException : Exception
    {
        public QueueException(string message) : base(message) { }
        public QueueException(string message, Exception innerException) : base(message, innerException) { }
    }
}
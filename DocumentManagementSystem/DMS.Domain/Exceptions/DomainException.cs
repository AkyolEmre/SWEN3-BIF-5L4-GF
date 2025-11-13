namespace DMS.Domain.Exceptions
{
    public class DomainException : System.Exception
    {
        public DomainException(string message, System.Exception? inner = null) : base(message, inner) { }
    }
}
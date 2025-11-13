namespace DMS.DAL.Exceptions
{
    public class DalException : System.Exception
    {
        public DalException(string message, System.Exception? inner = null) : base(message, inner) { }
    }
}
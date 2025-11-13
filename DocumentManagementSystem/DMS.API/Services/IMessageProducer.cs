// Add this before the MessageProducer class in MessageProducer.cs
public interface IMessageProducer
{
    Task SendMessageAsync(string message);
}
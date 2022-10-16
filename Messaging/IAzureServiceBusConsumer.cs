namespace Mango.Service.OrderApi.Messaging;

public interface IAzureServiceBusConsumer
{
    Task Start();
    Task Stop();
}
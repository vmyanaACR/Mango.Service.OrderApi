using Mango.Service.OrderApi.Messaging;

namespace Mango.Service.OrderApi.Extensions;

public static class ApplicationBuilderExtension
{
    public static IAzureServiceBusConsumer ServiceBusConsumer { get; set; }

    public static IApplicationBuilder UseServiceBusConsumer(this IApplicationBuilder app)
    {
        ServiceBusConsumer = app.ApplicationServices.GetService<IAzureServiceBusConsumer>();
        var appLife = app.ApplicationServices.GetService<IHostApplicationLifetime>();
        appLife.ApplicationStarted.Register(OnStart);
        appLife.ApplicationStopped.Register(OnStop);
        return app;
    }

    private static void OnStart()
    {
        ServiceBusConsumer.Start();
    }

    private static void OnStop()
    {
        ServiceBusConsumer.Stop();
    }
}
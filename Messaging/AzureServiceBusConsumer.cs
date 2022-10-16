using System.Text;
using Azure.Messaging.ServiceBus;
using Mango.Service.OrderApi.Messages;
using Mango.Service.OrderApi.Models;
using Mango.Service.OrderApi.Repository;
using Newtonsoft.Json;

namespace Mango.Service.OrderApi.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
    private readonly string serviceBusConnectionString;
    private readonly string subscriptionCheckOut;
    private readonly string checkoutMessageTopic;
    private readonly string orderPaymentProcessTopic;
    private readonly string orderUpdatePaymentResultTopic;
    private ServiceBusProcessor checkOutProcessor;
    private readonly OrderRepository _orderRepository;
    private readonly IConfiguration _configuration;

    public AzureServiceBusConsumer(OrderRepository orderRepository,
        IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _configuration = configuration;
        serviceBusConnectionString = _configuration.GetValue<string>("ServiceBusConnectionString");
        subscriptionCheckOut = _configuration.GetValue<string>("SubscriptionCheckOut");
        checkoutMessageTopic = _configuration.GetValue<string>("CheckoutMessageTopic");
        orderPaymentProcessTopic = _configuration.GetValue<string>("OrderPaymentProcessTopics");
        orderUpdatePaymentResultTopic = _configuration.GetValue<string>("OrderUpdatePaymentResultTopic");

        var client = new ServiceBusClient(serviceBusConnectionString);
        checkOutProcessor = client.CreateProcessor(checkoutMessageTopic, subscriptionCheckOut);
    }

    public async Task Start()
    {
        checkOutProcessor.ProcessMessageAsync += OnCheckoutMessageReceived;
        checkOutProcessor.ProcessErrorAsync += ErrorHandler;
        await checkOutProcessor.StartProcessingAsync();
    }

    public async Task Stop()
    {
        await checkOutProcessor.StopProcessingAsync();
        await checkOutProcessor.DisposeAsync();
    }

    Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }

    private async Task OnCheckoutMessageReceived(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        var body = Encoding.UTF8.GetString(message.Body);
        CheckoutHeaderDto checkoutHeaderDto = JsonConvert.DeserializeObject<CheckoutHeaderDto>(body);

        OrderHeader orderHeader = new()
        {
            UserId = checkoutHeaderDto.UserId,
            FirstName = checkoutHeaderDto.FirstName,
            LastName = checkoutHeaderDto.LastName,
            OrderDetails = new List<OrderDetails>(),
            CardNumber = checkoutHeaderDto.CardNumber,
            CouponCode = checkoutHeaderDto.CouponCode,
            CVV = checkoutHeaderDto.CVV,
            DiscountTotal = checkoutHeaderDto.DiscountTotal,
            Email = checkoutHeaderDto.Email,
            ExpiryMonthYear = checkoutHeaderDto.ExpiryMonthYear,
            OrderTime = DateTime.Now,
            OrderTotal = checkoutHeaderDto.OrderTotal,
            PaymentStatus = false,
            Phone = checkoutHeaderDto.Phone,
            PickupDateTime = checkoutHeaderDto.PickupDateTime
        };

        foreach(var details in checkoutHeaderDto.CartDetails)
        {
            OrderDetails orderDetails = new()
            {
                ProductId = details.ProductId,
                ProductName = details.Product.Name,
                Price = details.Product.Price,
                Count = details.Count
            };
            orderHeader.CartTotalItems += details.Count;
            orderHeader.OrderDetails.Add(orderDetails);
        }
        await _orderRepository.AddOrder(orderHeader);
    }
}
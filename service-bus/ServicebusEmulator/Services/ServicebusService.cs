using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ServicebusEmulator.Services;

public class ServicebusService : IServicebusService
{
    private readonly ILogger<ServicebusService> _logger;
    private static string _connectionString = "Endpoint=sb://127.0.0.1;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

    public ServicebusService(ILogger<ServicebusService> logger)
    {
        _logger = logger;
    }

    public async Task PublishMessageToQueue(string queueName, string messageBody)
    {
        var client = new ServiceBusClient(_connectionString);
        var sender = client.CreateSender(queueName);
        
        using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
        messageBatch.TryAddMessage(new ServiceBusMessage()
        {
            Body = new(messageBody)
        });
        
        await sender.SendMessagesAsync(messageBatch);
        await sender.DisposeAsync();
        await client.DisposeAsync();

        _logger.LogInformation("Message has been published to the queue.");
    }

    public async Task ConsumeMessageFromQueue(string queueName)
    {
        var client = new ServiceBusClient(_connectionString);

        ServiceBusReceiverOptions opt = new ServiceBusReceiverOptions
        {
            ReceiveMode = ServiceBusReceiveMode.PeekLock,
        };

        ServiceBusReceiver receiver = client.CreateReceiver(queueName, opt);

        while (true)
        {
            ServiceBusReceivedMessage message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(5));
            if (message != null)
            {
                // Process the message
                _logger.LogInformation($"Received message: {Encoding.UTF8.GetString(message.Body)}");

                // Complete the message to remove it from the queue
                await receiver.CompleteMessageAsync(message);
            }
            else
            {
                _logger.LogInformation("No messages received.");
                break;
            }
        }

        _logger.LogInformation("Done receiving.");

        await receiver.DisposeAsync();
        await client.DisposeAsync();
    }

    public async Task PublishMessageToTopic(string topicName, string messageBody)
    {
        await using (var client = new ServiceBusClient(_connectionString))
        {
            ServiceBusSender sender = client.CreateSender(topicName);

            ServiceBusMessage message = new ServiceBusMessage()
            {
                Body = new(messageBody)
            };

            await sender.SendMessageAsync(message);
        }

        _logger.LogInformation("Sent the message to the topic.");
    }

    public async Task ConsumeMessageFromSubscription(string topicName, string subscriptionName)
    {
        Console.WriteLine($"Rcv_Sub {subscriptionName} Begin");

        var client1 = new ServiceBusClient(_connectionString);
        var opt1 = new ServiceBusProcessorOptions();
        opt1.ReceiveMode = ServiceBusReceiveMode.PeekLock;
        var processor1 = client1.CreateProcessor(topicName, subscriptionName, opt1);

        processor1.ProcessMessageAsync += MessageHandler;
        processor1.ProcessErrorAsync += ErrorHandler;

        await processor1.StartProcessingAsync();

        await Task.Delay(TimeSpan.FromSeconds(5));

        await processor1.StopProcessingAsync();
        await processor1.DisposeAsync();
        await client1.DisposeAsync();
        _logger.LogInformation($"Rcv_Sub {subscriptionName} End");
    }

    public async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string body = args.Message.Body.ToString();
        _logger.LogInformation($"Received message: SequenceNumber:{args.Message.SequenceNumber} Body:{body} To:{args.Message.To}");
        await args.CompleteMessageAsync(args.Message);
    }

    public Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError($"Message handler encountered an exception {args.Exception}.");
        return Task.CompletedTask;
    }
}

using Azure.Messaging.ServiceBus;

namespace ServicebusEmulator.Services;

public interface IServicebusService
{
    Task PublishMessageToQueue(string queueName, string messageBody);

    Task ConsumeMessageFromQueue(string queueName);

    Task PublishMessageToTopic(string topicName, string messageBody);

    Task ConsumeMessageFromSubscription(string topicName, string subscriptionName);

    Task MessageHandler(ProcessMessageEventArgs args);

    Task ErrorHandler(ProcessErrorEventArgs args);
}

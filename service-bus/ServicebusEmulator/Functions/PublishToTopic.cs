using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using ServicebusEmulator.Services;

namespace ServicebusEmulator.Functions;

public class PublishToTopic
{
    private readonly ILogger<PublishToTopic> _logger;
    private readonly IServicebusService _servicebusService;

    public PublishToTopic(ILogger<PublishToTopic> logger, IServicebusService servicebusService)
    {
        _logger = logger;
        _servicebusService = servicebusService;
    }

    [Function("publishToTopic")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            if (!req.Headers.TryGetValue("Topic-Name", out var topicName))
            {
                return new BadRequestObjectResult("Missing \"Topic-Name\" in header.");
            }

            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var messageBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(messageBody))
            {
                return new BadRequestObjectResult("Missing message content.");
            }

            await _servicebusService.PublishMessageToTopic(topicName, messageBody);

            return new OkObjectResult("Published successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + "\n\n" + ex.InnerException?.Message);

            return new BadRequestObjectResult("Missing \"Topic-Name\" in header.");
        }
    }
}
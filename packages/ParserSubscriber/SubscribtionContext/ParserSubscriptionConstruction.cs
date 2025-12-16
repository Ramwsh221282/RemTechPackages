using System.Text;
using System.Text.Json;
using RabbitMQ.Client.Events;

namespace ParserSubscriber.SubscribtionContext;

internal static class ParserSubscriptionConstruction
{
    extension(ParserSubscribtion)
    {
        internal static ParserSubscribtion FromEventArgs(BasicDeliverEventArgs ea)
        {
            byte[] payload = ea.Body.ToArray();
            string json = Encoding.UTF8.GetString(payload);
            using JsonDocument document = JsonDocument.Parse(json);
            Guid parserId = document.RootElement.GetProperty("parser_id").GetGuid();
            string parserDomain = document.RootElement.GetProperty("parser_domain").GetString()!;
            string parserType = document.RootElement.GetProperty("parser_type").GetString()!;
            return new ParserSubscribtion(
                parserId,
                parserDomain,
                parserType,
                DateTime.UtcNow
            );
        }
    }
}
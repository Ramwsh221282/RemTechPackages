namespace ParserSubscriber.Subscribers.RabbitMq;

public sealed class RabbitMqRequestReplyResponseListeningQueueOptions
{
    public string ResponseQueue { get; set; } = string.Empty;
    public string ResponseExchange { get; set; } = string.Empty;
    public string ResponseRoutingKey { get; set; } = string.Empty;
    public string SubscribtionParserDomain { get; set; } = string.Empty;
    public string SubscribtionParserType { get; set; } = string.Empty;
    public string RequestQueue { get; set; } = string.Empty;
    public string RequestExchange { get; set; } = string.Empty;
    public string RequestRoutingKey { get; set; } = string.Empty;

    internal void ResponseQueueInfo(out string queue, out string exchange, out string routingKey)
    {
        queue = ResponseQueue;
        exchange = ResponseExchange;
        routingKey = ResponseRoutingKey;
    }

    internal void RequestQueueInfo(out string queue, out string exchange, out string routingKey)
    {
        queue = RequestQueue;
        exchange = RequestExchange;
        routingKey = RequestRoutingKey;
    }

    internal void ParserInfo(out string domain, out string type)
    {
        domain = SubscribtionParserDomain;
        type = SubscribtionParserType;
    }
}

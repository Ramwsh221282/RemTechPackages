using RabbitMQ.Client.Events;

namespace ParsingSDK.ParserInvokingContext;

public delegate Task ParserStartQueueHandle(BasicDeliverEventArgs ea);
using Confluent.Kafka;

using System.Text;

namespace KafkaResilientEvent;

public class ConsumeContext<TMessage> : Message<Ignore, TMessage> where TMessage : IMessage
{
    public const string EVENT_TYPE_KEY = "event-type";
    public const string RETRY_COUNT_KEY = "retry-count";

    public ConsumeContext(Message<Ignore, TMessage> message)
    {
        Key = message.Key;
        Value = message.Value;
    }

    public ConsumeContext(Ignore key, TMessage value)
    {
        Key = key;
        Value = value;
    }

    public int GetRetryAttempt()
    {
        var header = Headers.FirstOrDefault(h => h.Key == RETRY_COUNT_KEY);
        return header == null ? 0 : int.Parse(Encoding.UTF8.GetString(header.GetValueBytes()));
    }

    public string GetEventType()
    {
        var header = Headers.FirstOrDefault(h => h.Key == EVENT_TYPE_KEY);
        return header == null ? string.Empty : Encoding.UTF8.GetString(header.GetValueBytes());
    }

    public static string GetEventType(Headers headers)
    {
        var header = headers.FirstOrDefault(h => h.Key == EVENT_TYPE_KEY);
        return header == null ? string.Empty : Encoding.UTF8.GetString(header.GetValueBytes());
    }

    public TMessage Message => Value;
}

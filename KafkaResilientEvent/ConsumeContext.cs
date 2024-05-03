using Confluent.Kafka;

using MediatR;

using System.Text;

namespace KafkaResilientEvent;

public class ConsumeContext<TKey, TValue> : Message<TKey, TValue>, IRequest
{
    public const string EVENT_TYPE_KEY = "event-type";
    public const string RETRY_COUNT_KEY = "retry-count";

    public ConsumeContext(Message<TKey, TValue> message)
    {
        Key = message.Key;
        Value = message.Value;
        Headers = message.Headers;
    }

    public ConsumeContext(TKey key, TValue value, Headers headers)
    {
        Key = key;
        Value = value;
        Headers = headers;
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

    public TValue Message => Value;
}

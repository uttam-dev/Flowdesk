using System.Text.Json;
using System.Text.Json.Serialization;
using FlowDesk.Domain.Utils;

namespace FlowDesk.Api.Middleware
{

    public class UtcToIstJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetDateTime(); // keep incoming as-is
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var ist = DateTimeConverter.ToIst(value);
            writer.WriteStringValue(ist.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}

using CQRS.Core.Events;
using MongoDB.Bson.Serialization;
using Post.Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Post.Query.Infrastructure.Converters
{
    public class EventJsonConverter : System.Text.Json.Serialization.JsonConverter<BaseEvent>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsAssignableFrom(typeof(BaseEvent));
        }

        public override BaseEvent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (!JsonDocument.TryParseValue(ref reader, out var doc))
            {
                throw new JsonException("Failed to parse" + nameof(JsonDocument));
            }

            if (!doc.RootElement.TryGetProperty("Type", out var type)) 
            {
                throw new JsonException($"Unexpected type {type}");
            }

            var typeDescriminator = type.GetString();
            var json = doc.RootElement.GetRawText();

            return typeDescriminator switch
            {
                nameof(PostCreatedEvent) => JsonSerializer.Deserialize<PostCreatedEvent>(json, options),
                nameof(MessageUpdatedEvent) => JsonSerializer.Deserialize<MessageUpdatedEvent>(json, options),
                nameof(PostLikedEvent) => JsonSerializer.Deserialize<PostLikedEvent>(json, options),
                nameof(CommentAddedEvent) => JsonSerializer.Deserialize<CommentAddedEvent>(json, options),
                nameof(CommentUpdatedEvent) => JsonSerializer.Deserialize<CommentUpdatedEvent>(json, options),
                nameof(CommentRemovedEvent) => JsonSerializer.Deserialize<CommentRemovedEvent>(json, options),
                nameof(PostRemovedEvent) => JsonSerializer.Deserialize<PostRemovedEvent>(json, options),
                _ => throw new JsonException($"{typeDescriminator} is not supported")
            };
        }

        public override void Write(Utf8JsonWriter writer, BaseEvent value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}

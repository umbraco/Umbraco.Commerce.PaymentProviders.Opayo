using System.IO;
using System.Text.Json;
using Flurl.Http.Configuration;

namespace Umbraco.Commerce.PaymentProviders.Opayo.Api;

internal class CustomFlurlJsonSerializer : ISerializer
{
    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomFlurlJsonSerializer"/> class.
    /// </summary>
    /// <param name="settings">Settings to control (de)serialization behavior.</param>
    public CustomFlurlJsonSerializer(JsonSerializerOptions settings = null)
    {
        _serializerOptions = settings;
    }

    /// <summary>
    /// Serializes the specified object to a JSON string.
    /// </summary>
    /// <param name="obj">The object to serialize.</param>
    public string Serialize(object obj) => JsonSerializer.Serialize(obj, _serializerOptions);

    /// <summary>
    /// Deserializes the specified JSON string to an object of type T.
    /// </summary>
    /// <param name="s">The JSON string to deserialize.</param>
    public T Deserialize<T>(string s) => JsonSerializer.Deserialize<T>(s, _serializerOptions);

    /// <summary>
    /// Deserializes the specified stream to an object of type T.
    /// </summary>
    /// <param name="stream">The stream to deserialize.</param>
    public T Deserialize<T>(Stream stream)
    {
        return JsonSerializer.Deserialize<T>(stream, _serializerOptions);
    }
}

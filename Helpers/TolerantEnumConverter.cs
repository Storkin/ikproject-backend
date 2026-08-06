using System.Text.Json;
using System.Text.Json.Serialization;

namespace IkProjesi.Helpers;

// Enum alanlari JSON'da metin olarak gidip gelir (JsonStringEnumConverter gibi).
// Farki: frontend alani bos ("" veya null) gonderdiginde hata vermek yerine
// enum'un varsayilan degerini kullanir. Boylece form doldurulmadan gonderilen
// bos alanlar 400 hatasina yol acmaz.
public class TolerantEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            int numericValue = reader.GetInt32();
            return (TEnum)Enum.ToObject(typeof(TEnum), numericValue);
        }

        string? text = reader.GetString();

        if (string.IsNullOrWhiteSpace(text))
        {
            return default;
        }

        TEnum parsed;
        if (Enum.TryParse<TEnum>(text, true, out parsed))
        {
            return parsed;
        }

        throw new JsonException("'" + text + "' geçerli bir " + typeof(TEnum).Name + " değeri değil.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

public class TolerantEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converterType = typeof(TolerantEnumConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

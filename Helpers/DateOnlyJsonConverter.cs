using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IkProjesi.Helpers;

// Tarihler JSON'da saatsiz gidip gelir: "2026-02-01"
// Frontend alani bos ("") gonderdiginde hata vermek yerine
// varsayilan deger / null kullanilir.
public class DateOnlyJsonConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-dd";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        string? text = reader.GetString();

        if (string.IsNullOrWhiteSpace(text))
        {
            return default;
        }

        return DateTime.Parse(text, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}

public class NullableDateOnlyJsonConverter : JsonConverter<DateTime?>
{
    private const string Format = "yyyy-MM-dd";

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        string? text = reader.GetString();

        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return DateTime.Parse(text, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value.ToString(Format, CultureInfo.InvariantCulture));
    }
}

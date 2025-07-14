using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class DateOnlyJsonConverter : JsonConverter<DateTime>
{
    private readonly string _format = "yyyy-MM-dd";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Para lectura: parsear string a DateTime
        return DateTime.Parse(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Para escritura: formatear DateTime a "yyyy-MM-dd"
        writer.WriteStringValue(value.ToString(_format));
    }
}

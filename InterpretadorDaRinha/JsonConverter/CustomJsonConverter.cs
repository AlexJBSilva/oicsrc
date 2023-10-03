using System.Text.Json;
using System.Text.Json.Serialization;
using InterpretadorDaRinha.RinhaNodes;

namespace InterpretadorDaRinha.CustomJsonConverter;

public class TermConverter : JsonConverter<Term>
{
    public override bool CanConvert(Type typeToConvert) =>
        typeof(Term).IsAssignableFrom(typeToConvert);
    public override Term Read(
                ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DeserializeTerm(ref reader, options);
    }

    public static Term DeserializeTerm(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        Utf8JsonReader readerClone = reader;

        if (readerClone.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("TokenType not StartObject.");
        }

        readerClone.Read();
        if (readerClone.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException("TokenType not PropertyName.");
        }
        string? propertyName = readerClone.GetString();
        if (propertyName != "kind")
        {
            throw new JsonException("PropertyName not 'Kind'.");
        }

        readerClone.Read();
        if (readerClone.TokenType != JsonTokenType.String)
        {
            throw new JsonException("TokenType not String.");
        }

        string kind = readerClone.GetString();
        Term term = kind switch
        {
            "Int" => JsonSerializer.Deserialize<Int>(ref reader, options)!,
            "Str" => JsonSerializer.Deserialize<Str>(ref reader, options)!,
            "Call" => JsonSerializer.Deserialize<Call>(ref reader, options)!,
            "Binary" => JsonSerializer.Deserialize<Binary>(ref reader, options)!,
            "Function" => JsonSerializer.Deserialize<Function>(ref reader, options)!,
            "Let" => JsonSerializer.Deserialize<Let>(ref reader, options)!,
            "If" => JsonSerializer.Deserialize<If>(ref reader, options)!,
            "Print" => JsonSerializer.Deserialize<Print>(ref reader, options)!,
            "First" => JsonSerializer.Deserialize<First>(ref reader, options)!,
            "Second" => JsonSerializer.Deserialize<Second>(ref reader, options)!,
            "Bool" => JsonSerializer.Deserialize<Bool>(ref reader, options)!,
            "Tuple" => JsonSerializer.Deserialize<TupleRinha>(ref reader, options)!,
            "Var" => JsonSerializer.Deserialize<Var>(ref reader, options)!,
            _ => throw new JsonException()
        };
        return term;
    }

    public override void Write(
                Utf8JsonWriter writer, Term term, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, term, term.GetType(), options);
}

public class ArrayConverter : JsonConverter<Term[]>
{
    public override bool CanConvert(Type typeToConvert) =>
        typeof(Term[]).IsAssignableFrom(typeToConvert);

    public override Term[] Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("TokenType not StartArray.");
        }

        List<Term> list = new();

        reader.Read();

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            list.Add(TermConverter.DeserializeTerm(ref reader, options));

            reader.Read();
        }

        return list.ToArray();
    }

    public override void Write(Utf8JsonWriter writer, Term[] value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}


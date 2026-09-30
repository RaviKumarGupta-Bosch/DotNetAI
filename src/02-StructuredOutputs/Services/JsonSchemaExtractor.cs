using System.Reflection;
using System.Text.Json.Serialization;

namespace DotNetAI.StructuredOutputs.Services;

/// <summary>
/// Generates JSON schema descriptions from C# types for prompting LLMs to return strict JSON structures.
/// </summary>
public static class JsonSchemaExtractor
{
    public static string GenerateSchemaDescription<T>()
    {
        var type = typeof(T);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var schemaFields = new List<string>();

        foreach (var prop in properties)
        {
            var jsonProp = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
            var name = jsonProp?.Name ?? prop.Name;
            var typeName = GetFriendlyTypeName(prop.PropertyType);
            schemaFields.Add($"\"{name}\": ({typeName})");
        }

        return "{\n  " + string.Join(",\n  ", schemaFields) + "\n}";
    }

    private static string GetFriendlyTypeName(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int) || type == typeof(long)) return "integer";
        if (type == typeof(decimal) || type == typeof(double) || type == typeof(float)) return "number";
        if (type == typeof(bool)) return "boolean";
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var itemType = type.GetGenericArguments()[0];
            return $"array of {itemType.Name}";
        }
        return type.Name;
    }
}

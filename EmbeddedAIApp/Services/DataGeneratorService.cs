using Bogus;
using EmbeddedAIApp.Models;
using System.Text;
using System.Text.Json;

namespace EmbeddedAIApp.Services;

/// <summary>
/// Implementation of data generation service using Bogus
/// </summary>
public class DataGeneratorService : IDataGeneratorService
{
    private readonly ILogger<DataGeneratorService> _logger;
    private readonly IAIService _aiService;
    private int _autoIncrementCounter = 1;

    public DataGeneratorService(ILogger<DataGeneratorService> logger, IAIService aiService)
    {
        _logger = logger;
        _aiService = aiService;
    }

    public async Task<List<Dictionary<string, object>>> GenerateDataAsync(StructureDefinition structure, int count)
    {
        _logger.LogInformation("Generating {Count} records for entity: {EntityName}", count, structure.EntityName);

        var faker = new Faker();
        var records = new List<Dictionary<string, object>>();

        _autoIncrementCounter = 1;

        for (int i = 0; i < count; i++)
        {
            var record = new Dictionary<string, object>();
            await GenerateFieldsAsync(structure.Fields, record, faker);
            records.Add(record);
        }

        _logger.LogInformation("Successfully generated {Count} records", count);
        return records;
    }

    private async Task GenerateFieldsAsync(List<FieldDefinition> fields, Dictionary<string, object> record, Faker faker)
    {
        foreach (var field in fields)
        {
            var value = await GenerateFieldValueAsync(field, faker);
            record[field.Name] = value;
        }
    }

    private async Task<object> GenerateFieldValueAsync(FieldDefinition field, Faker faker)
    {
        // Check for auto-increment constraint
        if (field.Constraints?.ContainsKey("autoIncrement") == true && 
            Convert.ToBoolean(field.Constraints["autoIncrement"]))
        {
            return _autoIncrementCounter++;
        }

        // Handle nested objects
        if (field.Type.Equals("object", StringComparison.OrdinalIgnoreCase) && field.Fields != null)
        {
            var nestedObject = new Dictionary<string, object>();
            await GenerateFieldsAsync(field.Fields, nestedObject, faker);
            return nestedObject;
        }

        // Generate value based on type
        return field.Type.ToLower() switch
        {
            "string" => faker.Lorem.Word(),
            "int" or "integer" => faker.Random.Int(1, 1000),
            "decimal" or "double" or "float" => faker.Random.Decimal(0, 10000),
            "boolean" or "bool" => faker.Random.Bool(),
            "date" => faker.Date.Past(10).ToString("yyyy-MM-dd"),
            "datetime" => faker.Date.Past(10).ToString("yyyy-MM-ddTHH:mm:ss"),
            "email" => faker.Internet.Email(),
            "phone" => faker.Phone.PhoneNumber(),
            "address" => faker.Address.FullAddress(),
            "street" => faker.Address.StreetAddress(),
            "city" => faker.Address.City(),
            "state" => faker.Address.State(),
            "zipcode" or "zip" => faker.Address.ZipCode(),
            "country" => faker.Address.Country(),
            "firstname" or "first_name" => faker.Name.FirstName(),
            "lastname" or "last_name" => faker.Name.LastName(),
            "fullname" or "full_name" or "name" => faker.Name.FullName(),
            "company" => faker.Company.CompanyName(),
            "jobtitle" or "job_title" => faker.Name.JobTitle(),
            "url" or "website" => faker.Internet.Url(),
            "uuid" or "guid" => Guid.NewGuid().ToString(),
            "ipaddress" or "ip" => faker.Internet.Ip(),
            "username" => faker.Internet.UserName(),
            "password" => faker.Internet.Password(),
            "lorem" or "text" => faker.Lorem.Sentence(),
            "paragraph" => faker.Lorem.Paragraph(),
            "product" => faker.Commerce.ProductName(),
            "price" => faker.Commerce.Price(),
            "department" => faker.Commerce.Department(),
            "color" => faker.Commerce.Color(),
            "creditcard" or "credit_card" => faker.Finance.CreditCardNumber(),
            "currency" => faker.Finance.Currency().Code,
            "iban" => faker.Finance.Iban(),
            "bic" => faker.Finance.Bic(),
            _ => await HandleUnknownTypeAsync(field, faker)
        };
    }

    private async Task<object> HandleUnknownTypeAsync(FieldDefinition field, Faker faker)
    {
        _logger.LogWarning("Unknown field type: {Type} for field: {Name}. Using AI suggestion or default.", 
            field.Type, field.Name);

        try
        {
            // Try to get AI suggestion for the type
            var suggestedType = await _aiService.SuggestDataTypeAsync(field.Name, field.Type);
            
            // Retry with suggested type
            if (!string.IsNullOrEmpty(suggestedType) && suggestedType != field.Type)
            {
                var tempField = new FieldDefinition 
                { 
                    Name = field.Name, 
                    Type = suggestedType,
                    Constraints = field.Constraints 
                };
                return await GenerateFieldValueAsync(tempField, faker);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI suggestion for unknown type");
        }

        // Default fallback
        return faker.Lorem.Word();
    }

    public string ConvertToCsv(List<Dictionary<string, object>> data)
    {
        if (data == null || data.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();

        // Get all unique keys from all records (handles inconsistent structures)
        var allKeys = data.SelectMany(d => d.Keys).Distinct().OrderBy(k => k).ToList();

        // Write header
        sb.AppendLine(string.Join(",", allKeys.Select(k => $"\"{k}\"")));

        // Write data rows
        foreach (var record in data)
        {
            var values = allKeys.Select(key =>
            {
                if (record.TryGetValue(key, out var value))
                {
                    return FormatCsvValue(value);
                }
                return "\"\"";
            });

            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }

    private string FormatCsvValue(object value)
    {
        if (value == null)
        {
            return "\"\"";
        }

        // Handle nested objects by serializing to JSON
        if (value is Dictionary<string, object>)
        {
            var json = JsonSerializer.Serialize(value);
            return $"\"{json.Replace("\"", "\"\"")}\"";
        }

        var stringValue = value.ToString() ?? "";
        
        // Escape quotes and wrap in quotes if contains comma, quote, or newline
        if (stringValue.Contains(',') || stringValue.Contains('"') || stringValue.Contains('\n'))
        {
            return $"\"{stringValue.Replace("\"", "\"\"")}\"";
        }

        return $"\"{stringValue}\"";
    }

    public string ConvertToSql(List<Dictionary<string, object>> data, string tableName)
    {
        if (data == null || data.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        var allKeys = data.SelectMany(d => d.Keys).Distinct().OrderBy(k => k).ToList();

        foreach (var record in data)
        {
            var columns = string.Join(", ", allKeys);
            var values = allKeys.Select(key =>
            {
                if (record.TryGetValue(key, out var value))
                {
                    return FormatSqlValue(value);
                }
                return "NULL";
            });

            sb.AppendLine($"INSERT INTO {tableName} ({columns}) VALUES ({string.Join(", ", values)});");
        }

        return sb.ToString();
    }

    private string FormatSqlValue(object value)
    {
        if (value == null)
        {
            return "NULL";
        }

        // Handle nested objects
        if (value is Dictionary<string, object>)
        {
            var json = JsonSerializer.Serialize(value);
            return $"'{json.Replace("'", "''")}'";
        }

        // Handle different types
        return value switch
        {
            bool b => b ? "1" : "0",
            string s => $"'{s.Replace("'", "''")}'",
            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
            _ => value.ToString() ?? "NULL"
        };
    }
}

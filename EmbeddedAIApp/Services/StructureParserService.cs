using EmbeddedAIApp.Models;

namespace EmbeddedAIApp.Services;

/// <summary>
/// Implementation of structure parser service
/// </summary>
public class StructureParserService : IStructureParserService
{
    private readonly ILogger<StructureParserService> _logger;
    private static readonly HashSet<string> SupportedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "string", "int", "integer", "decimal", "double", "float",
        "boolean", "bool", "date", "datetime",
        "email", "phone", "address", "street", "city", "state", "zipcode", "zip", "country",
        "firstname", "first_name", "lastname", "last_name", "fullname", "full_name", "name",
        "company", "jobtitle", "job_title",
        "url", "website", "uuid", "guid",
        "ipaddress", "ip", "username", "password",
        "lorem", "text", "paragraph",
        "product", "price", "department", "color",
        "creditcard", "credit_card", "currency", "iban", "bic",
        "object"
    };

    public StructureParserService(ILogger<StructureParserService> logger)
    {
        _logger = logger;
    }

    public (bool isValid, List<string> errors) ValidateStructure(StructureDefinition structure)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(structure.EntityName))
        {
            errors.Add("Entity name is required");
        }

        if (structure.Fields == null || structure.Fields.Count == 0)
        {
            errors.Add("At least one field is required");
        }
        else
        {
            ValidateFields(structure.Fields, errors, "");
        }

        var isValid = errors.Count == 0;
        
        if (isValid)
        {
            _logger.LogInformation("Structure validation passed for entity: {EntityName}", structure.EntityName);
        }
        else
        {
            _logger.LogWarning("Structure validation failed for entity: {EntityName}. Errors: {ErrorCount}", 
                structure.EntityName, errors.Count);
        }

        return (isValid, errors);
    }

    private void ValidateFields(List<FieldDefinition> fields, List<string> errors, string path)
    {
        var fieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in fields)
        {
            var fieldPath = string.IsNullOrEmpty(path) ? field.Name : $"{path}.{field.Name}";

            if (string.IsNullOrWhiteSpace(field.Name))
            {
                errors.Add($"Field name is required at path: {path}");
                continue;
            }

            // Check for duplicate field names
            if (fieldNames.Contains(field.Name))
            {
                errors.Add($"Duplicate field name '{field.Name}' at path: {path}");
            }
            else
            {
                fieldNames.Add(field.Name);
            }

            if (string.IsNullOrWhiteSpace(field.Type))
            {
                errors.Add($"Field type is required for field: {fieldPath}");
            }
            else if (!SupportedTypes.Contains(field.Type))
            {
                _logger.LogWarning("Unknown field type '{Type}' for field: {FieldPath}. Will attempt AI suggestion.", 
                    field.Type, fieldPath);
                // Don't add to errors - allow AI to handle unknown types
            }

            // Validate nested objects
            if (field.Type.Equals("object", StringComparison.OrdinalIgnoreCase))
            {
                if (field.Fields == null || field.Fields.Count == 0)
                {
                    errors.Add($"Object field '{fieldPath}' must have nested fields defined");
                }
                else
                {
                    ValidateFields(field.Fields, errors, fieldPath);
                }
            }
        }
    }

    public List<string> GetSupportedFieldTypes()
    {
        return SupportedTypes.OrderBy(t => t).ToList();
    }
}

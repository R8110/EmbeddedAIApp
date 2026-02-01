# Sample Data Generator with Offline AI

A powerful sample data generator that outputs any amount of sample records based upon a supplied structure using offline AI.

## Prerequisites

### 1. Install .NET 8.0 SDK
Download and install from: https://dotnet.microsoft.com/download/dotnet/8.0

### 2. Install Ollama
Ollama is required for AI-powered features.

**macOS:**
```bash
brew install ollama
```

**Linux:**
```bash
curl https://ollama.ai/install.sh | sh
```

**Windows:**
Download from: https://ollama.ai/download

### 3. Pull the LLM Model
After installing Ollama, pull the default model:
```bash
ollama pull llama2
```

Or use an alternative model like Mistral:
```bash
ollama pull mistral
```

Update the model name in `appsettings.json` if using a different model.

## Getting Started

### 1. Clone and Build
```bash
git clone <repository-url>
cd EmbeddedAIApp/EmbeddedAIApp
dotnet build
```

### 2. Start Ollama Server
In a separate terminal:
```bash
ollama serve
```

The server will run on `http://localhost:11434` by default.

### 3. Run the Application
```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5153`
- HTTPS: `https://localhost:7126`
- Swagger UI: `http://localhost:5153` (root path)

## Tech Stack

- **ASP.NET Core 8.0** - Web API framework
- **C# 12** - Programming language
- **Ollama** - Local LLM execution for AI features
- **OllamaSharp 5.4.16** - C# client library for Ollama
- **Bogus 35.6.5** - Realistic fake data generation
- **Serilog 10.0** - Structured logging
- **Swagger/OpenAPI** - API documentation

## API Endpoints

### 1. Generate Sample Data
Generate sample data based on a structure definition.

**Endpoint:** `POST /api/data/generate`

**Request Body:**
```json
{
  "structure": {
    "entityName": "Customer",
    "fields": [
      { "name": "Id", "type": "int", "constraints": { "autoIncrement": true } },
      { "name": "FirstName", "type": "string" },
      { "name": "LastName", "type": "string" },
      { "name": "Email", "type": "email" },
      { "name": "Phone", "type": "phone" },
      { "name": "BirthDate", "type": "date" }
    ]
  },
  "count": 10,
  "format": "json"
}
```

**Supported Formats:**
- `json` - JSON array of objects (default)
- `csv` - Comma-separated values with headers
- `sql` - SQL INSERT statements

### 2. Validate Structure
Validate a structure definition with AI-powered suggestions.

**Endpoint:** `POST /api/data/validate-structure`

**Request Body:**
```json
{
  "structure": {
    "entityName": "Product",
    "fields": [
      { "name": "Id", "type": "uuid" },
      { "name": "Name", "type": "string" },
      { "name": "Price", "type": "decimal" }
    ]
  }
}
```

### 3. Generate Structure from Description (AI-Powered)
Generate a structure definition from natural language description.

**Endpoint:** `POST /api/data/generate-structure`

**Request Body:**
```json
{
  "description": "I need a customer database with contact info and purchase history"
}
```

### 4. Get Supported Types
Get a list of all supported field types.

**Endpoint:** `GET /api/data/supported-types`

### 5. Health Check
Check API and Ollama connectivity status.

**Endpoint:** `GET /api/health`

**Response:**
```json
{
  "status": "healthy",
  "ollamaConnected": true,
  "ollamaModel": "llama2"
}
```

## Supported Field Types

### Basic Types
- `string`, `int`, `integer`, `decimal`, `double`, `float`
- `boolean`, `bool`
- `date`, `datetime`

### Contact Information
- `email`, `phone`
- `address`, `street`, `city`, `state`, `zipcode`, `country`

### Personal Information
- `firstname`, `lastname`, `fullname`, `name`

### Business
- `company`, `jobtitle`

### Internet
- `url`, `website`, `ipaddress`, `username`, `password`

### Identifiers
- `uuid`, `guid`

### Text
- `lorem`, `text`, `paragraph`

### Commerce
- `product`, `price`, `department`, `color`

### Financial
- `creditcard`, `currency`, `iban`, `bic`

### Complex Types
- `object` - Nested objects (requires `fields` array)

## Example Structure Definitions

### Simple Customer
```json
{
  "entityName": "Customer",
  "fields": [
    { "name": "Id", "type": "int", "constraints": { "autoIncrement": true } },
    { "name": "FirstName", "type": "firstname" },
    { "name": "LastName", "type": "lastname" },
    { "name": "Email", "type": "email" },
    { "name": "Phone", "type": "phone" }
  ]
}
```

### Customer with Nested Address
```json
{
  "entityName": "Customer",
  "fields": [
    { "name": "Id", "type": "uuid" },
    { "name": "Name", "type": "fullname" },
    { "name": "Email", "type": "email" },
    { "name": "Address", "type": "object", 
      "fields": [
        { "name": "Street", "type": "street" },
        { "name": "City", "type": "city" },
        { "name": "State", "type": "state" },
        { "name": "ZipCode", "type": "zipcode" }
      ]
    }
  ]
}
```

### Product Catalog
```json
{
  "entityName": "Product",
  "fields": [
    { "name": "Id", "type": "uuid" },
    { "name": "Name", "type": "product" },
    { "name": "Description", "type": "paragraph" },
    { "name": "Price", "type": "price" },
    { "name": "Department", "type": "department" },
    { "name": "Color", "type": "color" },
    { "name": "InStock", "type": "boolean" }
  ]
}
```

## Configuration

Edit `appsettings.json` to customize settings:

```json
{
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "Model": "llama2",
    "Timeout": 30
  },
  "DataGeneration": {
    "DefaultCount": 10,
    "MaxCount": 10000
  }
}
```

### Configuration Options

- **Ollama.BaseUrl**: Ollama server URL
- **Ollama.Model**: LLM model to use (llama2, mistral, etc.)
- **Ollama.Timeout**: Request timeout in seconds
- **DataGeneration.DefaultCount**: Default number of records to generate
- **DataGeneration.MaxCount**: Maximum allowed records per request

## Features

✅ Generate realistic sample data with Bogus  
✅ AI-powered structure validation and suggestions  
✅ Natural language structure generation  
✅ Support for nested objects and complex types  
✅ Multiple output formats (JSON, CSV, SQL)  
✅ Configurable record count (1-10,000)  
✅ Graceful degradation when AI is unavailable  
✅ Structured logging with Serilog  
✅ OpenAPI/Swagger documentation  
✅ Auto-increment field support  

## Architecture

### Project Structure
```
EmbeddedAIApp/
├── Controllers/
│   ├── DataController.cs       - Data generation endpoints
│   └── HealthController.cs     - Health check endpoint
├── Models/
│   ├── OllamaSettings.cs       - Ollama configuration
│   ├── DataGenerationSettings.cs
│   ├── FieldDefinition.cs      - Field definition model
│   └── StructureDefinition.cs  - Structure definition model
├── DTOs/
│   ├── GenerateDataRequest.cs
│   ├── ValidateStructureRequest.cs
│   ├── ValidationResponse.cs
│   ├── GenerateStructureRequest.cs
│   └── HealthCheckResponse.cs
├── Services/
│   ├── IAIService.cs
│   ├── OllamaAIService.cs      - Ollama AI integration
│   ├── IDataGeneratorService.cs
│   ├── DataGeneratorService.cs  - Bogus data generation
│   ├── IStructureParserService.cs
│   └── StructureParserService.cs
├── Program.cs                   - Application entry point
└── appsettings.json            - Configuration
```

## Troubleshooting

### Ollama Connection Issues
1. Verify Ollama is running: `curl http://localhost:11434/api/version`
2. Check if the model is pulled: `ollama list`
3. Verify `appsettings.json` has correct BaseUrl

### Build Errors
1. Ensure .NET 8.0 SDK is installed: `dotnet --version`
2. Restore packages: `dotnet restore`
3. Clean and rebuild: `dotnet clean && dotnet build`

### AI Features Not Working
The application works without Ollama but with limited functionality:
- Data generation works normally
- Structure validation performs basic checks only
- Structure generation from description is unavailable

## License

This project is provided as-is for demonstration purposes.

using EmbeddedAIApp.Models;
using EmbeddedAIApp.Services;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting EmbeddedAIApp");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Configure settings
    builder.Services.Configure<OllamaSettings>(builder.Configuration.GetSection("Ollama"));
    builder.Services.Configure<DataGenerationSettings>(builder.Configuration.GetSection("DataGeneration"));

    // Register services
    builder.Services.AddSingleton<IAIService, OllamaAIService>();
    builder.Services.AddScoped<IDataGeneratorService, DataGeneratorService>();
    builder.Services.AddScoped<IStructureParserService, StructureParserService>();

    // Add controllers
    builder.Services.AddControllers();

    // Configure Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "EmbeddedAIApp - Sample Data Generator",
            Version = "v1",
            Description = "A powerful sample data generator that outputs any amount of sample records based upon a supplied structure using offline AI.",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "EmbeddedAIApp"
            }
        });

        // Include XML comments if available
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    app.UseSerilogRequestLogging();

    // Always enable Swagger for easy testing
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EmbeddedAIApp API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    Log.Information("EmbeddedAIApp started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

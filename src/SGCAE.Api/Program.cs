using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using SGCAE.Api.Infrastructure;
using SGCAE.Application;
using SGCAE.Infrastructure;
using SGCAE.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options =>
        // Las validaciones de datos obligatorios las hace el dominio, con mensajes propios.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(options =>
        options.InvalidModelStateResponseFactory = context =>
        {
            var problem = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "La solicitud contiene datos con formato inválido.",
                Detail = "Revise los campos indicados (p. ej. valores de enumeraciones, fechas o identificadores)."
            };
            return new BadRequestObjectResult(problem);
        });

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SGCAE - Sistema de Gestión y Control de Acceso de Empleados",
        Version = "v1",
        Description = "API REST para gestionar empleados, credenciales, puntos de acceso y el registro de entradas y salidas."
    });
    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    options.IncludeXmlComments(xml);
});

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(options => options.DocumentTitle = "SGCAE API");

if (app.Configuration.GetValue("Database:InitializeOnStartup", false))
    await DatabaseInitializer.InitializeAsync(app.Services);

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
app.MapControllers();

app.Run();

public partial class Program;

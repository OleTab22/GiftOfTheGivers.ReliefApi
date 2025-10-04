using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GiftOfTheGivers.ReliefApi.Swagger;

// Adds an optional 'token' query parameter to all operations so users can paste a JWT directly
public class JwtTokenQueryParameterOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();
        // Avoid duplicating if already present
        if (operation.Parameters.Any(p => p.Name == "token")) return;

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "token",
            In = ParameterLocation.Query,
            Description = "Paste JWT here if Authorization header isn't sent by Swagger",
            Required = false,
            Schema = new OpenApiSchema { Type = "string" }
        });
    }
}



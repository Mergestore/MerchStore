using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using MerchStore.WebUI.Authentication.ApiKey;

namespace MerchStore.WebUI.Infrastructure;

/// <summary>
/// Den här klassen säger åt Swagger att vissa endpoints kräver API-nyckel.
/// </summary>
public class SecurityRequirementsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Kolla att detta gäller en vanlig Controller-endpoint (inte minimal API)
        if (context.ApiDescription.ActionDescriptor.GetType().Name.Contains("ControllerActionDescriptor"))
        {
            // Hämta information om metod och klass
            var methodInfo = context.MethodInfo;
            var controllerType = methodInfo?.DeclaringType;

            if (methodInfo != null)
            {
                // Kolla om antingen metoden eller kontrollern har [Authorize]-attributet
                var hasAuthorizeAttribute = methodInfo.GetCustomAttribute<AuthorizeAttribute>() != null
                                         || controllerType?.GetCustomAttribute<AuthorizeAttribute>() != null;

                if (hasAuthorizeAttribute)
                {
                    // Om det finns [Authorize], så lägger vi till säkerhetskrav i Swagger
                    operation.Security = new List<OpenApiSecurityRequirement>
                    {
                        new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = ApiKeyAuthenticationDefaults.AuthenticationScheme // dvs "ApiKey"
                                    }
                                },
                                Array.Empty<string>()
                            }
                        }
                    };
                }
            }
        }
    }
}

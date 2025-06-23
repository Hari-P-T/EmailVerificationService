using Integrate.EmailVerification.Api.Constants;
using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Integrate.EmailVerification.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = SwaggerConstants.Title,
                Version = "v1",
                Description = SwaggerConstants.Description,
                Contact = new OpenApiContact
                {
                    Name = SwaggerConstants.SupportName,
                    Email = SwaggerConstants.SupportEmail
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues[SwaggerConstants.Controller] });
            c.DocInclusionPredicate((docName, apiDesc) => true);

            c.AddSecurityDefinition(AuthConstants.SchemeName, new OpenApiSecurityScheme
            {
                Description = AuthConstants.Description,
                Name = AuthConstants.HeaderName,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = AuthConstants.Scheme,
                BearerFormat = AuthConstants.BearerFormat
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = AuthConstants.SchemeName
                        }
                    },
                    []
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCustomSwaggerUI(this IApplicationBuilder app)
    {
        app.UseSwagger(c =>
        {
            c.RouteTemplate = SwaggerConstants.RouteTemplate;
        });

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint(SwaggerConstants.SwaggerEndpoint, "v1");
            c.RoutePrefix = SwaggerConstants.RoutePrefix;
            c.DocumentTitle = SwaggerConstants.DocumentTitle;
            c.DisplayRequestDuration();
        });

        return app;
    }
}

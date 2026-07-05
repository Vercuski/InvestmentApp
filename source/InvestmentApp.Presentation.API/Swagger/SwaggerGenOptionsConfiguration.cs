using InvestmentApp.Domain.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace InvestmentApp.Presentation.API.Swagger;

public static class SwaggerGenOptionsConfiguration
{
    public static void ApplySwaggerGenOptions(SwaggerGenOptions genOptions, IHostApplicationBuilder builder)
    {
        var serviceProvider = builder.Services.BuildServiceProvider();
        var swaggerOptions = serviceProvider.GetService<IOptions<SwaggerOptions>>()!.Value;

        genOptions.SwaggerDoc(SwaggerConstants.Version, new OpenApiInfo
        {
            Version = SwaggerConstants.Version,
            Title = SwaggerConstants.Title,
            Description = SwaggerConstants.Description,
            TermsOfService = SwaggerConstants.TermsOfService,
            Contact = new OpenApiContact
            {
                Name = SwaggerConstants.ContactName,
                Url = SwaggerConstants.ContactUrl,
                Email = SwaggerConstants.ContactEmail
            },
            License = new OpenApiLicense
            {
                Name = SwaggerConstants.SoftwareLicenseName,
                Url = SwaggerConstants.SoftwareLicenseUrl
            },
        });

        if (builder.Environment.IsProduction())
        {
            genOptions.AddServer(new OpenApiServer
            {
                Url = swaggerOptions.ServerUrl
            });
        }
        genOptions.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = SwaggerConstants.OpenApiSecuritySchemeDescription,
            Name = SwaggerConstants.OpenApiSecuritySchemeName,
            Type = SecuritySchemeType.Http,
            BearerFormat = SwaggerConstants.BearerFormat,
            Scheme = JwtBearerDefaults.AuthenticationScheme
        });
        //genOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
        //{
        //    {
        //        new OpenApiSecurityScheme
        //        {
        //            Reference = new OpenApiReference
        //            {
        //                Type=ReferenceType.SecurityScheme,
        //                Id=JwtBearerDefaults.AuthenticationScheme
        //            }
        //        },
        //        Array.Empty<string>()
        //    }
        //});
    }
}

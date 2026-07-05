using Swashbuckle.AspNetCore.SwaggerUI;

namespace InvestmentApp.Presentation.API.Swagger;

public static class SwaggerAppConfiguration
{
    public static WebApplication AddAppSwaggerConfiguration(this WebApplication application)
    {
        application.UseSwagger();
        application.UseSwaggerUI(config => SwaggerAppConfiguration.ConfigureSwaggerUIOptions(config));
        return application;
    }

    public static void ConfigureSwaggerUIOptions(SwaggerUIOptions uiOptions)
    {
        uiOptions.SwaggerEndpoint(SwaggerConstants.SwaggerEndpointUrl, SwaggerConstants.Title);
        uiOptions.DocumentTitle = SwaggerConstants.Title;
        uiOptions.DisplayRequestDuration();
        uiOptions.EnableFilter();
        uiOptions.InjectStylesheet("/swagger-ui/SwaggerStyle.css");
        uiOptions.InjectJavascript("/assets/js/docs.js");
    }
}

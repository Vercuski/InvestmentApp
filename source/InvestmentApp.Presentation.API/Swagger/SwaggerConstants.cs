namespace InvestmentApp.Presentation.API.Swagger;

public static class SwaggerConstants
{
    public const string Version = "v1";
    public const string Title = "InvestmentApp TITLE";
    public const string Description = "InvestmentApp DESCRIPTION";
    public const string ContactName = "InvestmentApp CONTACT";
    public const string ContactEmail = "InvestmentApp CONTACT EMAIL";
    public const string SoftwareLicenseName = "InvestmentApp SOFTWARE LICENSE";
    public const string SwaggerEndpointUrl = "/swagger/v1/swagger.json";
    public const string BearerFormat = "JWT";
    public const string OpenApiSecuritySchemeName = "Authorization";
    public const string OpenApiSecuritySchemeDescription = "InvestmentApp SECURITY SCHEME DEFINITION";

    public static readonly Uri TermsOfService = new("https://www.InvestmentApp.TERMSOFSERVICE.URL.com");
    public static readonly Uri ContactUrl = new("https://www.InvestmentApp.CONTACT.URL.com");
    public static readonly Uri SoftwareLicenseUrl = new("https://www.InvestmentApp.SOFTWARE.LICENSE.URL.com");
}

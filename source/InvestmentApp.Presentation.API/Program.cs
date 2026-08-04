using Hangfire;
using InvestmentApp.Application;
using InvestmentApp.Infrastructure;
using InvestmentApp.Persistence;
using InvestmentApp.Presentation.API.Swagger;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.AddApplicationRegistration();
builder.AddPersistenceRegistrations();
builder.AddInfrastructureRegistration();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => SwaggerGenOptionsConfiguration.ApplySwaggerGenOptions(options, builder));

var app = builder.Build();

app.UseHangfireDashboard();

app.AddAppSwaggerConfiguration();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();
app.AddInfrastructureApplicationRegistration();
app.UseHttpsRedirection();
await app.RunAsync();

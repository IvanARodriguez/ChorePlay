using ChorePlay.Api.Features.Auth;
using ChorePlay.Api.Features.Auth.GoogleLogin;
using ChorePlay.Api.Infrastructure.Extensions;
using DotNetEnv;
using Mediator;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
Env.Load();
builder.Configuration.AddEnvironmentVariables();

// Infrastructure (Database, Identity, Authentication, Cookies)
builder.Services.AddInfrastructure(builder.Configuration);

// Mediator
builder.Services.AddMediator(options =>
    options.ServiceLifetime = ServiceLifetime.Scoped);

// Features
builder.Services.AddAuthFeature();

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseExceptionHandler(_ => { });
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapGet("/", () => "ChorePlay API is running");
app.MapGoogleLogin();

app.Run();
using IdentityApi.Core.Interfaces;
using IdentityApi.Core.Models;
using IdentityApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            //"https://localhost:7081",  // Frontend
            //"https://localhost:7212",  // UserService
            //"https://localhost:7251",  // PostService
            "http://localhost:5000",  // Blazor Frontend
            "http://localhost:5001",  // IdentityApi
            "http://localhost:5002",  // PostService
            "http://localhost:5003"   // UserService
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ??
                    throw new InvalidOperationException("JWT Secret not configured")))
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers();
builder.Services.AddScoped<IIdentityService, IdentityService>();

builder.Services.AddHttpClient<IIdentityService, IdentityService>(client =>
{
    //client.BaseAddress = new Uri("https://localhost:7212/"); 
    //client.BaseAddress = new Uri("http://localhost:5003/");
    client.BaseAddress = new Uri("http://userservice:8080/");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IPasswordHasher<RegisterModel>, PasswordHasher<RegisterModel>>();


builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// health check for docker
builder.Services.AddHealthChecks();

var app = builder.Build();

// JWT exists?
var jwtSecret = app.Configuration["Jwt:Secret"];
var jwtIssuer = app.Configuration["Jwt:Issuer"];
var jwtAudience = app.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtSecret) ||
    string.IsNullOrEmpty(jwtIssuer) ||
    string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException(
        "JWT configuration is incomplete. Please check your appsettings.json file.");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// logs
Console.WriteLine("IdentityApi starting with configuration:");
Console.WriteLine($"JWT Issuer: {jwtIssuer}");
Console.WriteLine($"JWT Audience: {jwtAudience}");

// health check for docker
app.MapHealthChecks("/health");

app.Run();
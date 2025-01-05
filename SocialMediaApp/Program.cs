using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SaleKiosk.Infrastructure;
using System.Text;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.Mappings;
using UserService.Core.Services;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Repository;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            //"https://localhost:7081", 
            //"https://localhost:7168", 
            //"https://localhost:7251",
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

// JWT Authentication
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured")))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                Console.WriteLine($"Exception type: {context.Exception.GetType().Name}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                if (context.SecurityToken is JwtSecurityToken token && token.Claims != null)
                {
                    Console.WriteLine("Token validation succeeded.");
                    Console.WriteLine($"Token issuer: {token.Issuer}");
                    Console.WriteLine($"Token audience: {token.Audiences?.FirstOrDefault()}");

                    var claimsList = token.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                    if (claimsList.Any())
                    {
                        Console.WriteLine($"Token claims: {string.Join(", ", claimsList)}");
                    }
                }
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                if (!string.IsNullOrEmpty(context.Token))
                {
                    Console.WriteLine($"Received token: {context.Token[..Math.Min(50, context.Token.Length)]}...");
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"Challenge issued. Error: {context.Error}, Description: {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Service API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserServicee>();
builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);

// Database configuration - needed to be change to SQL Server - DONE
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));



// health check for docker
builder.Services.AddHealthChecks();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en-US" };
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseCors();

// Authentication before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Data population
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); 
    await context.Database.EnsureCreatedAsync();

    var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    dataSeeder.Seed();
}

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("UserService registered successfully.");

// health check for docker
app.MapHealthChecks("/health");

app.Run();
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PostService_module.Core.Interfaces;
using PostService_module.Core.Mappings;
using PostService_module.Core.Services;
using PostService_module.Infrastructure.Data;
using PostService_module.Infrastructure.Handlers;
using PostService_module.Infrastructure.Repository;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<DataSeeder>();

builder.Services.AddHttpClient<IUserClientService, UserClientService>(client =>
{
    //client.BaseAddress = new Uri("https://localhost:7212/"); 
    //client.BaseAddress = new Uri("http://localhost:5003/");
    client.BaseAddress = new Uri("http://userservice:8080/");
});


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Post Service API", Version = "v1" });

    // JWT Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            //"https://localhost:7081",  // Blazor - Frontend
            //"https://localhost:7168",  // IdentityApi
            //"https://localhost:7212",   // UserService
            "http://localhost:5000",  // Blazor Frontend
            "http://localhost:5001",  // IdentityApi
            "http://localhost:5002",  // PostService
            "http://localhost:5003"   // UserService
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.Configure<StaticFileSettings>(
builder.Configuration.GetSection("StaticFiles"));
builder.Services.AddAutoMapper(typeof(PostProfile).Assembly);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthenticationDelegatingHandler>();

builder.Services.AddHttpClient<IUserClientService, UserClientService>(client =>
{
    //client.BaseAddress = new Uri("https://localhost:7212/"); 
    //client.BaseAddress = new Uri("http://localhost:5003/");
    client.BaseAddress = new Uri("http://userservice:8080/");
})
.AddHttpMessageHandler<AuthenticationDelegatingHandler>();



// Get JWT settings 
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

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

builder.Services.AddAuthorization();

// Adding DbContext for Post - Needed change to SQL Server - DONE
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<PostDbContext>(options =>
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

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

//app.UseHttpsRedirection();
app.MapControllers();

// Data population
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PostDbContext>();
    await context.Database.EnsureCreatedAsync();

    var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    dataSeeder.Seed();
}

app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

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

Console.WriteLine("PostService starting with configuration:");
Console.WriteLine($"JWT Issuer: {jwtIssuer}");
Console.WriteLine($"JWT Audience: {jwtAudience}");

// health check for docker
app.MapHealthChecks("/health");

app.Run();



public class StaticFileSettings
{
    public long MaximumFileSize { get; set; }
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
};

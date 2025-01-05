using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using SocialMediaBlazorApp;
using SocialMediaBlazorApp.Services;
using Microsoft.JSInterop;
using Blazored.Toast;
using System.Net.Http.Headers;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddBlazoredToast();

// Custom auth message handler (debug needed)
builder.Services.AddScoped<CustomAuthorizationMessageHandler>();

void RegisterHttpClientWithAuthorization(string name, string baseAddress)
{
    builder.Services.AddHttpClient(name, client =>
    {
        client.BaseAddress = new Uri(baseAddress);
    })
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();
}

//RegisterHttpClientWithAuthorization("IdentityApi", "https://localhost:7168/");
//RegisterHttpClientWithAuthorization("PostService", "https://localhost:7251/");
//RegisterHttpClientWithAuthorization("UserService", "https://localhost:7212/");
RegisterHttpClientWithAuthorization("IdentityApi", "http://localhost:5001/");
RegisterHttpClientWithAuthorization("PostService", "http://localhost:5002/");
RegisterHttpClientWithAuthorization("UserService", "http://localhost:5003/");

builder.Services.AddScoped<AuthService>();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<ErrorLoggerService>();

await builder.Build().RunAsync();
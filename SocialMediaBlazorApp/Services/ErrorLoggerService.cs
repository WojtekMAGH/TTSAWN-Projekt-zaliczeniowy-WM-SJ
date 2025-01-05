using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace SocialMediaBlazorApp.Services
{
    public class ErrorLoggerService
    {
        private readonly IJSRuntime _jsRuntime;

        public ErrorLoggerService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task LogError(Exception ex)
        {
            await _jsRuntime.InvokeVoidAsync("console.error", "Error:", ex.Message);
            await _jsRuntime.InvokeVoidAsync("console.error", "Stack Trace:", ex.StackTrace);
        }
    }
}
using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Ma.Shared.Error;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Ma.Shared.HostingExtensions
{
    public static class ErrorPagesExtensions
    {
        public static void UseErrorPages(
            this IApplicationBuilder builder)
        { 
            builder.UseStatusCodePages(async context =>
            {
                if (context.HttpContext.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    context.HttpContext.Response.ContentType = MediaTypeNames.Application.Json;
                    await context.HttpContext.Response.WriteAsync(
                        JsonSerializer.Serialize(ErrorResponse.Error401));
                }

                if (context.HttpContext.Response.StatusCode == (int)HttpStatusCode.Forbidden)
                {
                    context.HttpContext.Response.ContentType = MediaTypeNames.Application.Json;
                    await context.HttpContext.Response.WriteAsync(
                        JsonSerializer.Serialize(ErrorResponse.Error403));
                }
            });
        }
    }
}
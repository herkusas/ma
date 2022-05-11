using Microsoft.AspNetCore.Builder;

namespace Masters.Shared.Middlewares;

public static class RequestCultureMiddlewareExtensions
{
    public static void UseCertificateValidation(
        this IApplicationBuilder builder)
    { 
        builder.UseMiddleware<CertificateAuthenticationMiddleware>();
    }
}
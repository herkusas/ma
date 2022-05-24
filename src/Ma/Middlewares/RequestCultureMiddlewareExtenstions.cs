using Microsoft.AspNetCore.Builder;

namespace Ma.Middlewares;

public static class RequestCultureMiddlewareExtensions
{
    public static void UseCertificateValidation(
        this IApplicationBuilder builder)
    {
        builder.UseMiddleware<CertificateAuthenticationMiddleware>();
    }
}

using Microsoft.AspNetCore.Builder;

namespace Masters.Shared.CertificateValidationMiddleware;

public static class RequestCultureMiddlewareExtensions
{
    public static void UseCertificateValidation(
        this IApplicationBuilder builder)
    { 
        builder.UseMiddleware<CertificateAuthenticationMiddleware>();
    }
}
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace Masters.Shared.Middlewares;

public class CertificateAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public CertificateAuthenticationMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    // ReSharper disable once UnusedMember.Global
    public async Task Invoke(HttpContext httpContext)
    {
        if (httpContext.User.Identity is {IsAuthenticated: true})
        {
            var proofOfPossessionClaim = httpContext.User.FindFirst("cnf")?.Value;
            if (!string.IsNullOrWhiteSpace(proofOfPossessionClaim))
            {
                var certificateAuthenticateResult = await httpContext.AuthenticateAsync(CertificateAuthenticationDefaults.AuthenticationScheme);
                if (!certificateAuthenticateResult.Succeeded)
                {
                    await httpContext.ChallengeAsync(CertificateAuthenticationDefaults.AuthenticationScheme);
                    return;
                }
                
                var clientCertificate = await httpContext.Connection.GetClientCertificateAsync();
                var certificateHash = Base64UrlTextEncoder.Encode(clientCertificate!.GetCertHash(HashAlgorithmName.SHA256));
                
                var x509CertificateSha256Thumbprint = JsonSerializer
                    .Deserialize<Dictionary<string, string>>(proofOfPossessionClaim, options: new JsonSerializerOptions())!
                    .FirstOrDefault().Value;
                
                if (string.IsNullOrWhiteSpace(x509CertificateSha256Thumbprint) ||
                    !certificateHash.Equals(x509CertificateSha256Thumbprint, StringComparison.OrdinalIgnoreCase))
                {
                    await httpContext.ChallengeAsync(JwtBearerDefaults.AuthenticationScheme);
                    return;
                }
            }
        }
        await _next(httpContext);
    }
}
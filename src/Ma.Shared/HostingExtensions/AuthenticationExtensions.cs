using IdentityModel;
using Ma.Shared.Options;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Ma.Shared.HostingExtensions
{
    public static class AuthenticationAndAuthorizationExtensions
    {
        public static void AddAuthentication(this WebApplicationBuilder builder,
            AuthOptions authenticationOptions)
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authenticationOptions.Issuer;
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidAudience = authenticationOptions.Audience,
                        ValidIssuer = authenticationOptions.Issuer
                    };
                }).AddCertificate(CertificateAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;

                    options.Events = new CertificateAuthenticationEvents
                    {
                        OnCertificateValidated = context =>
                        {
                            context.Principal = Principal.CreateFromCertificate(context.ClientCertificate, includeAllClaims: true);
                            context.Success();
                            return Task.CompletedTask;
                        }
                    };
                });
        }
    }
}
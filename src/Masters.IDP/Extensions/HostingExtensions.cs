using System.Security.Cryptography.X509Certificates;
using Serilog;

namespace Masters.IDP.Extensions
{
    internal static class HostingExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication("mTLS")
                .AddCertificate("mTLS", options
                    => options.RevocationMode = X509RevocationMode.NoCheck);

            builder.Services.AddIdentityServer(options =>
                {
                    options.MutualTls.Enabled = true;
                    options.MutualTls.ClientCertificateAuthenticationScheme = "mTLS";
                })
                .AddMutualTlsSecretValidators()
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddConfigurationStore("HOST=::1;PORT=5432;DATABASE=idp;Uid=postgres;Pwd=admin;");
            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            
            app.UseIdentityServer();

            return app;
        }
    }
}
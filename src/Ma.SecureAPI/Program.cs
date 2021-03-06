using Ma.HostingExtensions;
using Ma.Middlewares;
using Ma.Options;
using Ma.SecureAPI.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.ConfigureHttpsDefaults(httpsConnectionAdapterOptions =>
        httpsConnectionAdapterOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate);
});

var authOptions = new AuthOptions();
builder.Configuration.Bind(nameof(AuthOptions), authOptions);

builder.AddAuthentication(authOptions);

builder.AddAuthorization(new Dictionary<string, string> {{nameof(Policies.GetWeather), Policies.GetWeather}});

var app = builder.Build();

app.UseErrorPages();

app.UseHttpsRedirection();

app.UseAuthentication();

app.MapControllers();

app.UseCertificateValidation();

app.UseAuthorization();

app.Run();

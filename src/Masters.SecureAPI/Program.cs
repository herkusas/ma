using Masters.SecureAPI.Authorization;
using Masters.Shared.HostingExtensions;
using Masters.Shared.Middlewares;
using Masters.Shared.Options;
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

var authenticationOptions = new AuthOptions();
builder.Configuration.Bind(nameof(AuthOptions), authenticationOptions);

builder.AddAuthentication(authenticationOptions);

builder.AddAuthorization(policies: new Dictionary<string, string>{{nameof(Policies.GetWeather), Policies.GetWeather}});

var app = builder.Build();

app.UseErrorPages();

app.UseHttpsRedirection();

app.UseAuthentication();

app.MapControllers();

app.UseCertificateValidation();

app.UseAuthorization();

app.Run();
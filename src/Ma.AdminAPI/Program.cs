using FluentValidation.AspNetCore;
using Ma.AdminAPI.Authorization;
using Ma.Shared.HostingExtensions;
using Ma.Shared.Options;
using Masters.Storage.Contracts;
using Masters.Storage.Stores;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddFluentValidation(options =>
    options.RegisterValidatorsFromAssemblyContaining<Program>(
        includeInternalTypes: true, lifetime: ServiceLifetime.Singleton));
builder.Services.AddEndpointsApiExplorer();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.ConfigureHttpsDefaults(httpsConnectionAdapterOptions =>
        httpsConnectionAdapterOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate);
});

builder.Services.AddSingleton(builder.Configuration.GetConnectionString("Postgres"));

builder.Services.AddSingleton<IExtendedResourceStore, ExtendedResourceStore>();

builder.Services.AddSingleton<IExtendedClientStore, ExtendedClientStore>();

var authOptions = new AuthOptions();
builder.Configuration.Bind(nameof(AuthOptions), authOptions);

builder.AddAuthentication(authOptions);

builder.AddAuthorization(policies: new Dictionary<string, string>
{
    { nameof(Policies.ManageResources), Policies.ManageResources },
    { nameof(Policies.ManageRobots), Policies.ManageRobots }
});

var app = builder.Build();

app.UseErrorPages();

app.UseHttpsRedirection();

app.UseAuthentication();

app.MapControllers();

app.UseAuthorization();

app.Run();
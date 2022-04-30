using Masters.AdminAPI.Authorization;
using Masters.Shared.HostingExtensions;
using Masters.Shared.Options;
using Masters.Storage.Contracts;
using Masters.Storage.Stores;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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

var authenticationOptions = new AuthOptions();
builder.Configuration.Bind(nameof(AuthOptions), authenticationOptions);

builder.AddAuthentication(authenticationOptions);

builder.AddAuthorization(policies: new Dictionary<string, string>
{
    {nameof(Policies.ManageResources), Policies.ManageResources},
    {nameof(Policies.ManageRobots), Policies.ManageRobots}
});

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();

app.MapControllers();

app.UseAuthorization();

app.Run();
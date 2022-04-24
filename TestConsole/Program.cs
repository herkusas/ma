// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography.X509Certificates;
using IdentityModel;
using IdentityModel.Client;

var result = RequestTokenAsync();

Console.WriteLine(result);

static Task<string> RequestTokenAsync()
{
    var handler = new HttpClientHandler();
    var cert = FromStore();
    handler.ClientCertificates.Add(cert);

    var client = new HttpClient(handler);

    var response=   client.GetAsync("https://localhost:7092/WeatherForecast").Result;

    return "test";

    // var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
    // if (disco.IsError) throw new Exception(disco.Error);
    //
    // var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
    // {
    //     Address = disco
    //         .TryGetValue(OidcConstants.Discovery.MtlsEndpointAliases)
    //         .TryGetValue(OidcConstants.Discovery.TokenEndpoint)
    //         .ToString(),
    //
    //     ClientId = "mtls",
    //     Scope = "api1"
    // });
    //
    // if (response.IsError) throw new Exception(response.Error);
    // return response;
}

static X509Certificate2 FromStore()
{
    var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
    store.Open(OpenFlags.ReadOnly);
    try
    {
        var results = store.Certificates.Find(X509FindType.FindByThumbprint, 
            "7B7913F271347B5CAC88B3308A3CACD37F771F3A", false);
        return results[0];
    }
    finally
    {
        store.Close();
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ma.Shared.HostingExtensions
{
    public static class AuthorizationExtensions
    {
        public static void AddAuthorization(this WebApplicationBuilder builder, Dictionary<string,string> policies)
        {
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

            
                foreach(var policy in policies)
                {
                    options.AddPolicy(policy.Key,
                        new AuthorizationPolicyBuilder()
                            .RequireClaim("scope",policy.Value)
                            .Build());
                }
            });
        }
    }
}
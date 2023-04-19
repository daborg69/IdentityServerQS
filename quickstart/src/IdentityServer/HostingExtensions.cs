using Duende.IdentityServer;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using quickstart;
using Serilog;

namespace IdentityServer;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // uncomment if you want to add a UI
        builder.Services.AddRazorPages();
        builder.Services.Configure<CookiePolicyOptions>(o =>
                                                            o.Secure = CookieSecurePolicy.Always);

        builder.Services.AddIdentityServer(options =>
               {
                   // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
                   options.EmitStaticAudienceClaim = true;
               })
               .AddInMemoryIdentityResources(Config.IdentityResources)
               .AddInMemoryApiScopes(Config.ApiScopes)
               .AddInMemoryClients(Config.Clients)
               .AddTestUsers(TestUsers.Users);


        // Add Identity Server Cloud hosted demo server as login provider
        builder.Services.AddAuthentication()
               .AddOpenIdConnect("oidc", "Demo IdentityServer", options =>
               {
                   options.SignInScheme  = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                   options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                   options.SaveTokens    = true;
                   options.ClientSecret  = "2CT8Q~1SXccOrGOf2MsDPerkQALjg5ypBL_NDcvB";
                   options.Authority     = "https://demo.duendesoftware.com";
                   options.ClientId      = "interactive.confidential";
                   options.ClientSecret  = "secret";
                   options.ResponseType  = "code";

                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer   = false,
                       ValidateAudience = false,
                       NameClaimType    = "name",
                       RoleClaimType    = "role"
                   };
               });

        // Add Azure AD
        builder.Services.AddAuthentication()
               .AddOpenIdConnect("aad", "Azure Employee SSO", options =>
               {
                   options.SignInScheme          = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                   options.SignOutScheme         = IdentityServerConstants.SignoutScheme;
                   options.Authority             = "https://login.microsoftonline.com/8574f991-f2d3-4413-90fa-1a24cba1e9f6";
                   options.ClientId              = "ae7790a4-7cb0-4bfa-94ae-bf527dbd2aac";
                   options.ResponseType          = OpenIdConnectResponseType.IdToken;
                   options.CallbackPath          = "/signin-aad";
                   options.SignedOutCallbackPath = "/signout-callback-aad";
                   options.RemoteSignOutPath     = "/signout-aad";

                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       NameClaimType = "name",
                       RoleClaimType = "role"
                   };
               });


        return builder.Build();
    }


    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // uncomment if you want to add a UI
        app.UseStaticFiles();
        app.UseRouting();

        app.UseIdentityServer();

        // uncomment if you want to add a UI
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
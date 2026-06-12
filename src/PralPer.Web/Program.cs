using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using MudBlazor.Services;
using PralPer.Infrastructure.Persistence;
using PralPer.Application;
using PralPer.Application.Abstractions;
using PralPer.Domain.Constants;
using PralPer.Infrastructure;
using PralPer.Infrastructure.Identity;
using PralPer.Infrastructure.Seed;
using PralPer.Web.Auth;
using PralPer.Web.Components;
using PralPer.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Blazor (interactive server available; pages opt in per-page where interactivity is needed)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddControllersWithViews();   // MVC + antiforgery filter for AccountController
builder.Services.AddHttpContextAccessor();

// Application + Infrastructure (EF Core, Identity, repositories, UoW, stored-procedure layer)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Shared Data Protection key ring stored in SQL — so the auth/antiforgery cookies issued by one
// server can be read by every other server in a multi-instance (load-balanced) deployment.
builder.Services.AddDataProtection()
    .SetApplicationName("PralPer")
    .PersistKeysToDbContext<AppDbContext>();

// Auth plumbing
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<ICaptchaService, CaptchaService>();
builder.Services.AddScoped<IClaimsTransformation, ActiveRoleClaimsTransformation>();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthPolicies.AdminArea, p => p.RequireRole(RoleNames.Admin))
    .AddPolicy(AuthPolicies.ManagerArea, p => p.RequireRole(RoleNames.Manager))
    .AddPolicy(AuthPolicies.EmployeeArea, p => p.RequireRole(RoleNames.Employee))
    .AddPolicy(AuthPolicies.AdminOrManagerArea, p => p.RequireRole(RoleNames.Admin, RoleNames.Manager));

var app = builder.Build();

// Behind a TLS-terminating load balancer / reverse proxy, trust the forwarded scheme/host so
// HTTPS redirects and the Secure auth cookie work correctly. Configure known proxies in production.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Force HRMS-provisioned accounts through the set-password screen before any app page.
app.Use(async (context, next) =>
{
    var user = context.User;
    if (user?.Identity?.IsAuthenticated == true &&
        user.HasClaim(c => c.Type == AppUserClaimsPrincipalFactory.MustChangePasswordClaim))
    {
        var path = context.Request.Path.Value ?? "/";
        var allowed = path.StartsWith("/verify-otp", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/set-password", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/account", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/_", StringComparison.OrdinalIgnoreCase)   // _blazor / _framework
            || Path.HasExtension(path);                                     // static assets
        if (!allowed)
        {
            // Before OTP -> verify screen; after OTP -> create-password screen.
            var target = user.HasClaim(c => c.Type == AppUserClaimsPrincipalFactory.OtpVerifiedClaim)
                ? "/set-password" : "/verify-otp";
            context.Response.Redirect(target);
            return;
        }
    }
    await next();
});

app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Apply EF migrations + seed reference/identity data at startup
await DbInitializer.InitializeAsync(app.Services);

app.Run();

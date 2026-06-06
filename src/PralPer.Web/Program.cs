using Microsoft.AspNetCore.Authentication;
using MudBlazor.Services;
using PralPer.Application;
using PralPer.Application.Abstractions;
using PralPer.Domain.Constants;
using PralPer.Infrastructure;
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

// Auth plumbing
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IClaimsTransformation, ActiveRoleClaimsTransformation>();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthPolicies.AdminArea, p => p.RequireRole(RoleNames.Admin))
    .AddPolicy(AuthPolicies.ManagerArea, p => p.RequireRole(RoleNames.Manager))
    .AddPolicy(AuthPolicies.EmployeeArea, p => p.RequireRole(RoleNames.Employee));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Apply EF migrations + seed reference/identity data at startup
await DbInitializer.InitializeAsync(app.Services);

app.Run();

using Blazr.App.Bootstrap.Server.Components;
using Blazr.App.Infrastructure;
using Blazr.App.Presentation;
using Blazr.App.UI.Bootstrap;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var services = builder.Services;

// Add in the App services
services.AddAppServerInfrastructureServices();
services.AddAppBootstrapPresentationServices();
services.AddAppBootstrapUIServices();

var app = builder.Build();

// Add the test data to the in-memory test database
app.Services.AddWeatherTestData();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Blazr.App.UI.Bootstrap._Imports).Assembly);

app.Run();

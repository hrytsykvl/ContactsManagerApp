using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using Services;
using RepositoryContracts;
using Repositories;
using Microsoft.AspNetCore.HttpLogging;
using Serilog;
using CRUDExample.Filters.ActionFilters;
using CRUDExample.Filters.ResultFilters;
using CRUDExample;
using CRUDExample.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services,LoggerConfiguration loggerConfiguration) =>
{
    loggerConfiguration
    .ReadFrom.Configuration(context.Configuration) // reading configuration from appsettings.json
    .ReadFrom.Services(services); // reads services and makes them available to serilog
});

builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();


if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseExceptionHandlingMiddleware();
}

app.UseHsts();
app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseHttpLogging();

//app.Logger.LogDebug("debug-message");
//app.Logger.LogInformation("info-message");
//app.Logger.LogWarning("warning-message");
//app.Logger.LogError("error-message");
//app.Logger.LogCritical("critical-message");

if(builder.Environment.IsEnvironment("Test") == false)
{
    Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");
}

app.UseStaticFiles();


app.UseRouting(); // Identify action method based on route
app.UseAuthentication(); // Reading identity cookie
app.UseAuthorization(); // Validates access permissions of the user
app.MapControllers(); // Execute the filter pipeline (action + filters)

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}"); // Admin/Home/Index

    endpoints.MapControllerRoute(name: "default",
        pattern: "{controller}/{action}");
});
app.Run();

public partial class Program { } // make the auto-generated Program class accessible programmatically
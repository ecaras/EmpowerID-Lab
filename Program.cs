using Microsoft.Extensions.Azure;
using Azure.Identity;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Logging.AzureAppServices;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);

//for azure file system logging 
builder.Logging.AddAzureWebAppDiagnostics();
builder.Services.Configure<AzureFileLoggerOptions>(options =>
{
    options.FileName = "log-";
    options.FileSizeLimit = 50 * 1024;
    options.RetainedFileCountLimit = 10;
});


//for blob storage logging
builder.Services.Configure<AzureFileLoggerOptions>(options =>
{
    options.IsEnabled = true;
    options.FileName = "log.txt";
});

//log to App Insights
builder.Logging.AddApplicationInsights(configureTelemetryConfiguration: (config) => 
    config.ConnectionString = builder.Configuration.GetConnectionString("AppInsights"),
    configureApplicationInsightsLoggerOptions: (options) => { }
);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


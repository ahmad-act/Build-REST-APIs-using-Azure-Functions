using BookInformationService.BusinessLayer;
using BookInformationService.DataAccessLayer;
using BookInformationService.DatabaseContext;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using System;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker =>
    {
        // Add additional configuration for the worker if needed
        // worker.UseMiddleware<MyCustomMiddleware>(); // Example middleware
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        var env = context.HostingEnvironment;

        // Add local settings for local development
        if (env.IsDevelopment())
        {
            config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        }
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var configuration = context.Configuration;

        // EF Core for Cosmos DB
        var cosmosEndpoint = configuration.GetValue<string>("COSMOS_DB_ENDPOINT");
        var cosmosKey = configuration.GetValue<string>("COSMOS_DB_KEY");
        var cosmosDatabaseName = configuration.GetValue<string>("COSMOS_DB_DATABASE_NAME");

        services.AddDbContext<AppDbContext>(options =>
            options.UseCosmos(cosmosEndpoint!, cosmosKey!, cosmosDatabaseName!));

        // Models
        services.AddScoped<IBookInformationDL, BookInformationDL>();
        services.AddScoped<IBookInformationBL, BookInformationBL>();

        // AutoMapper
        services.AddAutoMapper(typeof(Program).Assembly);
    })
    .Build();

if (host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment())
{
    // Apply migrations at startup
    using (var serviceScope = host.Services.CreateScope())
    {
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            // Ensure the database is created
            await dbContext.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            // Log the error
            // Log.Error(ex, "An error occurred while applying migrations.");
        }
    }
}

host.Run();

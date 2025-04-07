using CwrStatusChecker.Data;
using CwrStatusChecker.Service;
using CwrStatusChecker.Service.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddScoped<ILdapService, TestLdapService>();
        services.AddScoped<IEmailService, TestEmailService>();
        
        // Configure SQLite database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite("Data Source=CwrStatusChecker.db"));
    });

var host = builder.Build();

// In development mode, run the worker once and exit
if (host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment())
{
    // Initialize the database
    using (var scope = host.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
    }

    // Start the host and wait for it to complete
    await host.StartAsync();
    
    // Give the worker time to complete its task
    await Task.Delay(TimeSpan.FromSeconds(5));
    
    // Stop the host
    await host.StopAsync();
}
else
{
    await host.RunAsync();
}

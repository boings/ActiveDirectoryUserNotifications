using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CwrStatusChecker.Data;
using CwrStatusChecker.Models;
using CwrStatusChecker.Service.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CwrStatusChecker.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string[] _ldapEndpoints;
        private readonly bool _isDevelopment;

        public Worker(
            ILogger<Worker> logger,
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            IHostEnvironment environment)
        {
            _logger = logger;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            _ldapEndpoints = _configuration.GetSection("LdapEndpoints").Get<string[]>() ?? Array.Empty<string>();
            _isDevelopment = environment.IsDevelopment();
            _logger.LogInformation("Worker initialized with {count} LDAP endpoints", _ldapEndpoints.Length);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);
            
            try
            {
                _logger.LogInformation("Starting LDAP user processing at: {time}", DateTimeOffset.Now);
                await ProcessLdapUsersAsync();
                _logger.LogInformation("Completed LDAP user processing at: {time}", DateTimeOffset.Now);
                
                if (!_isDevelopment)
                {
                    // In production, schedule next run for 8 AM
                    var now = DateTime.Now;
                    var nextRun = now.Date.AddDays(1).AddHours(8);
                    var delay = nextRun - now;
                    
                    _logger.LogInformation("Next run scheduled for: {nextRun}", nextRun);
                    await Task.Delay(delay, stoppingToken);
                }
                else
                {
                    _logger.LogInformation("Development mode: Exiting after one iteration");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in worker execution");
                if (!_isDevelopment)
                {
                    // Wait a bit before retrying in production
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private async Task ProcessLdapUsersAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var ldapService = scope.ServiceProvider.GetRequiredService<ILdapService>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            _logger.LogInformation("Starting to process LDAP endpoints");
            
            foreach (var endpoint in _ldapEndpoints)
            {
                try
                {
                    _logger.LogInformation("Processing LDAP endpoint: {endpoint}", endpoint);
                    var ldapUsers = ldapService.GetUsersFromLdap(endpoint);
                    _logger.LogInformation("Retrieved {count} users from LDAP endpoint: {endpoint}", ldapUsers.Count, endpoint);

                    var existingUsers = await context.Users
                        .Where(u => u.LdapEndpoint == endpoint)
                        .ToDictionaryAsync(u => u.Username);
                    _logger.LogInformation("Found {count} existing users in database for endpoint: {endpoint}", existingUsers.Count, endpoint);
                    
                    foreach (var ldapUser in ldapUsers)
                    {
                        if (existingUsers.TryGetValue(ldapUser.Username, out var existingUser))
                        {
                            _logger.LogInformation("Updating existing user: {username}", ldapUser.Username);
                            // Update existing user
                            existingUser.Email = ldapUser.Email;
                            existingUser.ManagerEmail = ldapUser.ManagerEmail;
                            existingUser.LastPasswordChange = ldapUser.LastPasswordChange;
                            existingUser.AccountExpirationDate = ldapUser.AccountExpirationDate;
                            existingUser.IsDisabled = ldapUser.IsDisabled;
                            existingUser.LastChecked = DateTime.UtcNow;
                        }
                        else
                        {
                            _logger.LogInformation("Adding new user: {username}", ldapUser.Username);
                            // Add new user
                            context.Users.Add(ldapUser);
                        }
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation("Successfully saved changes to database for endpoint: {endpoint}", endpoint);
                    
                    await ProcessUserNotificationsAsync(context, emailService);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing LDAP endpoint: {endpoint}", endpoint);
                }
            }
        }

        private async Task ProcessUserNotificationsAsync(ApplicationDbContext context, IEmailService emailService)
        {
            _logger.LogInformation("Starting to process user notifications");
            
            var users = await context.Users
                .Where(u => !u.IsArchived)
                .ToListAsync();

            _logger.LogInformation("Processing notifications for {count} users", users.Count);

            foreach (var user in users)
            {
                try
                {
                    // Check account expiration
                    if (user.AccountExpirationDate.HasValue && user.AccountExpirationDate.Value != DateTime.MaxValue)
                    {
                        var timeUntilExpiration = user.AccountExpirationDate.Value - DateTime.UtcNow;
                        var daysUntilExpiration = timeUntilExpiration.TotalDays;
                        if (daysUntilExpiration <= 30 && daysUntilExpiration > 0)
                        {
                            _logger.LogInformation("Sending expiration notification to user: {username}", user.Username);
                            await emailService.SendExpirationNotificationAsync(user);
                        }
                        else if (daysUntilExpiration <= 0 && !user.IsDisabled)
                        {
                            _logger.LogInformation("Disabling account and sending notification to user: {username}", user.Username);
                            user.IsDisabled = true;
                            await emailService.SendDisableNotificationAsync(user);
                        }
                    }

                    // Check password change requirement
                    if (user.LastPasswordChange.HasValue)
                    {
                        var timeSincePasswordChange = DateTime.UtcNow - user.LastPasswordChange.Value;
                        var daysSincePasswordChange = timeSincePasswordChange.TotalDays;
                        if (daysSincePasswordChange >= 90)
                        {
                            _logger.LogInformation("Sending password change notification to user: {username}", user.Username);
                            await emailService.SendPasswordChangeNotificationAsync(user);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing notifications for user: {username}", user.Username);
                }
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Completed processing user notifications");
        }
    }
}

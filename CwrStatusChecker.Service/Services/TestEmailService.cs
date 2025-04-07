using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CwrStatusChecker.Models;

namespace CwrStatusChecker.Service.Services
{
    public class TestEmailService : IEmailService
    {
        private readonly ILogger<TestEmailService> _logger;

        public TestEmailService(ILogger<TestEmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendExpirationNotificationAsync(User user)
        {
            _logger.LogInformation("TEST EMAIL: Account Expiration Notice for {Username}. Account expires on {ExpirationDate}", 
                user.Username, user.AccountExpirationDate);
            await Task.CompletedTask;
        }

        public async Task SendPasswordChangeNotificationAsync(User user)
        {
            _logger.LogInformation("TEST EMAIL: Password Change Required for {Username}. Last changed on {LastChange}", 
                user.Username, user.LastPasswordChange);
            await Task.CompletedTask;
        }

        public async Task SendDisableNotificationAsync(User user)
        {
            _logger.LogInformation("TEST EMAIL: Account Disabled for {Username}", user.Username);
            await Task.CompletedTask;
        }

        public async Task SendArchiveNotificationAsync(User user)
        {
            _logger.LogInformation("TEST EMAIL: Account Archived for {Username}", user.Username);
            await Task.CompletedTask;
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using CwrStatusChecker.Models;

namespace CwrStatusChecker.Service.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;

        public EmailService(string smtpServer, int smtpPort, string smtpUsername, string smtpPassword, string fromEmail)
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _smtpUsername = smtpUsername;
            _smtpPassword = smtpPassword;
            _fromEmail = fromEmail;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Account Management", _fromEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };
            await SendEmailAsync(message);
        }

        public async Task SendExpirationNotificationAsync(User user)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Account Management", _fromEmail));
            message.To.Add(new MailboxAddress(user.Username, user.Email));
            if (!string.IsNullOrEmpty(user.ManagerEmail))
            {
                message.Cc.Add(new MailboxAddress("Manager", user.ManagerEmail));
            }

            message.Subject = "Account Expiration Notice";
            message.Body = new TextPart("plain")
            {
                Text = $"Dear {user.Username},\n\n" +
                      $"Your account will expire on {user.AccountExpirationDate?.ToShortDateString()}. " +
                      $"Please contact your system administrator to extend your account.\n\n" +
                      $"Best regards,\nAccount Management Team"
            };

            await SendEmailAsync(message);
        }

        public async Task SendPasswordChangeNotificationAsync(User user)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Account Management", _fromEmail));
            message.To.Add(new MailboxAddress(user.Username, user.Email));
            if (!string.IsNullOrEmpty(user.ManagerEmail))
            {
                message.Cc.Add(new MailboxAddress("Manager", user.ManagerEmail));
            }

            message.Subject = "Password Change Required";
            message.Body = new TextPart("plain")
            {
                Text = $"Dear {user.Username},\n\n" +
                      $"Your password has not been changed in over 90 days. " +
                      $"Please change your password immediately to avoid account disablement.\n\n" +
                      $"Best regards,\nAccount Management Team"
            };

            await SendEmailAsync(message);
        }

        public async Task SendDisableNotificationAsync(User user)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Account Management", _fromEmail));
            message.To.Add(new MailboxAddress(user.Username, user.Email));
            if (!string.IsNullOrEmpty(user.ManagerEmail))
            {
                message.Cc.Add(new MailboxAddress("Manager", user.ManagerEmail));
            }

            message.Subject = "Account Disabled";
            message.Body = new TextPart("plain")
            {
                Text = $"Dear {user.Username},\n\n" +
                      $"Your account has been disabled due to not changing your password for over 90 days. " +
                      $"Please contact your system administrator to re-enable your account.\n\n" +
                      $"Best regards,\nAccount Management Team"
            };

            await SendEmailAsync(message);
        }

        public async Task SendArchiveNotificationAsync(User user)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Account Management", _fromEmail));
            message.To.Add(new MailboxAddress(user.Username, user.Email));
            if (!string.IsNullOrEmpty(user.ManagerEmail))
            {
                message.Cc.Add(new MailboxAddress("Manager", user.ManagerEmail));
            }

            message.Subject = "Account Archived";
            message.Body = new TextPart("plain")
            {
                Text = $"Dear {user.Username},\n\n" +
                      $"Your account has been archived due to being disabled for over 90 days. " +
                      $"Please contact your system administrator if you need to restore your account.\n\n" +
                      $"Best regards,\nAccount Management Team"
            };

            await SendEmailAsync(message);
        }

        private async Task SendEmailAsync(MimeMessage message)
        {
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpServer, _smtpPort, false);
                await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
} 
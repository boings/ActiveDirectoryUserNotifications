using System.Threading.Tasks;
using CwrStatusChecker.Models;

namespace CwrStatusChecker.Service.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendExpirationNotificationAsync(User user);
        Task SendPasswordChangeNotificationAsync(User user);
        Task SendDisableNotificationAsync(User user);
        Task SendArchiveNotificationAsync(User user);
    }
} 
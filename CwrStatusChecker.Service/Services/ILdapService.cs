using System.Collections.Generic;
using System.Threading.Tasks;
using CwrStatusChecker.Models;

namespace CwrStatusChecker.Service.Services
{
    public interface ILdapService
    {
        Task<List<User>> GetUsersFromLdap(string ldapEndpoint);
    }
} 
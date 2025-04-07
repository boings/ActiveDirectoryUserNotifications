using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using CwrStatusChecker.Models;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;

namespace CwrStatusChecker.Service.Services
{
    public interface ILdapService
    {
        List<User> GetUsersFromLdap(string ldapEndpoint);
    }

    [SupportedOSPlatform("windows")]
    public class LdapService : ILdapService
    {
        private readonly ILogger<LdapService> _logger;

        public LdapService(ILogger<LdapService> logger)
        {
            _logger = logger;
        }

        public List<User> GetUsersFromLdap(string ldapEndpoint)
        {
            var users = new List<User>();
            
            try
            {
                using var context = new PrincipalContext(ContextType.Domain, ldapEndpoint);
                using var searcher = new PrincipalSearcher(new UserPrincipal(context));
                
                foreach (var result in searcher.FindAll())
                {
                    if (result is not UserPrincipal userPrincipal)
                    {
                        continue;
                    }

                    string? managerEmail = null;
                    try
                    {
                        var directoryEntry = (DirectoryEntry)userPrincipal.GetUnderlyingObject();
                        var manager = directoryEntry.Properties["manager"].Value?.ToString();
                        if (!string.IsNullOrEmpty(manager))
                        {
                            using var managerEntry = new DirectoryEntry($"LDAP://{manager}");
                            managerEmail = managerEntry.Properties["mail"].Value?.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get manager for user {Username}", userPrincipal.SamAccountName);
                    }

                    var user = new User
                    {
                        Username = userPrincipal.SamAccountName ?? string.Empty,
                        Email = userPrincipal.EmailAddress ?? string.Empty,
                        ManagerEmail = managerEmail ?? string.Empty,
                        LastPasswordChange = userPrincipal.LastPasswordSet,
                        AccountExpirationDate = userPrincipal.AccountExpirationDate,
                        IsDisabled = !userPrincipal.Enabled ?? true,
                        IsArchived = false,
                        LdapEndpoint = ldapEndpoint,
                        LastChecked = DateTime.UtcNow
                    };

                    users.Add(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get users from LDAP endpoint {Endpoint}", ldapEndpoint);
                throw;
            }

            return users;
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CwrStatusChecker.Models;

namespace CwrStatusChecker.Service.Services
{
    public class TestLdapService : ILdapService
    {
        public async Task<List<User>> GetUsersFromLdap(string endpoint)
        {
            // Create different test users based on the endpoint
            var users = new List<User>();
            var now = DateTime.UtcNow;

            if (endpoint == "localhost:389")
            {
                users.Add(new User
                {
                    Username = "jdoe",
                    Email = "jdoe@test.com",
                    ManagerEmail = "manager1@test.com",
                    LastPasswordChange = now.AddDays(-85),
                    AccountExpirationDate = now.AddDays(4),
                    LdapEndpoint = endpoint
                });

                users.Add(new User
                {
                    Username = "asmith",
                    Email = "asmith@test.com",
                    ManagerEmail = "manager1@test.com",
                    LastPasswordChange = now.AddDays(-95),
                    AccountExpirationDate = now.AddDays(-1),
                    LdapEndpoint = endpoint
                });

                users.Add(new User
                {
                    Username = "mjohnson",
                    Email = "mjohnson@test.com",
                    ManagerEmail = "manager2@test.com",
                    LastPasswordChange = now.AddDays(-80),
                    AccountExpirationDate = now.AddDays(-2),
                    LdapEndpoint = endpoint
                });

                users.Add(new User
                {
                    Username = "rlee",
                    Email = "rlee@test.com",
                    ManagerEmail = "manager2@test.com",
                    LastPasswordChange = now.AddDays(-75),
                    AccountExpirationDate = now.AddDays(30),
                    LdapEndpoint = endpoint
                });

                users.Add(new User
                {
                    Username = "pwilson",
                    Email = "pwilson@test.com",
                    ManagerEmail = "manager1@test.com",
                    LastPasswordChange = now.AddDays(-89),
                    AccountExpirationDate = now.AddDays(20),
                    LdapEndpoint = endpoint
                });
            }
            else if (endpoint == "test.ldap:389")
            {
                users.Add(new User
                {
                    Username = "jsmith",
                    Email = "jsmith@test2.com",
                    ManagerEmail = "manager3@test2.com",
                    LastPasswordChange = now.AddDays(-88),
                    AccountExpirationDate = now.AddDays(5),
                    LdapEndpoint = endpoint
                });

                users.Add(new User
                {
                    Username = "mwilliams",
                    Email = "mwilliams@test2.com",
                    ManagerEmail = "manager3@test2.com",
                    LastPasswordChange = now.AddDays(-92),
                    AccountExpirationDate = now.AddDays(-3),
                    LdapEndpoint = endpoint
                });

                users.Add(new User
                {
                    Username = "rbrown",
                    Email = "rbrown@test2.com",
                    ManagerEmail = "manager4@test2.com",
                    LastPasswordChange = now.AddDays(-82),
                    AccountExpirationDate = now.AddDays(-1),
                    LdapEndpoint = endpoint
                });

                users.Add(new User
                {
                    Username = "dthomas",
                    Email = "dthomas@test2.com",
                    ManagerEmail = "manager4@test2.com",
                    LastPasswordChange = now.AddDays(-78),
                    AccountExpirationDate = now.AddDays(25),
                    LdapEndpoint = endpoint
                });

                users.Add(new User
                {
                    Username = "kwhite",
                    Email = "kwhite@test2.com",
                    ManagerEmail = "manager3@test2.com",
                    LastPasswordChange = now.AddDays(-91),
                    AccountExpirationDate = now.AddDays(15),
                    LdapEndpoint = endpoint
                });
            }

            return await Task.FromResult(users);
        }
    }
} 
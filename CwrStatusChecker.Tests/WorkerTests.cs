using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using CwrStatusChecker.Data;
using CwrStatusChecker.Models;
using CwrStatusChecker.Service;
using CwrStatusChecker.Service.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace CwrStatusChecker.Tests
{
    public class WorkerTests
    {
        [Fact]
        public async Task ExecuteAsync_DevelopmentMode_ProcessesUsersAndExits()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Worker>>();
            var configurationMock = new Mock<IConfiguration>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var hostEnvironmentMock = new Mock<IHostEnvironment>();
            var ldapServiceMock = new Mock<ILdapService>();
            var emailServiceMock = new Mock<IEmailService>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup configuration
            configurationSectionMock.Setup(x => x.Value)
                .Returns("ldap://test1,ldap://test2");
            configurationMock.Setup(x => x.GetSection("LdapEndpoints"))
                .Returns(configurationSectionMock.Object);

            // Setup environment
            hostEnvironmentMock.Setup(x => x.EnvironmentName)
                .Returns("Development");

            // Setup LDAP service
            var testUsers = new List<User>
            {
                new User
                {
                    Username = "test1",
                    Email = "test1@example.com",
                    ManagerEmail = "manager1@example.com",
                    LdapEndpoint = "ldap://test1",
                    AccountExpirationDate = DateTime.UtcNow.AddDays(5),
                    LastPasswordChange = DateTime.UtcNow.AddDays(-85)
                }
            };
            ldapServiceMock.Setup(x => x.GetUsersFromLdap(It.IsAny<string>()))
                .ReturnsAsync(testUsers);

            // Setup email service
            emailServiceMock.Setup(x => x.SendExpirationNotificationAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            emailServiceMock.Setup(x => x.SendDisableNotificationAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            emailServiceMock.Setup(x => x.SendPasswordChangeNotificationAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Setup service scope
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            var dbContext = new ApplicationDbContext(options);

            serviceProviderMock.Setup(x => x.GetService(typeof(ApplicationDbContext)))
                .Returns(dbContext);
            serviceProviderMock.Setup(x => x.GetService(typeof(ILdapService)))
                .Returns(ldapServiceMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IEmailService)))
                .Returns(emailServiceMock.Object);

            serviceScopeMock.Setup(x => x.ServiceProvider)
                .Returns(serviceProviderMock.Object);
            serviceScopeFactoryMock.Setup(x => x.CreateScope())
                .Returns(serviceScopeMock.Object);

            var worker = new Worker(
                loggerMock.Object,
                configurationMock.Object,
                serviceScopeFactoryMock.Object,
                hostEnvironmentMock.Object);

            // Act
            await worker.StartAsync(CancellationToken.None);
            await Task.Delay(100); // Give it time to process
            await worker.StopAsync(CancellationToken.None);

            // Assert
            ldapServiceMock.Verify(x => x.GetUsersFromLdap(It.IsAny<string>()), Times.Exactly(2));
            emailServiceMock.Verify(x => x.SendExpirationNotificationAsync(It.IsAny<User>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_ProductionMode_RunsContinuously()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<Worker>>();
            var configurationMock = new Mock<IConfiguration>();
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var hostEnvironmentMock = new Mock<IHostEnvironment>();
            var ldapServiceMock = new Mock<ILdapService>();
            var emailServiceMock = new Mock<IEmailService>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup configuration
            configurationSectionMock.Setup(x => x.Value)
                .Returns("ldap://test1,ldap://test2");
            configurationMock.Setup(x => x.GetSection("LdapEndpoints"))
                .Returns(configurationSectionMock.Object);

            // Setup environment
            hostEnvironmentMock.Setup(x => x.EnvironmentName)
                .Returns("Production");

            // Setup LDAP service
            var testUsers = new List<User>
            {
                new User
                {
                    Username = "test1",
                    Email = "test1@example.com",
                    ManagerEmail = "manager1@example.com",
                    LdapEndpoint = "ldap://test1",
                    AccountExpirationDate = DateTime.UtcNow.AddDays(5),
                    LastPasswordChange = DateTime.UtcNow.AddDays(-85)
                }
            };
            ldapServiceMock.Setup(x => x.GetUsersFromLdap(It.IsAny<string>()))
                .ReturnsAsync(testUsers);

            // Setup email service
            emailServiceMock.Setup(x => x.SendExpirationNotificationAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            emailServiceMock.Setup(x => x.SendDisableNotificationAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            emailServiceMock.Setup(x => x.SendPasswordChangeNotificationAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            // Setup service scope
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb2")
                .Options;
            var dbContext = new ApplicationDbContext(options);

            serviceProviderMock.Setup(x => x.GetService(typeof(ApplicationDbContext)))
                .Returns(dbContext);
            serviceProviderMock.Setup(x => x.GetService(typeof(ILdapService)))
                .Returns(ldapServiceMock.Object);
            serviceProviderMock.Setup(x => x.GetService(typeof(IEmailService)))
                .Returns(emailServiceMock.Object);

            serviceScopeMock.Setup(x => x.ServiceProvider)
                .Returns(serviceProviderMock.Object);
            serviceScopeFactoryMock.Setup(x => x.CreateScope())
                .Returns(serviceScopeMock.Object);

            var worker = new Worker(
                loggerMock.Object,
                configurationMock.Object,
                serviceScopeFactoryMock.Object,
                hostEnvironmentMock.Object);

            var cts = new CancellationTokenSource();
            cts.CancelAfter(100); // Cancel after 100ms

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => worker.StartAsync(cts.Token));
            ldapServiceMock.Verify(x => x.GetUsersFromLdap(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
} 
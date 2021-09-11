using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using eShopLabs.BuildingBlocks.Utils.Linq;
using eShopLabs.Services.Identity.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace eShopLabs.Services.Identity.API.Data
{
    public class ApplicationDbContextSeed
    {
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher = new PasswordHasher<ApplicationUser>();
        public async Task SeedAsync(ApplicationDbContext context, IWebHostEnvironment env,
            ILogger<ApplicationDbContextSeed> logger, IOptions<AppSettings> settings, int? retry = 0)
        {
            var retryForAvailability = retry.Value;

            try
            {
                var useCustomizationData = settings.Value.UseCustomizationData;
                var contentRootPath = env.ContentRootPath;

                if (!context.Users.Any())
                {
                    context.Users.AddRange(useCustomizationData ? GetUsersFromFile(contentRootPath, logger) : GetDefaultUser());

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                if (retryForAvailability < 10)
                {
                    retryForAvailability++;

                    logger.LogError(ex, "Exception error while migrating {DbContextname}", nameof(ApplicationDbContext));

                    await SeedAsync(context, env, logger, settings, retryForAvailability);
                }
            }
        }

        private IEnumerable<ApplicationUser> GetDefaultUser()
        {
            var user = new ApplicationUser()
            {
                CardHolderName = "DemoUser",
                CardNumber = "4012888888881881",
                CardType = 1,
                City = "Redmond",
                Country = "U.S.",
                Email = "demouser@microsoft.com",
                Expiration = "12/21",
                Id = Guid.NewGuid().ToString(),
                LastName = "DemoLastName",
                Name = "DemoUser",
                PhoneNumber = "1234567890",
                UserName = "demouser@microsoft.com",
                ZipCode = "98052",
                State = "WA",
                Street = "15703 NE 61st Ct",
                SecurityNumber = "535",
                NormalizedEmail = "DEMOUSER@MICROSOFT.COM",
                NormalizedUserName = "DEMOUSER@MICROSOFT.COM",
                SecurityStamp = Guid.NewGuid().ToString("D"),
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, "Pass@word1");

            return new List<ApplicationUser> { user };
        }

        private IEnumerable<ApplicationUser> GetUsersFromFile(string contentRootPath, ILogger<ApplicationDbContextSeed> logger)
        {
            var csvFileUsers = Path.Combine(contentRootPath, "Setup", "Users.csv");

            if (!File.Exists(csvFileUsers))
            {
                return GetDefaultUser();
            }

            string[] csvHeaders;

            try
            {
                var requiredHeaders = new string[]{ "cardholdername", "cardnumber", "cardtype", "city", "country",
                    "email", "expiration", "lastname", "name", "phonenumber",
                    "username", "zipcode", "state", "street", "securitynumber",
                    "normalizedemail", "normalizedusername", "password" };

                csvHeaders = GetHeaders(requiredHeaders, csvFileUsers);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception error: {Message}", ex.Message);

                return GetDefaultUser();
            }

            var users = File.ReadAllLines(csvFileUsers)
                .Skip(1)
                .Select(r => Regex.Split(r, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"))
                .SelectTry(c => CreateAppliationUser(c, csvHeaders))
                .OnCaughtException(ex =>
                {
                    logger.LogError(ex, "Exception error: {Message}", ex.Message);

                    return null;
                })
                .Where(x => x != null)
                .ToList();

            return users;
        }

        private ApplicationUser CreateAppliationUser(string[] column, string[] headers)
        {

            if (column.Count() != headers.Count())
            {
                throw new Exception($"column count '{column.Count()}' not the same as headers count'{headers.Count()}'");
            }

            var cardTypeString = column[Array.IndexOf(headers, "cardtype")].Trim('"').Trim();

            if (!int.TryParse(cardTypeString, out int cardtype))
            {
                throw new Exception($"cardtype='{cardTypeString}' is not a number");
            }

            var user = new ApplicationUser
            {
                CardHolderName = column[Array.IndexOf(headers, "cardholdername")].Trim('"').Trim(),
                CardNumber = column[Array.IndexOf(headers, "cardnumber")].Trim('"').Trim(),
                CardType = cardtype,
                City = column[Array.IndexOf(headers, "city")].Trim('"').Trim(),
                Country = column[Array.IndexOf(headers, "country")].Trim('"').Trim(),
                Email = column[Array.IndexOf(headers, "email")].Trim('"').Trim(),
                Expiration = column[Array.IndexOf(headers, "expiration")].Trim('"').Trim(),
                Id = Guid.NewGuid().ToString(),
                LastName = column[Array.IndexOf(headers, "lastname")].Trim('"').Trim(),
                Name = column[Array.IndexOf(headers, "name")].Trim('"').Trim(),
                PhoneNumber = column[Array.IndexOf(headers, "phonenumber")].Trim('"').Trim(),
                UserName = column[Array.IndexOf(headers, "username")].Trim('"').Trim(),
                ZipCode = column[Array.IndexOf(headers, "zipcode")].Trim('"').Trim(),
                State = column[Array.IndexOf(headers, "state")].Trim('"').Trim(),
                Street = column[Array.IndexOf(headers, "street")].Trim('"').Trim(),
                SecurityNumber = column[Array.IndexOf(headers, "securitynumber")].Trim('"').Trim(),
                NormalizedEmail = column[Array.IndexOf(headers, "normalizedemail")].Trim('"').Trim(),
                NormalizedUserName = column[Array.IndexOf(headers, "normalizedusername")].Trim('"').Trim(),
                SecurityStamp = Guid.NewGuid().ToString("D"),
                PasswordHash = column[Array.IndexOf(headers, "password")].Trim('"').Trim(), // Note: This is the password
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, user.PasswordHash);

            return user;
        }

        private string[] GetHeaders(string[] requiredHeaders, string csvFileUsers)
        {
            var csvHeaders = File.ReadLines(csvFileUsers).First().ToLowerInvariant().Split(',');

            if (csvHeaders.Count() != requiredHeaders.Count())
            {
                throw new Exception($"requiredHeader count '{requiredHeaders.Count()}' is different than readHeader '{csvHeaders.Count()}'");
            }

            foreach (var header in requiredHeaders)
            {
                if (!csvHeaders.Contains(header))
                {
                    throw new Exception($"doesn't contain required header '{header}'");
                }
            }

            return csvHeaders;
        }
    }
}
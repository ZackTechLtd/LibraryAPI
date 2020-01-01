

namespace LibraryAPIApp.Data
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Security.Claims;
    using Microsoft.Extensions.Options;
    using DataAccess.IdentityModels;

    public class DbInitializer
    {
        public static async Task Initialize(IdentityDb context, string[] defaultAdmins, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, ILogger<DbInitializer> logger, IOptions<IdentityOptions> optionsAccessor, string environmentName)
        {
            context.Database.EnsureCreated();

            
                //Look for any users.
             if (context.Users.Any())
             {
                return; // DB has been seeded
             }

             await CreateDefaultUserAndRoleForApplication(defaultAdmins, userManager, roleManager, logger, optionsAccessor);
           
        }

        private static async Task CreateDefaultUserAndRoleForApplication(string[] defaultAdmins, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm, ILogger<DbInitializer> logger, IOptions<IdentityOptions> optionsAccessor)
        {
            string[] appRoles = { "Administrator", "Librarian", "User" };
            await CreateDefaultRoles(rm, logger, appRoles);
            foreach(string email in defaultAdmins)
            {
                var user = await CreateDefaultUser(um, logger, email);
                await SetPasswordForDefaultUser(um, logger, email, user);
                await AddDefaultRoleToDefaultUser(um, logger, email, appRoles[0], user);
            }
            
            var adminrole = await rm.FindByNameAsync(appRoles[0]);

            //string[] claims = { "usermanagerview", "usermanageredit", "companyview", "companyedit", "branchview", "branchedit" };
            //foreach(string claim in claims)
            //{
            //    await rm.AddClaimAsync(adminrole, new System.Security.Claims.Claim(ClaimTypes.Role, claim));
            //}
            
            
        }

        private static async Task CreateDefaultRoles(RoleManager<IdentityRole> rm, ILogger<DbInitializer> logger, string[] roles)
        {
            foreach(string role in roles)
            {
                logger.LogInformation($"Create the role `{role}` for application");
                var ir = await rm.CreateAsync(new IdentityRole(role));
                if (ir.Succeeded)
                {
                    logger.LogDebug($"Created the role `{role}` successfully");
                }
                else
                {
                    var exception = new ApplicationException($"Default role `{role}` cannot be created");
                    logger.LogError(exception, GetIdentiryErrorsInCommaSeperatedList(ir));
                    throw exception;
                }

            }
            
        }

        private static async Task<ApplicationUser> CreateDefaultUser(UserManager<ApplicationUser> um, ILogger<DbInitializer> logger, string email)
        {
            logger.LogInformation($"Create default user with email `{email}` for application");
            var user = new ApplicationUser { UserName = email, Email = email, PhoneNumber = "07554459413", LockoutEnabled = false };
            //var user = new ApplicationUser(email, "First", "Last", new DateTime(1970, 1, 1));

            var ir = await um.CreateAsync(user);
            if (ir.Succeeded)
            {
                logger.LogDebug($"Created default user `{email}` successfully");
            }
            else
            {
                var exception = new ApplicationException($"Default user `{email}` cannot be created");
                logger.LogError(exception, GetIdentiryErrorsInCommaSeperatedList(ir));
                throw exception;
            }

            var createdUser = await um.FindByEmailAsync(email);
            return createdUser;
        }

        private static async Task SetPasswordForDefaultUser(UserManager<ApplicationUser> um, ILogger<DbInitializer> logger, string email, ApplicationUser user)
        {
            logger.LogInformation($"Set password for default user `{email}`");
            const string password = "Password123@";
            var ir = await um.AddPasswordAsync(user, password);
            if (ir.Succeeded)
            {
                logger.LogTrace($"Set password `{password}` for default user `{email}` successfully");
            }
            else
            {
                var exception = new ApplicationException($"Password for the user `{email}` cannot be set");
                logger.LogError(exception, GetIdentiryErrorsInCommaSeperatedList(ir));
                throw exception;
            }
        }

        private static async Task AddDefaultRoleToDefaultUser(UserManager<ApplicationUser> um, ILogger<DbInitializer> logger, string email, string administratorRole, ApplicationUser user)
        {
            logger.LogInformation($"Add default user `{email}` to role '{administratorRole}'");
            var ir = await um.AddToRoleAsync(user, administratorRole);
            if (ir.Succeeded)
            {
                logger.LogDebug($"Added the role '{administratorRole}' to default user `{email}` successfully");
            }
            else
            {
                var exception = new ApplicationException($"The role `{administratorRole}` cannot be set for the user `{email}`");
                logger.LogError(exception, GetIdentiryErrorsInCommaSeperatedList(ir));
                throw exception;
            }
        }

        private static string GetIdentiryErrorsInCommaSeperatedList(IdentityResult ir)
        {
            string errors = null;
            foreach (var identityError in ir.Errors)
            {
                errors += identityError.Description;
                errors += ", ";
            }
            return errors;
        }
    }
}

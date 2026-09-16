

namespace LibraryAPIApp.Data
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Options;
    using DataAccess.IdentityModels;

    public class DbInitializer
    {
        public static async Task Initialize(IdentityDb context, string[] defaultAdmins, string defaultAdminPassword,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, ILogger<DbInitializer> logger, IOptions<IdentityOptions> optionsAccessor,
            string environmentName,
            string companySeedCode, string companySeedName, string branchSeedCode, string branchSeedName)
        {
            context.Database.EnsureCreated();

            // Ensure the legacy Company/Branch schema is populated (idempotent).
            Company company = EnsureCompany(context, companySeedCode, companySeedName);
            Branch branch = EnsureBranch(context, company, branchSeedCode, branchSeedName);

            // Look for any users.
             if (context.Users.Any())
             {
                // Assign the config-listed admins to the seeded company/branch if they have none.
                await AssignCompanyBranchToAdmins(context, defaultAdmins, company.CompanyId, branch.BranchId);
                return; // DB has been seeded
             }

             await CreateDefaultUserAndRoleForApplication(defaultAdmins, defaultAdminPassword, userManager, roleManager, logger, optionsAccessor, company.CompanyId, branch.BranchId);
        }

        private static Company EnsureCompany(IdentityDb context, string companyCode, string companyName)
        {
            var company = context.Companies.FirstOrDefault(x => x.CompanyCode == companyCode);
            if (company == null)
            {
                company = new Company { CompanyCode = companyCode, CompanyName = companyName };
                context.Companies.Add(company);
                context.SaveChanges();
            }

            return company;
        }

        private static Branch EnsureBranch(IdentityDb context, Company company, string branchCode, string branchName)
        {
            var branch = context.Branches.FirstOrDefault(x => x.BranchCode == branchCode);
            if (branch == null)
            {
                branch = new Branch { CompanyId = company.CompanyId, BranchCode = branchCode, BranchName = branchName };
                context.Branches.Add(branch);
                context.SaveChanges();
            }

            return branch;
        }

        private static async Task AssignCompanyBranchToAdmins(IdentityDb context, string[] defaultAdmins, int companyId, int branchId)
        {
            var changed = false;

            foreach (string email in defaultAdmins)
            {
                var user = context.Users.FirstOrDefault(x => x.NormalizedEmail == email.ToUpperInvariant() || x.NormalizedUserName == email.ToUpperInvariant());
                if (user == null)
                {
                    continue;
                }

                if (user.CompanyId == null || user.CompanyId < 1)
                {
                    user.CompanyId = companyId;
                    changed = true;
                }

                if (user.BranchId == null || user.BranchId < 1)
                {
                    user.BranchId = branchId;
                    changed = true;
                }
            }

            if (changed)
            {
                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateDefaultUserAndRoleForApplication(string[] defaultAdmins, string defaultAdminPassword,
            UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm, ILogger<DbInitializer> logger, IOptions<IdentityOptions> optionsAccessor, int companyId, int branchId)
        {
            string[] appRoles = { "Administrator", "Librarian", "User" };
            await CreateDefaultRoles(rm, logger, appRoles);
            foreach(string email in defaultAdmins)
            {
                var user = await CreateDefaultUser(um, logger, email, companyId, branchId);
                await SetPasswordForDefaultUser(um, logger, email, defaultAdminPassword, user);
                await AddDefaultRoleToDefaultUser(um, logger, email, appRoles[0], user);
            }
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

        private static async Task<ApplicationUser> CreateDefaultUser(UserManager<ApplicationUser> um, ILogger<DbInitializer> logger, string email, int companyId, int branchId)
        {
            logger.LogInformation($"Create default user with email `{email}` for application");
            var user = new ApplicationUser { UserName = email, Email = email, PhoneNumber = "07554459413", LockoutEnabled = false, CompanyId = companyId, BranchId = branchId };

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

        private static async Task SetPasswordForDefaultUser(UserManager<ApplicationUser> um, ILogger<DbInitializer> logger, string email, string password, ApplicationUser user)
        {
            logger.LogInformation($"Set password for default user `{email}`");
            var ir = await um.AddPasswordAsync(user, password);
            if (ir.Succeeded)
            {
                logger.LogTrace($"Set password for default user `{email}` successfully");
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
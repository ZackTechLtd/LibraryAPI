using System;
using LibraryApp.Infrastructure.Identity;
using LibraryApp.Infrastructure.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LibraryApp.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {

        //services.AddTransient<IDomainEventDispatcher, DomainEventDispatcher>();
        //services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        var useDbType = configuration.GetValue<string>("UseDbType");
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? String.Empty;

        switch (useDbType)
        {
            case "UseInMemoryDatabase":
                services.AddDbContext<IdentityDb>(options =>
                options.UseInMemoryDatabase("LibraryDb"));
                break;

            
            case "UseSqlLite":
                services.AddDbContext<IdentityDb>(options =>
                options.UseSqlite(connectionString));
                break;

            case "UseMySql":
                services.AddDbContext<IdentityDb>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
                break;

            default:
                services.AddDbContext<IdentityDb>(options =>
                    options.UseInMemoryDatabase("LibraryDb"));
                break;
        }

        //services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        //services.AddTransient<IDateTime, DateTimeService>();

        return services;
    }

    public static void EnsureDatabase(IApplicationBuilder app, IConfiguration configuration)
    {
        try
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<IdentityDb>();
                if (db != null && db.Database != null)
                {
                    //false if already created
                    //bool dbCreated = db.Database.EnsureCreated();
                }
                //bool dbCreated = serviceScope.ServiceProvider.GetService<IdentityDb>().Database.EnsureCreated();
                //serviceScope.ServiceProvider.GetService<IdentityDb>().Database.Migrate();
                //serviceScope.ServiceProvider.GetService<IdentityDb>().EnsureSeedData();

                //https://stackoverflow.com/questions/30453567/how-to-register-custom-userstore-usermanager-in-di
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var optionsAccessor = serviceScope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>();
                var dbInitializerLogger = serviceScope.ServiceProvider.GetRequiredService<ILogger<DbInitializer>>();
                IWebHostEnvironment env = serviceScope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                var adminSection = configuration.GetSection("DefaultAdmins");
                var defaultAdmins = adminSection
                    .GetSection("DbAdmins")
                    .GetChildren()
                    .Select(x => x.Value)
                    .ToArray();

                string[] admins = Array.ConvertAll(defaultAdmins, s => s?.ToString() ?? "");

                DbInitializer.Initialize(db ?? default!, admins, userManager, roleManager, dbInitializerLogger, optionsAccessor, env.EnvironmentName).Wait();


            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }

    }
}


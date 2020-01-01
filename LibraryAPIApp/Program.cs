using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace LibraryAPIApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
            /*
            var config = new ConfigurationBuilder().AddCommandLine(args).Build();

            IHostBuilder host = CreateHostBuilder(args, config);

            
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<IdentityDb>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var optionsAccessor = services.GetRequiredService<IOptions<IdentityOptions>>();

                    var dbInitializerLogger = services.GetRequiredService<ILogger<DbInitializer>>();
                    IHostingEnvironment env = services.GetRequiredService<IHostingEnvironment>();

                    DbInitializer.Initialize(context, userManager, roleManager, dbInitializerLogger, optionsAccessor, env.EnvironmentName).Wait();

                }
                catch (Exception ex)
                {

                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
           
            host.Build().Run();
            */

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

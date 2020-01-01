using System;
using System.Linq;
using Common.Configuration;
using DataAccess.IdentityModels;
using LibraryAPIApp.Data;
using LibraryAPIApp.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LibraryAPIApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //https://codingblast.com/asp-net-core-configuration-reloading-binding-injecting/
            // Add our Config object so it can be injected
            //services.Configure<ApiConfiguration>(Configuration.GetSection("ApiConfiguration"));

            //services.Configure<ApiConfiguration>(Configuration);
            IConfiguration appconfig = Configuration.GetSection("ApiConfiguration");
            services.Configure<ApiConfiguration>(appconfig);

            services.AddDbContext<IdentityDb>();
            services.AddCustomServices();

            //services.AddDbContext<IdentityDb>(options =>
            //{
            //    options.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
            //});

            services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<IdentityDb>().AddUserManager<UserManager<ApplicationUser>>();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;

                // User settings
                options.User.RequireUniqueEmail = true;
            });



            /*
            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), builder.Services);
            builder.AddEntityFrameworkStores<IdentityDb>();
            builder.AddRoleValidator<RoleValidator<IdentityRole>>();
            builder.AddSignInManager<SignInManager<ApplicationUser>>();
            */
            

#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
            ServiceLocator.SetLocatorProvider(services.BuildServiceProvider());
#pragma warning restore ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'

            services.AddJwtBearerServices();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Member", policy => policy.RequireClaim("MembershipId"));
                options.AddPolicy(Policies.Admin, Policies.AdminPolicy());
                options.AddPolicy(Policies.Librarian, Policies.LibrarianPolicy());
                options.AddPolicy(Policies.User, Policies.UserPolicy());

                //options.AddPolicy("Administrator", policy => policy.RequireClaim("AdministratorId"));
                //options.AddPolicy("Senior Librarian", policy => policy.RequireClaim("SeniorLibrarianId"));
                //foreach (string claim in claims)
                //{
                //    options.AddPolicy(claim, policy => policy.RequireClaim(claim));
                //}
 

                //options.AddPolicy("AdminOrSeniorLibrarian", policy => policy.RequireAssertion(context =>
                //            context.User.HasClaim(c =>
                //                (c.Type == "AdministratorId" ||
                //                 c.Type == "SeniorLibrarianId"))));


            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            EnsureDatabase(app);
        }

        private void EnsureDatabase(IApplicationBuilder app)
        {
            try
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var db = serviceScope.ServiceProvider.GetService<IdentityDb>();
                    if (db != null && db.Database != null)
                    {
                        //false if already created
                        bool dbCreated = db.Database.EnsureCreated();
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

                    var adminSection = Configuration.GetSection("DefaultAdmins");
                    var defaultAdmins = adminSection
                        .GetSection("DbAdmins")
                        .GetChildren()
                        .Select(x => x.Value)
                        .ToArray();
                    DbInitializer.Initialize(db, defaultAdmins, userManager, roleManager, dbInitializerLogger, optionsAccessor, env.EnvironmentName).Wait();
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            
        }
    }
}

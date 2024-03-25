using System.Reflection;
using Common.Configuration;
using Common.Util;
using LibraryApp.Core.Util;
using LibraryApp.Infrastructure;
using LibraryApp.Infrastructure.Identity;
using LibraryApp.Infrastructure.Repository;
using LibraryApp.Infrastructure.WebApiManager.Interfaces;
using LibraryApp.Infrastructure.WebApiManager.Manager;
using LibraryApp.WebApiManager.Infrastructure.Interfaces;
using MediatR;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

namespace LibraryApp.Api;

public static class StartupExtensions
{
    public static void ConfigureHost(this WebApplicationBuilder builder)
    {
        // ApplicationInsights
        builder.Host.UseSerilog((_, services, loggerConfiguration) =>
        {
            var logLevelString = builder.Configuration["LogLevel"];

            var parsed = Enum.TryParse<LogEventLevel>(logLevelString, out var logLevel);

            loggerConfiguration.WriteTo.ApplicationInsights(
                services.GetRequiredService<TelemetryConfiguration>(),
                TelemetryConverter.Traces,
                parsed ? logLevel : LogEventLevel.Warning);

            loggerConfiguration.WriteTo.Console(
                parsed ? logLevel : LogEventLevel.Warning);
        });
    }

    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration, bool isProduction)
    {
        services.AddApplicationInsightsTelemetry();
        IConfiguration appconfig = configuration.GetSection("ApiConfiguration");
        services.Configure<ApiConfiguration>(appconfig);

        services.AddDbContext<IdentityDb>();

        services.AddApplicationServices();

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
        //ServiceLocator.SetLocatorProvider(services.BuildServiceProvider());
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

        // Add services to the container.
        services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer()
            .AddInfrastructureServices(configuration)
            .AddApplicationServices();


        services.AddSwaggerGen();
    }

    

    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(Assembly.GetExecutingAssembly());

        //services.AddTransient<IJwtTokenBuilder, JwtTokenBuilder>();
        //services.AddTransient<IJwtToken, JwtToken>();
        //services.AddTransient<IAppConfig, AppConfig>();

        //services.AddTransient<UserManager<ApplicationUser>>();


        services.AddTransient<IdentityDb>();
        services.AddTransient<IRijndaelCrypt, RijndaelCrypt>();
        services.AddTransient<IRandomKeyGenerator, RandomKeyGenerator>();


        services.AddTransient<IUserWebApiManager, UserWebApiManager>();
        services.AddTransient<IUserRepository, UserRepository>();

        services.AddTransient<ILibraryUserWebApiManager, LibraryUserWebApiManager>();
        services.AddTransient<ILibraryUserRepository, LibraryUserRepository>();

        services.AddTransient<ILibraryBookWebApiManager, LibraryBookWebApiManager>();
        services.AddTransient<ILibraryBookRepository, LibraryBookRepository>();

        services.AddTransient<ILibraryBookStatusWebApiManager, LibraryBookStatusWebApiManager>();
        services.AddTransient<ILibraryBookStatusRepository, LibraryBookStatusRepository>();


    }

    public static void AddJwtBearerServices(this IServiceCollection services)
    {
        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,

                        ValidIssuer = "ZackTechSecurityBearer",
                        ValidAudience = "ZackTechSecurityBearer",
                        IssuerSigningKey = JwtSecurityKey.Create("ZackTechSecretKey")
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} OnAuthenticationFailed: " + context.Exception.Message);
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} OnTokenValidated: {context.SecurityToken}");
                            return Task.CompletedTask;
                        }
                    };
                });

        services.AddTransient<IJwtTokenBuilder, JwtTokenBuilder>();
        //services.AddTransient<IJwtToken, JwtToken>();

    }

    public static IServiceProvider ConfigureWebApplication(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        //await app.RegisterEndPoints();

        return app.Services;
    }
}


using System;
using System.Reflection;
using Common.Util;
using DataAccess.IdentityModels;
using DataAccess.WebApiManager.Interfaces;
using DataAccess.WebApiManager.Manager;
using DataAccess.WebApiRepository.Interfaces;
using DataAccess.WebApiRepository.Repository;
using LibraryAPIApp.Data;
using LibraryAPIApp.Util;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace LibraryAPIApp
{
    public static class ServicesConfiguration
    {
        public static void AddCustomServices(this IServiceCollection services)
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
    }
}

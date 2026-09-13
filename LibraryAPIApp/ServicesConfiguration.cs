using System.Reflection;
using Common.Util;
using DataAccess.WebApiManager.Interfaces;
using DataAccess.WebApiManager.Manager;
using DataAccess.WebApiRepository.Interfaces;
using DataAccess.WebApiRepository.Repository;
using LibraryAPIApp.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MediatR;

namespace LibraryAPIApp
{
    public static class ServicesConfiguration
    {
        public static void AddCustomServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

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

        public static void AddJwtBearerServices(this IServiceCollection services, IConfiguration configuration)
        {
            var jwt = configuration.GetSection("Jwt");
            var issuer = jwt["Issuer"] ?? "ZackTechSecurityBearer";
            var audience = jwt["Audience"] ?? "ZackTechSecurityBearer";
            var secret = jwt["SecretKey"] ?? "ZackTechSecretKey";

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

                            ValidIssuer = issuer,
                            ValidAudience = audience,
                            IssuerSigningKey = JwtSecurityKey.Create(secret)
                        };
                    });

            services.AddTransient<IJwtTokenBuilder, JwtTokenBuilder>(sp =>
                new JwtTokenBuilder()
                    .AddIssuer(issuer)
                    .AddAudience(audience)
                    .AddSecurityKey(JwtSecurityKey.Create(secret)));
        }
    }
}
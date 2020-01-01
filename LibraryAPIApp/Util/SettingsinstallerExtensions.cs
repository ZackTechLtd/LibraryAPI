using System;
using LibraryAPIApp.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryAPIApp.Util
{
    public static class SettingsinstallerExtensions
    {
        public static IWebHostBuilder ConfigureSettings(this IWebHostBuilder builder)
        {
            return builder.ConfigureServices((context, services) =>
            {
                var config = context.Configuration;

                //var ctx = services..GetRequiredService<IdentityDb>();

                //services.Configure<ConnectionStrings>(config.GetSection("ConnectionStrings"));
                //services.AddSingleton<ConnectionStrings>(
                //    ctx => ctx.GetService<IOptions<ConnectionStrings>>().Value)
            });
        }
    }
}

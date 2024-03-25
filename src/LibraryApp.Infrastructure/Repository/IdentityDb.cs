using System;
using System.Configuration;
using LibraryApp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace LibraryApp.Infrastructure.Repository;

public class IdentityDb : IdentityDbContext<ApplicationUser>
{
    private readonly IConfiguration _configuration;
    
    //public IdentityDb() { }

    public IdentityDb(DbContextOptions<IdentityDb> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    //public IdentityDb(DbContextOptions<IdentityDb> options, IOptions<ApiConfiguration> apiConfiguration)
    //    : base(options)
    //{
    //    _apiConfiguration = apiConfiguration;
    //}

    public IdentityDb Create()
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDb>();
        var connectionString = GetAppConnectionString();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new IdentityDb(optionsBuilder.Options, _configuration);
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = GetAppConnectionString();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }

    private string GetAppConnectionString()
    {
        IConfiguration config = _configuration; // ServiceLocator.Current.GetInstance<IConfiguration>();
        if (config == null)
        {
            return string.Empty;
        }
        else
        {
            return config.GetConnectionString("DefaultConnection") ?? string.Empty;
        }
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
        //if (string.Equals(_apiConfiguration.Value.RDBMS, "MySQL", StringComparison.OrdinalIgnoreCase) || string.Equals(_apiConfiguration.Value.RDBMS, "Postgres", StringComparison.OrdinalIgnoreCase))
        //{
        //Do Nothing
        //}
        //else
        //{
        //    builder.HasDefaultSchema("Identity");
        //}

        builder.Entity<PreviousPassword>()
        .HasKey(c => new { c.PasswordHash, c.UserId });
    }



    /// <summary>
    /// Update Model Item and save changes
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool UpdateModelItem(object item)
    {
        if (item == null)
            return false;
        this.Entry(item).State = EntityState.Modified;
        this.SaveChanges();
        return true;
    }

    /// <summary>
    /// Delete Model Item and save changes
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool DeleteModelItem(object item)
    {
        if (item == null)
            return false;
        this.Entry(item).State = EntityState.Deleted;
        this.SaveChanges();
        return true;
    }
}


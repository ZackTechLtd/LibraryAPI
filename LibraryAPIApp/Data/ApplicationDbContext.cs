using DataAccess.IdentityModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPIApp.Data
{
    public class IdentityDb : IdentityDbContext<ApplicationUser>
    {
        public IdentityDb(DbContextOptions<IdentityDb> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.Entity<PreviousPassword>()
            .HasKey(c => new { c.PasswordHash, c.UserId });

            // Legacy schema (restored so legacy UserRepository queries resolve)
            builder.Entity<Company>()
                .ToTable("Company")
                .HasIndex(c => c.CompanyCode)
                .IsUnique();

            builder.Entity<Branch>()
                .ToTable("Branch")
                .HasIndex(b => b.BranchCode)
                .IsUnique();

            builder.Entity<Branch>()
                .HasOne(b => b.Company)
                .WithMany(c => c.Branches)
                .HasForeignKey(b => b.CompanyId);

            builder.Entity<LastUserCompanyAndBranch>()
                .ToTable("LastUserCompanyAndBranch")
                .HasKey(x => x.LastUserCompanyAndBranchId);
        }

        /// <summary>
        /// Companies (legacy schema restored)
        /// </summary>
        public DbSet<Company> Companies { get; set; }

        /// <summary>
        /// Branches (legacy schema restored)
        /// </summary>
        public DbSet<Branch> Branches { get; set; }

        /// <summary>
        /// Last Company and Branch per user (legacy schema restored)
        /// </summary>
        public DbSet<LastUserCompanyAndBranch> LastUserCompanyAndBranches { get; set; }

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
}
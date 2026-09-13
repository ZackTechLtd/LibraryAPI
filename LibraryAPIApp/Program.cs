using Common.Configuration;
using DataAccess.IdentityModels;
using LibraryAPIApp;
using LibraryAPIApp.Data;
using LibraryAPIApp.Util;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiConfiguration>(builder.Configuration.GetSection("ApiConfiguration"));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured. Set it in user secrets or environment variables.");
}

// EF Core is used for ASP.NET Core Identity only. Business data uses Dapper via DataAccess.
builder.Services.AddDbContext<IdentityDb>(options => options.UseMySQL(connectionString));
builder.Services.AddCustomServices();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityDb>()
    .AddUserManager<UserManager<ApplicationUser>>();

builder.Services.Configure<IdentityOptions>(options =>
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

builder.Services.AddJwtBearerServices(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Member", policy => policy.RequireClaim("MembershipId"));
    options.AddPolicy(Policies.Admin, Policies.AdminPolicy());
    options.AddPolicy(Policies.Librarian, Policies.LibrarianPolicy());
    options.AddPolicy(Policies.User, Policies.UserPolicy());
});

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await EnsureDatabase(app);

app.Run();

static async Task EnsureDatabase(WebApplication app)
{
    using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

    var db = serviceScope.ServiceProvider.GetService<IdentityDb>();
    if (db == null || db.Database == null)
    {
        return;
    }

    db.Database.EnsureCreated();

    // EnsureCreated never mutates an existing database; apply the idempotent legacy-schema
    // upgrade (Company/Branch/LastUserCompanyAndBranch tables + AspNetUsers.CompanyId/BranchId).
    UpgradeDatabaseSchema(db);

    var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var optionsAccessor = serviceScope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>();
    var dbInitializerLogger = serviceScope.ServiceProvider.GetRequiredService<ILogger<DbInitializer>>();

    var adminSection = app.Configuration.GetSection("DefaultAdmins");
    var defaultAdmins = adminSection
        .GetSection("DbAdmins")
        .GetChildren()
        .Select(x => x.Value)
        .ToArray();

    var defaultAdminPassword = app.Configuration["DefaultAdmins:Password"] ?? "Password123@";

    var companySeed = app.Configuration.GetSection("CompanySeed");
    var companySeedCode = companySeed["CompanyCode"] ?? "TEST";
    var companySeedName = companySeed["CompanyName"] ?? "Test Company";

    var branchSeed = app.Configuration.GetSection("BranchSeed");
    var branchSeedCode = branchSeed["BranchCode"] ?? "MAIN";
    var branchSeedName = branchSeed["BranchName"] ?? "Main Branch";

    await DbInitializer.Initialize(db, defaultAdmins, defaultAdminPassword, userManager, roleManager, dbInitializerLogger, optionsAccessor, app.Environment.EnvironmentName,
        companySeedCode, companySeedName, branchSeedCode, branchSeedName);
}

static void UpgradeDatabaseSchema(IdentityDb db)
{
    // Idempotent DDL for the legacy schema restored so the legacy UserRepository queries resolve.
    db.Database.ExecuteSqlRaw(
        "CREATE TABLE IF NOT EXISTS Company " +
        "(CompanyId INT AUTO_INCREMENT PRIMARY KEY, CompanyCode VARCHAR(40) NOT NULL, CompanyName VARCHAR(255) NOT NULL, " +
        "UNIQUE KEY uq_Company_CompanyCode (CompanyCode));");

    db.Database.ExecuteSqlRaw(
        "CREATE TABLE IF NOT EXISTS Branch " +
        "(BranchId INT AUTO_INCREMENT PRIMARY KEY, CompanyId INT NOT NULL, BranchCode VARCHAR(40) NOT NULL, BranchName VARCHAR(255) NOT NULL, " +
        "UNIQUE KEY uq_Branch_BranchCode (BranchCode), CONSTRAINT fk_Branch_Company FOREIGN KEY (CompanyId) REFERENCES Company (CompanyId));");

    db.Database.ExecuteSqlRaw(
        "CREATE TABLE IF NOT EXISTS LastUserCompanyAndBranch " +
        "(LastUserCompanyAndBranchId INT AUTO_INCREMENT PRIMARY KEY, UserName VARCHAR(256) NOT NULL, CompanyId INT NOT NULL, BranchId INT NOT NULL, " +
        "DateCreated DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, CreatedBy VARCHAR(255) NOT NULL, " +
        "DateModified DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, ModifiedBy VARCHAR(255) NOT NULL);");

    var hasCompanyId = db.Database.SqlQueryRaw<string>(
        "SELECT column_name FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = 'AspNetUsers' AND column_name = 'CompanyId' LIMIT 1").Any();

    var hasBranchId = db.Database.SqlQueryRaw<string>(
        "SELECT column_name FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = 'AspNetUsers' AND column_name = 'BranchId' LIMIT 1").Any();

    if (!hasCompanyId)
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE AspNetUsers ADD COLUMN CompanyId INT NULL");
    }

    if (!hasBranchId)
    {
        db.Database.ExecuteSqlRaw("ALTER TABLE AspNetUsers ADD COLUMN BranchId INT NULL");
    }
}

public partial class Program { }
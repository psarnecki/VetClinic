using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using QuestPDF.Infrastructure;
using VetClinicManager.Areas.Admin.Mappers;
using VetClinicManager.Data;
using VetClinicManager.Mappers;
using VetClinicManager.Mappers.Shared;
using VetClinicManager.Models;
using VetClinicManager.Services;

var logger = LogManager.Setup()
    .LoadConfigurationFromFile("nlog.config")
    .GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();
    builder.WebHost.UseStaticWebAssets();

    QuestPDF.Settings.License = LicenseType.Community;

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                           throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddIdentity<User, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Identity/Account/Login";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
        options.LogoutPath = "/Identity/Account/Logout";
    });

    builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add<VetClinicManager.Filters.GlobalExceptionFilter>();
    });
    builder.Services.AddRazorPages();

    builder.Services.AddTransient<SeedData>();
    builder.Services.AddTransient<IEmailSender, DummyEmailSender>();

    // Services
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IMedicationService, MedicationService>();
    builder.Services.AddScoped<IFileService, FileService>();
    builder.Services.AddScoped<IAnimalService, AnimalService>();
    builder.Services.AddScoped<IHealthRecordService, HealthRecordService>();
    builder.Services.AddScoped<IVisitService, VisitService>();
    builder.Services.AddScoped<IVisitUpdateService, VisitUpdateService>();
    builder.Services.AddScoped<IAnimalMedicationService, AnimalMedicationService>();
    builder.Services.AddScoped<IDashboardService, DashboardService>();

    // Mappers
    builder.Services.AddScoped<UserMapper>();
    builder.Services.AddScoped<MedicationMapper>();
    builder.Services.AddScoped<AnimalMapper>();
    builder.Services.AddScoped<HealthRecordMapper>();
    builder.Services.AddScoped<VisitMapper>();
    builder.Services.AddScoped<VisitUpdateMapper>();
    builder.Services.AddScoped<AnimalMedicationMapper>();
    builder.Services.AddScoped<DashboardMapper>();

    // Brief mappers
    builder.Services.AddScoped<UserBriefMapper>();
    builder.Services.AddScoped<MedicationBriefMapper>();
    builder.Services.AddScoped<AnimalBriefMapper>();
    builder.Services.AddScoped<VisitUpdateBriefMapper>();
    builder.Services.AddScoped<PrescriptionBriefMapper>();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var seedData = scope.ServiceProvider.GetRequiredService<SeedData>();

        try
        {
            await seedData.InitializeAsync();
        }
        catch (Exception ex)
        {
            var seedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            seedLogger.LogError(ex, "An error occurred seeding the DB.");
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapAreaControllerRoute(
        "Admin",
        "Admin",
        "Admin/{controller=Users}/{action=Index}/{id?}");

    app.MapControllerRoute(
        "default",
        "{controller=Home}/{action=Index}/{id?}");

    app.MapRazorPages();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Application stopped due to an unhandled exception.");
    throw;
}
finally
{
    LogManager.Shutdown();
}
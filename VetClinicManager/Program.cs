using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using VetClinicManager.Areas.Admin.Mappers;
using VetClinicManager.Data;
using VetClinicManager.Mappers;
using VetClinicManager.Models;
using VetClinicManager.Services;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddTransient<SeedData>();
builder.Services.AddTransient<IEmailSender, DummyEmailSender>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMedicationService, MedicationService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IAnimalService, AnimalService>();

builder.Services.AddScoped<UserMapper>();
builder.Services.AddScoped<MedicationMapper>();
builder.Services.AddScoped<AnimalMapper>();
builder.Services.AddScoped<UserBriefMapper>();

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
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
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
// Add these at the top
using EHRSystem.Core.Models;
using EHRSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EHRSystem.Core;
//using EHRSystem.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "EHR System API", Version = "v1" });
});

// Add Identity services
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<EhrDbContext>()
.AddDefaultTokenProviders();

// Replace existing DbContext registration with:
builder.Services.AddDbContext<EhrDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("EHRDatabase"),
        b => b.MigrationsAssembly("EHRSystem.Data")
    )
);

// [Requirement: US-AUTH-02] Configure session timeout
builder.Services.ConfigureApplicationCookie(options => {
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Configure Swagger
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EHR API v1"));

// Add these middleware components in order
//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed data
using (var scope = app.Services.CreateScope())
{
    await SeedData.Initialize(scope.ServiceProvider);
}

app.Run();

public partial class Program { } 
using DAL.Models;
using DAL.Repositories.Compatibilities;
using DAL.Repositories.Components;
using DAL.Repositories.ComponentTypes;
using DAL.Repositories.Configurations;
using DAL.Repositories.Images;
using DAL.Repositories.Logs;
using DAL.Repositories.Users;
using DAL.Services.Compatibilities;
using DAL.Services.Components;
using DAL.Services.ComponentTypes;
using DAL.Services.Configurations;
using DAL.Services.Images;
using DAL.Services.Logs;
using DAL.Services.Users;
using CarConfigurator_WebApp.MappingProfiles;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<CarConfiguratorDbContext>(options =>
{
    options.UseSqlServer("name=ConnectionStrings:Default");
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login";
        options.LogoutPath = "/User/Logout";
        options.AccessDeniedPath = "/User/Forbidden";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

builder.Services.AddAuthorization();

// Repositories
builder.Services.AddScoped<ICompatibilityRepository, CompatibilityRepository>();
builder.Services.AddScoped<IComponentRepository, ComponentRepository>();
builder.Services.AddScoped<IComponentTypeRepository, ComponentTypeRepository>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IApiLogRepository, ApiLogRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services
builder.Services.AddScoped<ICompatibilityService, CompatibilityService>();
builder.Services.AddScoped<IComponentService, ComponentService>();
builder.Services.AddScoped<IComponentTypeService, ComponentTypeService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IUserService, UserService>();

// AutoMapper (MVC MappingProfiles folder)
builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

app.UseDeveloperExceptionPage();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

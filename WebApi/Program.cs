using DAL.Models;
using DAL.Repositories.Compatibilities;
using DAL.Repositories.ComponentTypes;
using DAL.Repositories.Components;
using DAL.Repositories.Configurations;
using DAL.Repositories.Images;
using DAL.Repositories.Logs;
using DAL.Repositories.Users;
using DAL.Services.Compatibilities;
using DAL.Services.ComponentTypes;
using DAL.Services.Components;
using DAL.Services.Configurations;
using DAL.Services.Images;
using DAL.Services.Logs;
using DAL.Services.Users;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();

// Swagger + JWT support
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CarConfigurator API",
        Version = "v1"
    });

    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter valid JWT",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

// DbContext (DAL)
builder.Services.AddDbContext<CarConfiguratorDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// JWT authentication
var secureKey = builder.Configuration["Jwt:SecureKey"];
var key = Encoding.UTF8.GetBytes(secureKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

//Repositories
builder.Services.AddScoped<IComponentTypeRepository, ComponentTypeRepository>();
builder.Services.AddScoped<IComponentRepository, ComponentRepository>();
builder.Services.AddScoped<ICompatibilityRepository, CompatibilityRepository>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IApiLogRepository, ApiLogRepository>();

//Services
builder.Services.AddScoped<IComponentTypeService, ComponentTypeService>();
builder.Services.AddScoped<IComponentService, ComponentService>();
builder.Services.AddScoped<ICompatibilityService, CompatibilityService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILogService, LogService>();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Auth middleware 
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

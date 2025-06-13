using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using ErpPortal.Infrastructure;
using System;
using ErpPortal.Application.Interfaces.Repositories;
using ErpPortal.Infrastructure.Repositories;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Controller & JSON (ignore navigation loops)
builder.Services.AddControllersWithViews()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddInfrastructure(builder.Configuration);

// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKey123!@#$%^&*()";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ErpPortal";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ErpPortalUsers";

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Add Memory Cache
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IShopOrderOperationRepository, ShopOrderOperationRepository>();
builder.Services.AddScoped<IShopOrderRepository, ShopOrderRepository>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Use session
app.UseSession();

// Area routing önce tanımlanmalı ki /Admin/... yolları doğru eşleşsin.
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Users}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

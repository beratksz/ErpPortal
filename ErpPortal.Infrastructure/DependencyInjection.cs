using ErpPortal.Application.Interfaces.Repositories;
using ErpPortal.Application.Interfaces.Services;
using ErpPortal.Infrastructure.Data;
using ErpPortal.Infrastructure.Repositories;
using ErpPortal.Infrastructure.Services;
using ErpPortal.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ErpPortal.Infrastructure.ExternalServices.ShopOrder;
using System;
using System.Text;

namespace ErpPortal.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ErpPortalDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ErpPortalDbContext).Assembly.FullName)));

            services.AddScoped<IShopOrderOperationRepository, ShopOrderOperationRepository>();
            services.AddScoped<IWorkCenterRepository, WorkCenterRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IWorkLogRepository, WorkLogRepository>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IWorkCenterService, WorkCenterService>();
            services.AddScoped<IShopOrderService, Application.Services.ShopOrderService>();

            // Kalite: NCR entegrasyonu (typed HTTP client)
            services.AddHttpClient<INonConformanceApiService, ErpPortal.Infrastructure.ExternalServices.Quality.NonConformanceApiService>(client =>
            {
                client.BaseAddress = new Uri(configuration["IfsApi:BaseUrl"]
                    ?? throw new InvalidOperationException("IfsApi:BaseUrl is not configured."));

                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(
                    $"{configuration["IfsApi:Username"]}:{configuration["IfsApi:Password"]}"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            });
            services.AddScoped<IQualityService, ErpPortal.Application.Services.QualityService>();

            // Arka plan senkronizasyon servisi ve API servisi
            services.AddHttpClient<IShopOrderApiService, ShopOrderApiService>(client =>
            {
                client.BaseAddress = new Uri(configuration["IfsApi:BaseUrl"] 
                    ?? throw new InvalidOperationException("IfsApi:BaseUrl is not configured."));
                
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(
                    $"{configuration["IfsApi:Username"]}:{configuration["IfsApi:Password"]}"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            });

            // Otomatik kalite kapatma servisi (10 dk)
            services.AddHostedService<ErpPortal.Application.BackgroundServices.QualityAutoCloseService>();

            return services;
        }
    }
} 
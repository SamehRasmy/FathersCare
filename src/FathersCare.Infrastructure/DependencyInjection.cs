using FathersCare.Application.Abstractions;
using FathersCare.Infrastructure.Files;
using FathersCare.Infrastructure.Identity;
using FathersCare.Infrastructure.Notifications;
using FathersCare.Infrastructure.Persistence;
using FathersCare.Infrastructure.Reports;
using FathersCare.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FathersCare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<INotificationService, SignalRNotificationService>();
        services.AddScoped<IReportService, CompositeReportService>();
        services.AddScoped<ICareOverviewService, CareOverviewService>();
        services.AddScoped<IResidentManagementService, ResidentManagementService>();
        services.AddScoped<IMedicationManagementService, MedicationManagementService>();
        services.AddScoped<IRoomManagementService, RoomManagementService>();
        services.AddScoped<IMedicationRepository, MedicationRepository>();

        return services;
    }
}

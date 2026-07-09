using FathersCare.Application.Abstractions;
using FathersCare.Application.Residents.Commands.CreateResident;
using FathersCare.Application.Residents.Commands.UpdateResident;
using FathersCare.Application.Rooms.Commands.AssignResidentToRoom;
using FathersCare.Application.Rooms.Commands.CompleteRoomMaintenanceRequest;
using FathersCare.Application.Rooms.Commands.CreateFloor;
using FathersCare.Application.Rooms.Commands.CreateRoom;
using FathersCare.Application.Rooms.Commands.CreateRoomMaintenanceRequest;
using FathersCare.Application.Rooms.Commands.DeleteRoom;
using FathersCare.Application.Rooms.Commands.UpdateRoomCapacity;
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
        services.AddScoped<ICommandHandler<CreateResidentCommand, Guid>, CreateResidentCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateResidentCommand, bool>, UpdateResidentCommandHandler>();
        services.AddScoped<ICommandHandler<CreateFloorCommand, Guid>, CreateFloorCommandHandler>();
        services.AddScoped<ICommandHandler<CreateRoomCommand, Guid>, CreateRoomCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateRoomCapacityCommand, bool>, UpdateRoomCapacityCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteRoomCommand, bool>, DeleteRoomCommandHandler>();
        services.AddScoped<ICommandHandler<AssignResidentToRoomCommand, bool>, AssignResidentToRoomCommandHandler>();
        services.AddScoped<ICommandHandler<CreateRoomMaintenanceRequestCommand, Guid>, CreateRoomMaintenanceRequestCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteRoomMaintenanceRequestCommand, bool>, CompleteRoomMaintenanceRequestCommandHandler>();

        return services;
    }
}

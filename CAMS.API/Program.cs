﻿using CAMS.API.Extensions;
using CAMS.API.Helpers;
using CAMS.API.MiddleWares;
using CAMS.API.MiddleWares.Auth;
using CAMS.DependencyInjection;
using CAMS.Infrastructure.Seeding;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>(); // 👈 Load from User Secrets

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

    // This forces consistent enum handling across all serialization contexts
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
    options.JsonSerializerOptions.WriteIndented = false; // For production
});

builder.Services.OptionsPatternConfig(builder.Configuration); // belong IOptions Pattern

builder.Services.AddDataProtection();
builder.Services.AddMemoryCache();

// ✅ Configure Serilog from `appsettings.json`
//builder.Host.UseCustomSerilog();

// Call Container here

builder.Services.RegisterConfig(builder.Configuration);
builder.Services.AddAuthentication(builder.Configuration);
builder.Services.RegisterIdentityConfig();
builder.Services.AddHttpContextAccessor();
builder.Services.RegisterFluentValidationSettings();


// for permission based authorization

//builder.Services.AddTransient(typeof(IAuthorizationHandler), typeof(PermissionAuthorizationHandler));
//builder.Services.AddTransient(typeof(IAuthorizationPolicyProvider), typeof(PermissionAuthorizationPolicyProvider));
builder.Services.AddSingleton<AppointmentJob>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddSwaggerServices();

// Call Seed Data

await builder.Services.Seeding();


#region For Validation Error

builder.Services.Configure();

#endregion
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed roles and permissions on startup
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    try
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedIdentityData.SeedRolesAndPermissionsAsync(roleManager);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding roles and permissions: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwagger();
}

app.UseStatusCodePagesWithRedirects("/errors/{0}");

app.UseHttpsRedirection();

app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

//app.UseStaticFiles();  // it's very very Important after added wwwroot folder and folder of images that belong each entity. 

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
    }
});

// ✅ Log every request in a structured way
//app.UseSerilogRequestLogging();

//app.UseRateLimiter();

app.UseAuthentication();
app.UseMiddleware<PermissionAuthorizationMiddleware>(); // your custom permission check
app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<CalculateTimeOfRequest>();
app.UseMiddleware<ErrorHandlingMiddleWare>();


app.MapControllers();
// ✅ Schedule the Recurring Job
//ScheduleRecurringJob(app.Services);

app.Run();


void ScheduleRecurringJob(IServiceProvider services)
{
    using (var scope = services.CreateScope())
    {
        var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        var job = scope.ServiceProvider.GetRequiredService<AppointmentJob>();

        string recurringJobId = "activateEmployeesJob";

        recurringJobManager.AddOrUpdate(
            recurringJobId,
            () => job.Run(), // ✅ Uses DI properly
            Cron.Daily
        );
    }
}

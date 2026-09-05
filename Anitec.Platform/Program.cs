using Anitec.Platform.Activities.Application.CommandServices;
using Anitec.Platform.Activities.Application.Internal.CommandServices;
using Anitec.Platform.Activities.Application.Internal.QueryServices;
using Anitec.Platform.Activities.Application.QueryServices;
using Anitec.Platform.Activities.Domain.Repositories;
using Anitec.Platform.Activities.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Anitec.Platform.Analytics.Application.CommandServices;
using Anitec.Platform.Analytics.Application.Internal.CommandServices;
using Anitec.Platform.Analytics.Application.Internal.QueryServices;
using Anitec.Platform.Analytics.Application.QueryServices;
using Anitec.Platform.Analytics.Domain.Repositories;
using Anitec.Platform.Analytics.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Anitec.Platform.Clients.Application.CommandServices;
using Anitec.Platform.Clients.Application.Internal.CommandServices;
using Anitec.Platform.Clients.Application.Internal.QueryServices;
using Anitec.Platform.Clients.Application.QueryServices;
using Anitec.Platform.Clients.Domain.Repositories;
using Anitec.Platform.Clients.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Anitec.Platform.Devices.Application.CommandServices;
using Anitec.Platform.Devices.Application.Internal.CommandServices;
using Anitec.Platform.Devices.Application.Internal.QueryServices;
using Anitec.Platform.Devices.Application.QueryServices;
using Anitec.Platform.Devices.Domain.Repositories;
using Anitec.Platform.Devices.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Anitec.Platform.Financial.Application.CommandServices;
using Anitec.Platform.Financial.Application.Internal.CommandServices;
using Anitec.Platform.Financial.Application.Internal.QueryServices;
using Anitec.Platform.Financial.Application.QueryServices;
using Anitec.Platform.Financial.Domain.Repositories;
using Anitec.Platform.Financial.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Anitec.Platform.Iam.Application.Acl;
using Anitec.Platform.Iam.Application.CommandServices;
using Anitec.Platform.Iam.Application.Internal.CommandServices;
using Anitec.Platform.Iam.Application.Internal.OutboundServices;
using Anitec.Platform.Iam.Application.Internal.QueryServices;
using Anitec.Platform.Iam.Application.QueryServices;
using Anitec.Platform.Iam.Domain.Repositories;
using Anitec.Platform.Iam.Infrastructure.Hashing.BCrypt.Services;
using Anitec.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;
using Anitec.Platform.Iam.Infrastructure.Tokens.Jwt.Configuration;
using Anitec.Platform.Iam.Infrastructure.Tokens.Jwt.Services;
using Anitec.Platform.Iam.Interfaces.Acl;
using Anitec.Platform.Iam.Resources;
using Anitec.Platform.Livestock.Application.CommandServices;
using Anitec.Platform.Livestock.Application.Internal.CommandServices;
using Anitec.Platform.Livestock.Application.Internal.QueryServices;
using Anitec.Platform.Livestock.Application.QueryServices;
using Anitec.Platform.Livestock.Domain.Repositories;
using Anitec.Platform.Livestock.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Anitec.Platform.Metrics.Application.CommandServices;
using Anitec.Platform.Metrics.Application.Internal.CommandServices;
using Anitec.Platform.Metrics.Application.Internal.QueryServices;
using Anitec.Platform.Metrics.Application.QueryServices;
using Anitec.Platform.Metrics.Domain.Repositories;
using Anitec.Platform.Metrics.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Anitec.Platform.Profiles.Application.CommandServices;
using Anitec.Platform.Profiles.Application.Internal.CommandServices;
using Anitec.Platform.Profiles.Application.Internal.QueryServices;
using Anitec.Platform.Profiles.Application.QueryServices;
using Anitec.Platform.Profiles.Domain.Repositories;
using Anitec.Platform.Profiles.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Anitec.Platform.Profiles.Resources;
using Anitec.Platform.Resources.Errors;
using Anitec.Platform.Resources.Shared;
using Anitec.Platform.Sanitary.Application.CommandServices;
using Anitec.Platform.Sanitary.Application.Internal.CommandServices;
using Anitec.Platform.Sanitary.Application.Internal.QueryServices;
using Anitec.Platform.Sanitary.Application.QueryServices;
using Anitec.Platform.Sanitary.Domain.Repositories;
using Anitec.Platform.Sanitary.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Anitec.Platform.Shared.Domain.Repositories;
using Anitec.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;
using Anitec.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Anitec.Platform.Shared.Infrastructure.Pipeline.Middleware.Extensions;
using Anitec.Platform.Subscriptions.Application.CommandServices;
using Anitec.Platform.Subscriptions.Application.Internal.CommandServices;
using Anitec.Platform.Subscriptions.Application.Internal.QueryServices;
using Anitec.Platform.Subscriptions.Application.QueryServices;
using Anitec.Platform.Subscriptions.Domain.Repositories;
using Anitec.Platform.Subscriptions.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Cortex.Mediator.Commands;
using Cortex.Mediator.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi;
using ProblemDetailsFactory = Anitec.Platform.Shared.Interfaces.Rest.ProblemDetails.ProblemDetailsFactory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    .AddDataAnnotationsLocalization();

builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var connectionStringTemplate = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionStringTemplate))
        throw new InvalidOperationException("Database connection string is not set in the configuration.");

    options.UseMySQL(connectionStringTemplate)
        .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
        .EnableDetailedErrors();

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddSingleton<IStringLocalizer<ErrorMessages>, StringLocalizer<ErrorMessages>>();
builder.Services.AddSingleton<IStringLocalizer<CommonMessages>, StringLocalizer<CommonMessages>>();
builder.Services.AddSingleton<IStringLocalizer<IamMessages>, StringLocalizer<IamMessages>>();
builder.Services.AddSingleton<IStringLocalizer<ProfilesMessages>, StringLocalizer<ProfilesMessages>>();
builder.Services.AddSingleton<ProblemDetailsFactory>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "AniTec Platform",
            Version = "v1",
            Description = "Backend API for livestock, devices, metrics, activities, analytics and subscriptions.",
            TermsOfService = new Uri("https://anitec.local/tos"),
            Contact = new OpenApiContact
            {
                Name = "AniTec Team",
                Email = "team@anitec.local"
            },
            License = new OpenApiLicense
            {
                Name = "Apache 2.0",
                Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0.html")
            }
        });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter the JWT token. Swagger will send it as Bearer token.",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] });
    options.EnableAnnotations();
});

// Shared Bounded Context
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Livestock Bounded Context
builder.Services.AddScoped<IHerdRepository, HerdRepository>();
builder.Services.AddScoped<IHerdCommandService, HerdCommandService>();
builder.Services.AddScoped<IHerdQueryService, HerdQueryService>();
builder.Services.AddScoped<IAnimalRepository, AnimalRepository>();
builder.Services.AddScoped<IAnimalCommandService, AnimalCommandService>();
builder.Services.AddScoped<IAnimalQueryService, AnimalQueryService>();

// Sanitary Bounded Context
builder.Services.AddScoped<IHealthEventRepository, HealthEventRepository>();
builder.Services.AddScoped<IHealthEventCommandService, HealthEventCommandService>();
builder.Services.AddScoped<IHealthEventQueryService, HealthEventQueryService>();

// Financial Bounded Context
builder.Services.AddScoped<IFinancialRecordRepository, FinancialRecordRepository>();
builder.Services.AddScoped<IFinancialRecordCommandService, FinancialRecordCommandService>();
builder.Services.AddScoped<IFinancialRecordQueryService, FinancialRecordQueryService>();

// Activities Bounded Context
builder.Services.AddScoped<IFarmActivityRepository, FarmActivityRepository>();
builder.Services.AddScoped<IFarmActivityCommandService, FarmActivityCommandService>();
builder.Services.AddScoped<IFarmActivityQueryService, FarmActivityQueryService>();

// Analytics Bounded Context
builder.Services.AddScoped<IReportMetricRepository, ReportMetricRepository>();
builder.Services.AddScoped<IReportMetricCommandService, ReportMetricCommandService>();
builder.Services.AddScoped<IReportMetricQueryService, ReportMetricQueryService>();

// Clients Bounded Context
builder.Services.AddScoped<IVeterinarianClientRepository, VeterinarianClientRepository>();
builder.Services.AddScoped<IVeterinarianClientCommandService, VeterinarianClientCommandService>();
builder.Services.AddScoped<IVeterinarianClientQueryService, VeterinarianClientQueryService>();

// Devices Bounded Context
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IDeviceCommandService, DeviceCommandService>();
builder.Services.AddScoped<IDeviceQueryService, DeviceQueryService>();

// Metrics Bounded Context
builder.Services.AddScoped<IDeviceMetricRepository, DeviceMetricRepository>();
builder.Services.AddScoped<IDeviceMetricCommandService, DeviceMetricCommandService>();
builder.Services.AddScoped<IDeviceMetricQueryService, DeviceMetricQueryService>();

// Subscriptions Bounded Context
builder.Services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
builder.Services.AddScoped<ISubscriptionPlanCommandService, SubscriptionPlanCommandService>();
builder.Services.AddScoped<ISubscriptionPlanQueryService, SubscriptionPlanQueryService>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<ISubscriptionCommandService, SubscriptionCommandService>();
builder.Services.AddScoped<ISubscriptionQueryService, SubscriptionQueryService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentCommandService, PaymentCommandService>();
builder.Services.AddScoped<IPaymentQueryService, PaymentQueryService>();

// Profiles Bounded Context
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IProfileCommandService, ProfileCommandService>();
builder.Services.AddScoped<IProfileQueryService, ProfileQueryService>();

// IAM Bounded Context
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddScoped<IIamContextFacade, IamContextFacade>();

builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(LoggingCommandBehavior<>));
builder.Services.AddCortexMediator([typeof(Program)]);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();

    if (app.Environment.IsDevelopment())
    {
        var hashingService = services.GetRequiredService<IHashingService>();
        await AppDbContextSeeder.SeedDevelopmentDataAsync(context, hashingService);
    }
}

app.UseGlobalExceptionHandler();

var supportedCultures = new[] { "en", "es" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAllPolicy");
app.UseRequestAuthorization();
if (!app.Environment.IsProduction())
    app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

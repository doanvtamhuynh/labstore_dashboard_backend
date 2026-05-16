using System.Reflection;
using System.Text;
using backend.src.Config;
using backend.src.Helpers;
using backend.src.Hubs;
using backend.src.Middlewares;
using backend.src.Models;
using backend.src.Repositories;
using backend.src.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MongoDB.Driver;

EnvLoader.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);

var mongoOptions = new MongoDbOptions
{
    ConnectionString = Environment.GetEnvironmentVariable("MONGODB_URI") ?? builder.Configuration["MongoDb:ConnectionString"] ?? string.Empty,
    DatabaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE") ?? builder.Configuration["MongoDb:DatabaseName"] ?? "labstore"
};

var jwtOptions = new JwtOptions
{
    Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"] ?? "Labstore",
    Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"] ?? "LabstoreDashboard",
    Secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? builder.Configuration["Jwt:Secret"] ?? string.Empty,
    AccessTokenMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_MINUTES"), out var minutes) ? minutes : 30,
    RefreshTokenDays = int.TryParse(Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_DAYS"), out var days) ? days : 7
};

var cloudinaryOptions = new CloudinaryOptions
{
    CloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? string.Empty,
    ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? string.Empty,
    ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? string.Empty,
    UploadFolder = Environment.GetEnvironmentVariable("CLOUDINARY_UPLOAD_FOLDER") ?? "labstore/products",
    LocalUploadRoot = Environment.GetEnvironmentVariable("LOCAL_UPLOAD_ROOT") ?? "wwwroot/uploads"
};

var smtpOptions = new SmtpOptions
{
    Host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? string.Empty,
    Port = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var smtpPort) ? smtpPort : 587,
    Username = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? string.Empty,
    Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? string.Empty,
    EnableSsl = !bool.TryParse(Environment.GetEnvironmentVariable("SMTP_ENABLE_SSL"), out var smtpSsl) || smtpSsl,
    FromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? "no-reply@labstore.local",
    FromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? "Labstore",
    DefaultRecipients = Environment.GetEnvironmentVariable("SMTP_DEFAULT_RECIPIENTS") ?? string.Empty
};

var backupOptions = new BackupOptions
{
    Directory = Environment.GetEnvironmentVariable("BACKUP_DIRECTORY") ?? "backups"
};

if (string.IsNullOrWhiteSpace(mongoOptions.ConnectionString))
{
    throw new InvalidOperationException("MONGODB_URI is required");
}

if (jwtOptions.Secret.Length < 32)
{
    throw new InvalidOperationException("JWT_SECRET must contain at least 32 characters");
}

builder.Services.AddSingleton(Options.Create(mongoOptions));
builder.Services.AddSingleton(Options.Create(jwtOptions));
builder.Services.AddSingleton(Options.Create(cloudinaryOptions));
builder.Services.AddSingleton(Options.Create(smtpOptions));
builder.Services.AddSingleton(Options.Create(backupOptions));
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoOptions.ConnectionString));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoOptions.DatabaseName));
builder.Services.AddHttpClient();

builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerOrderRepository, CustomerOrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.AddScoped<ICrudRepository<Coupon>>(sp => new MongoCrudRepository<Coupon>(sp.GetRequiredService<IMongoDatabase>(), "promotion_coupons"));
builder.Services.AddScoped<ICrudRepository<FlashSale>>(sp => new MongoCrudRepository<FlashSale>(sp.GetRequiredService<IMongoDatabase>(), "promotion_flash_sales"));
builder.Services.AddScoped<ICrudRepository<PromoBanner>>(sp => new MongoCrudRepository<PromoBanner>(sp.GetRequiredService<IMongoDatabase>(), "promotion_banners"));
builder.Services.AddScoped<ICrudRepository<AffiliateProgram>>(sp => new MongoCrudRepository<AffiliateProgram>(sp.GetRequiredService<IMongoDatabase>(), "promotion_affiliates"));
builder.Services.AddScoped<ICrudRepository<EmailCampaign>>(sp => new MongoCrudRepository<EmailCampaign>(sp.GetRequiredService<IMongoDatabase>(), "promotion_email_campaigns"));
builder.Services.AddScoped<ICrudRepository<ShippingConfig>>(sp => new MongoCrudRepository<ShippingConfig>(sp.GetRequiredService<IMongoDatabase>(), "shipping_configs"));
builder.Services.AddScoped<ICrudRepository<ShippingProvider>>(sp => new MongoCrudRepository<ShippingProvider>(sp.GetRequiredService<IMongoDatabase>(), "shipping_providers"));
builder.Services.AddScoped<ICrudRepository<Warehouse>>(sp => new MongoCrudRepository<Warehouse>(sp.GetRequiredService<IMongoDatabase>(), "shipping_warehouses"));
builder.Services.AddScoped<ICrudRepository<ShippingReturn>>(sp => new MongoCrudRepository<ShippingReturn>(sp.GetRequiredService<IMongoDatabase>(), "shipping_returns"));
builder.Services.AddScoped<ICrudRepository<SupportTicket>>(sp => new MongoCrudRepository<SupportTicket>(sp.GetRequiredService<IMongoDatabase>(), "support_tickets"));
builder.Services.AddScoped<ICrudRepository<FaqItem>>(sp => new MongoCrudRepository<FaqItem>(sp.GetRequiredService<IMongoDatabase>(), "support_faq"));
builder.Services.AddScoped<ICrudRepository<Notification>>(sp => new MongoCrudRepository<Notification>(sp.GetRequiredService<IMongoDatabase>(), "notifications"));
builder.Services.AddScoped<ICrudRepository<SeoMetaEntry>>(sp => new MongoCrudRepository<SeoMetaEntry>(sp.GetRequiredService<IMongoDatabase>(), "seo_meta"));
builder.Services.AddScoped<ICrudRepository<BlogPost>>(sp => new MongoCrudRepository<BlogPost>(sp.GetRequiredService<IMongoDatabase>(), "blog_posts"));
builder.Services.AddScoped<ICrudRepository<StaticPage>>(sp => new MongoCrudRepository<StaticPage>(sp.GetRequiredService<IMongoDatabase>(), "pages"));
builder.Services.AddScoped<ICrudRepository<SeoRedirect>>(sp => new MongoCrudRepository<SeoRedirect>(sp.GetRequiredService<IMongoDatabase>(), "seo_redirects"));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IShippingService, ShippingService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ISupportService, SupportService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Labstore Admin Dashboard API", Version = "v1" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT access token."
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("ManagerOrHigher", policy => policy.RequireRole("SuperAdmin", "Manager"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("DashboardCors", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:3000", "http://127.0.0.1:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("DashboardCors");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationsHub>("/hubs/notifications");

app.Run("http://localhost:5000");

using System.Reflection;
using System.Text;
using backend.src.Config;
using backend.src.Helpers;
using backend.src.Middlewares;
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
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoOptions.ConnectionString));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoOptions.DatabaseName));

builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddControllers();
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
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("DashboardCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run("http://localhost:5000");

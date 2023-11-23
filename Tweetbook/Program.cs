using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Tweetbook.Cache;
using System.Text.Json;
using Tweetbook.Contracts.HealthChecks;
using Tweetbook.Data;
using Tweetbook.Filters;
using Tweetbook.Options;
using Tweetbook.Services;
using SwaggerOptions = Tweetbook.Options.SwaggerOptions;
using StackExchange.Redis;
using Tweetbook.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
    {
        // Add input validation filters middleware (to be used with FluentValidation).
        options.Filters.Add<ValidationFilter>();
    });

// Register our DataContext as a service.
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DataContext>();

builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();

// Add JWT Authentication
var jwtSettings = new JwtSettings();
builder.Configuration.Bind(nameof(jwtSettings), jwtSettings);
builder.Services.AddSingleton(jwtSettings);

var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
    ValidateIssuer = false,
    ValidateAudience = false,
    RequireExpirationTime = false,
    ValidateLifetime = true
};
builder.Services.AddSingleton(tokenValidationParameters);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.SaveToken = true;
    x.TokenValidationParameters = tokenValidationParameters;
});

builder.Services.AddAuthorization();

// Add our UriService
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IUriService>(provider =>
{
    var accessor = provider.GetRequiredService<IHttpContextAccessor>();
    var request = accessor.HttpContext.Request;
    var absoluteUri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent(), "/");
    return new UriService(absoluteUri);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.SwaggerDoc("v1", new OpenApiInfo { Title = "Tweetbook API", Version = "v1" });

    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    x.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {new OpenApiSecurityScheme{Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }}, new List<string>()}
    });
});

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddFluentValidationAutoValidation().AddValidatorsFromAssemblyContaining<Program>();

// Redis cache (with health checks)
var redisCacheSettings = new RedisCacheSettings();
builder.Configuration.GetSection(nameof(RedisCacheSettings)).Bind(redisCacheSettings);
builder.Services.AddSingleton(redisCacheSettings);
if (redisCacheSettings.Enabled)
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
        ConnectionMultiplexer.Connect(redisCacheSettings.ConnectionString)
    );
    builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redisCacheSettings.ConnectionString);
    builder.Services.AddSingleton<IResponseCacheService, ResponseCacheService>();
}

// Adding (DataContext) health checks (see further configs below in the app)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<DataContext>();
    // Removing Redis health check for now
    //.AddCheck<RedisHealthCheck>("Redis");

var app = builder.Build();

using(var serviceScope = app.Services.CreateAsyncScope())
{
    //Note: this applies DB migrations every time the app is run.
    // This shouldn't be done in PROD environments.
    //var dbContext = serviceScope.ServiceProvider.GetRequiredService<DataContext>();
    //await dbContext.Database.MigrateAsync();

   var roleManager  = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
   if (!await roleManager.RoleExistsAsync("Admin"))
   {
        var adminRole = new IdentityRole("Admin");
        await roleManager.CreateAsync(adminRole);
   }

   if (!await roleManager.RoleExistsAsync("Poster"))
   {
        var posterRole = new IdentityRole("Poster");
        await roleManager.CreateAsync(posterRole);
   }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    var swaggerOptions = new SwaggerOptions();
    builder.Configuration.GetSection(nameof(SwaggerOptions)).Bind(swaggerOptions);
    app.UseSwagger(option => { option.RouteTemplate = swaggerOptions.JsonRoute; });
    app.UseSwaggerUI(option =>
    {
        option.SwaggerEndpoint(swaggerOptions.UiEndpoint, swaggerOptions.Description);
    });
}

// Removed in order to run and test the app in Docker (otherwhise need to setup certificates for the container, etc.)
//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Configuring the health check
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new HealthCheckResponse
        {
            Status = report.Status.ToString(),
            Checks = report.Entries.Select(x => new HealthCheck
            {
                Component = x.Key,
                Status = x.Value.Status.ToString(),
                Description = x.Value.Description
            }),
            Duration = report.TotalDuration
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

app.MapControllers();

// Note: see note above about DB migrations at app start. Remove the await and
// change to app.Run(); for normal app start when not using the DB migration code above.
await app.RunAsync();

// Make this class available for integration tests
public partial class Program { }
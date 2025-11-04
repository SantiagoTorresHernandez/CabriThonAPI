using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using CabriThonAPI.Application.Interfaces;
using CabriThonAPI.Application.Services;
using CabriThonAPI.Domain.Interfaces;
using CabriThonAPI.Infrastructure.Data;
using CabriThonAPI.Infrastructure.Repositories;
using CabriThonAPI.Infrastructure.Services;
using CabriThonAPI.WebAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        // Use in-memory database for development/testing
        options.UseInMemoryDatabase("CabriThonDB");
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForDevelopmentPurposes123456789";
var issuer = jwtSettings["Issuer"] ?? "CabriThonAPI";
var audience = jwtSettings["Audience"] ?? "CabriThonClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };

    // For development: allow API Keys as alternative authentication
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
            if (!string.IsNullOrEmpty(apiKey))
            {
                // In production, validate API key against database
                // For now, accept any API key for development
                context.HttpContext.Items["ClientId"] = apiKey;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Register Repositories
builder.Services.AddScoped<IPromotionSuggestionRepository, PromotionSuggestionRepository>();
builder.Services.AddScoped<IOrderSuggestionRepository, OrderSuggestionRepository>();
builder.Services.AddScoped<IImpactMetricRepository, ImpactMetricRepository>();

// Register Application Services
builder.Services.AddScoped<ISuggestionService, SuggestionService>();
builder.Services.AddScoped<IMetricsService, MetricsService>();
builder.Services.AddScoped<IAIAgentService, AIAgentService>();

// Register Infrastructure Services
builder.Services.AddScoped<IGeminiAIService, GeminiAIService>();
builder.Services.AddHttpClient<IExternalDataService, ExternalDataService>();

// Add Controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CabriThon AI Agents & Suggestions API",
        Version = "v1",
        Description = "Backend API for AI-powered business intelligence, suggestions, and metrics"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CabriThon API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseJwtMiddleware();

app.MapControllers();

// Initialize database with sample data in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Ensure database is created
    context.Database.EnsureCreated();
}

app.Run();

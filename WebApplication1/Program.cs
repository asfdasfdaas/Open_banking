using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using WebApplication1.Data;
using WebApplication1.Interface;
using WebApplication1.Repository;
using WebApplication1.Services;
using WebApplication1.Services.Providers;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMemoryCache();

builder.Services.AddCors();

builder.Services.AddHttpClient<IBankIntegrationService, VakifbankIntegrationService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Vakifbank:BaseUrl"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddHttpClient<IGeminiIntegrationService, GeminiIntegrationService>();

builder.Services.AddScoped<IAiAssistantService, AiAssistantService>();

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IVakifbankSyncService, VakifbankSyncService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey("jwt_token"))
                {
                    context.Token = context.Request.Cookies["jwt_token"];
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                // Grab the cache
                var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();

                // cast the token to the JsonWebToken to get the raw string
                if (context.SecurityToken is Microsoft.IdentityModel.JsonWebTokens.JsonWebToken modernToken)
                {
                    var rawTokenString = modernToken.EncodedToken;

                    // Check the blacklist
                    if (cache.TryGetValue(rawTokenString, out _))
                    {
                        context.Fail("This token has been revoked due to logout.");
                    }
                }
                // legacy handler
                else if (context.SecurityToken is JwtSecurityToken legacyToken)
                {
                    if (cache.TryGetValue(legacyToken.RawData, out _))
                    {
                        context.Fail("This token has been revoked due to logout.");
                    }
                }

                return Task.CompletedTask;
            }
        };

    });
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Open Banking API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[]{}
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(options =>
    options.WithOrigins("http://localhost:4200")
           .AllowAnyMethod()
           .AllowCredentials()
           .AllowAnyHeader());

app.UseHttpsRedirection();

app.UseMiddleware<WebApplication1.Middleware.ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();



app.MapControllers();

await app.RunAsync();
using GeographyQuiz.Exceptions;
using GeographyQuiz.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// 1. SERVICES (Dependency Injection)
// ------------------------------------------------------------

// Controllers
builder.Services.AddControllers();

// CountryService (NYTT — ditt spel)
//builder.Services.AddScoped<ICountryService, CountryService>();

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();


// Typed HttpClient för API Ninjas
builder.Services.AddHttpClient<ICountryService, CountryService>(client =>
{
    client.BaseAddress = new Uri("https://api.api-ninjas.com/v1/country");
    client.DefaultRequestHeaders.Add("X-Api-Key",
        builder.Configuration["ApiNinjas:ApiKey"]);
});

builder.Services.AddSwaggerGen(options =>
{
    // 1. Definiera att API:et använder Bearer Tokens (JWT) för säkerhet
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Klistra in din JWT-token här!"
    });

    // 2. Tvinga dokumentationsgränssnittet (Swagger/Scalar) att använda låset på alla endpoints
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ProblemDetails (RFC 7807)
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
    };
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HybridCache (för caching)
#pragma warning disable EXTEXP0018
builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ------------------------------------------------------------
// 2. MIDDLEWARE PIPELINE
// ------------------------------------------------------------

// Custom Exception Middleware (HÖGST UPP)
app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var exception = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()
            ?.Error;

        var problemDetails = exception switch
        {
            NotFoundException ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = ex.Message
            },
            BadRequestException ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = 400,
                Title = "Bad Request",
                Detail = ex.Message
            },
            AlreadyAnsweredException ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = 400,
                Title = "Bad Request",
                Detail = ex.Message
            },
            GameOverException ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = 400,
                Title = "Bad Request",
                Detail = ex.Message
            },
            MustAnswerException ex => new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = 400,
                Title = "Bad Request",
                Detail = ex.Message
            },
            _ => new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = "Ett oväntat fel inträffade. Försök igen senare."
            }
        };

        context.Response.StatusCode = problemDetails.Status ?? 500;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

// Swagger i utvecklingsmiljö
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

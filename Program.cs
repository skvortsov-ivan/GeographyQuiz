using GeographyQuiz.Exceptions;
using GeographyQuiz.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. SERVICES (Dependency Injection)

// Add Controllers
builder.Services.AddControllers();

// Register application services
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();


// Typed HttpClient for API Ninjas (country data)
builder.Services.AddHttpClient<ICountryService, CountryService>(client =>
{
    client.BaseAddress = new Uri("https://api.api-ninjas.com/v1/country");
    client.DefaultRequestHeaders.Add("X-Api-Key",
        builder.Configuration["ApiNinjas:ApiKey"]);
});

// Swagger configuration with JWT support
builder.Services.AddSwaggerGen(options =>
{
    // Define Bearer authentication scheme
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Klistra in din JWT-token här!"
    });

    // Require JWT for all endpoints in Swagger UI
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

// ProblemDetails for standardized error responses (RFC 7807)
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
    };
});

// Rate limiting configuration
builder.Services.AddRateLimiter(options =>
{
    // Return 429 when rate limit is exceeded
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Fixed window limiter (used for POST endpoints)
    options.AddFixedWindowLimiter("fixed", config =>
    {
        config.Window = TimeSpan.FromMinutes(1); // Fönstret är 1 minut långt
        config.PermitLimit = 10; // Man får göra 6 anrop under denna minut
        config.QueueLimit = 0; // Vi tillåter ingen kö just nu. Blir det fullt så är det tvärnit.
    });

    // Sliding window limiter (used for GET endpoints)
    options.AddSlidingWindowLimiter("sliding", config =>
    {
        config.Window = TimeSpan.FromMinutes(1);
        config.SegmentsPerWindow = 10;
        config.PermitLimit = 10;
    });
});

// Swagger endpoint
builder.Services.AddEndpointsApiExplorer();

// HybridCache for caching API responses
#pragma warning disable EXTEXP0018
builder.Services.AddHybridCache();
#pragma warning restore EXTEXP0018

// JWT authentication configuration
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

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// Build the application
var app = builder.Build();



// 2. MIDDLEWARE PIPELINE

// Global exception handler
app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var exception = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()
            ?.Error;

        // Map custom exceptions to ProblemDetails
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

// Enable Swagger in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirects all HTTP requests to HTTPS
app.UseHttpsRedirection();

// Enable routing
app.UseRouting();

// Enable rate limiting
app.UseRateLimiter();

// Enable authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

// Start the application
app.Run();

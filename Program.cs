using GeographyQuiz.Exceptions;
using GeographyQuiz.Services;


var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// 1. SERVICES (Dependency Injection)
// ------------------------------------------------------------

// Controllers
builder.Services.AddControllers();

// CountryService (NYTT — ditt spel)
//builder.Services.AddScoped<ICountryService, CountryService>();

// Typed HttpClient för API Ninjas
builder.Services.AddHttpClient<ICountryService, CountryService>(client =>
{
    client.BaseAddress = new Uri("https://api.api-ninjas.com/v1/country");
    client.DefaultRequestHeaders.Add("X-Api-Key",
        builder.Configuration["ApiNinjas:ApiKey"]);
});

// Preloading countries
// builder.Services.AddHostedService<CountryPreloadService>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();

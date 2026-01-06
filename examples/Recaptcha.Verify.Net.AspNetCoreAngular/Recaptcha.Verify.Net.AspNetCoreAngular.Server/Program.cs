using Recaptcha.Verify.Net.AspNetCoreAngular.Server.Models;
using Recaptcha.Verify.Net.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRecaptcha(builder.Configuration.GetSection("Recaptcha"))
    .AddRecaptchaActionArgumentsTokenExtractor(x =>
    {
        if (x.TryGetValue("credentials", out var credentials))
        {
            return (credentials as BaseRecaptchaCredentials)?.RecaptchaToken;
        }

        return null;
    });

builder.Services.AddLogging();

builder.Services.AddControllers();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

app.UseAuthorization();

app.MapControllers();

app.Run();

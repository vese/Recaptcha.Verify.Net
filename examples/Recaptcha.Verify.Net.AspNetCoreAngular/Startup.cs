using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Recaptcha.Verify.Net.AspNetCoreAngular.Models;
using Recaptcha.Verify.Net.Configuration;
using System;
using System.Threading.Tasks;

namespace Recaptcha.Verify.Net.AspNetCoreAngular
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRecaptcha(builder =>
            {
                builder.Configure(Configuration.GetSection("Recaptcha"),
                    // Retrieve token from parsed action arguments handler used in RecaptchaAttribute
                    o => o.AttributeOptions.GetResponseTokenFromActionArguments =
                        d =>
                        {
                            if (d.TryGetValue("credentials", out var credentials))
                            {
                                return ((BaseRecaptchaCredentials)credentials).RecaptchaToken;
                            }
                            return null;
                        });
                builder.ConfigureLogging(o =>
                {
                    // Enable logging exceptions. (Do not enable if logging performed in catch or RecaptchaOptions handlers)
                    o.EnableExceptionLogging = true;
                    o.UpdateLevels((RecaptchaServiceEventId.ServiceException, LogLevel.Critical));
                    // Custom log message handler
                    o.LogSendingRequestMessageHandler = (level, id, message, token, ip) =>
                    {
                        Console.WriteLine(level);
                        Console.WriteLine(id);
                        Console.WriteLine(string.Format(message, token, ip));
                    };
                    // Custom log message handler using async logging
                    o.LogRequestSuccededMessageHandler = (level, id, message, response) =>
                    {
                        Task.Run(async () =>
                        {
                            //await LogAsync(level, id, message, args);
                        });
                    };
                });
            });

            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}

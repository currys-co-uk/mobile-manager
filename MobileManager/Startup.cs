using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MobileManager.Appium;
using MobileManager.Configuration;
using MobileManager.Configuration.ConfigurationProvider;
using MobileManager.Configuration.Interfaces;
using MobileManager.Controllers;
using MobileManager.Controllers.Interfaces;
using MobileManager.Database;
using MobileManager.Database.Extensions;
using MobileManager.Database.Repositories;
using MobileManager.Database.Repositories.Interfaces;
using MobileManager.Http.Clients;
using MobileManager.Http.Clients.Interfaces;
using MobileManager.Logging.Logger;
using MobileManager.Models.Devices;
using MobileManager.Models.Logger;
using MobileManager.Models.Reservations;
using MobileManager.Services;
using MobileManager.Services.Interfaces;
using Newtonsoft.Json;
using React.AspNet;
using Swashbuckle.AspNetCore.Swagger;

namespace MobileManager
{
    /// <summary>
    /// Startup.
    /// </summary>
    public class Startup
    {
        private readonly CancellationTokenSource _cancellationTokenSource =
            new CancellationTokenSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MobileManager.Startup"/> class.
        /// </summary>
        /// <param name="env">Env.</param>
        public Startup(IHostingEnvironment env)
        {
            AppConfigurationProvider.Register<DbConfiguration>(@"ConfigFiles/dbconfig.json")
                .RegisterNext<ManagerConfiguration>(@"ConfigFiles/managerconfig.json")
                .RegisterNext<AppConfiguration>(@"ConfigFiles/appsettings.json");
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>The configuration.</value>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">Services.</param>
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            using (var deviceDbContext = new GeneralDbContext())
            {
                deviceDbContext.Database.EnsureCreated();
            }

            services.AddMvc()
                .AddJsonOptions(
                    options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                ).AddJsonOptions(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info {Title = "MobileManager API", Version = "v1"});

                //Set the comments path for the swagger json and ui.
                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "MobileManager.xml");
                c.IncludeXmlComments(xmlPath);
            });

            services.AddReact();
            services.AddMvc();

            services.AddEntityFrameworkMultiDb()
                .AddDbContext<GeneralDbContext>();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllHeaders",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            services.AddTransient<IRepository<Device>, DeviceRepository>()
                .AddTransient<IRepository<Reservation>, ReservationQueueRepository>()
                .AddTransient<IRepository<ReservationApplied>, ReservationAppliedRepository>()
                .AddTransient<IRepository<AppiumProcess>, AppiumRepository>()
                .AddTransient<IRepository<LogMessage>, LoggerRepository>();

            //services.AddSingleton<IManagerConfiguration, ManagerConfiguration>();
            services.AddSingleton<IRestClient, RestClient>()
                .AddSingleton<IAppiumService, AppiumService>()
                .AddSingleton<IAdbController, AdbController>()
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddSingleton(typeof(IManagerConfiguration), AppConfigurationProvider.Get<ManagerConfiguration>())
                .AddSingleton<IManagerLogger, ManagerLogger>()
                .AddSingleton<IExternalProcesses, ExternalProcesses>();

            services.AddMvcCore().AddApiExplorer();

            // Run hosted services
            var configuration = AppConfigurationProvider.Get<ManagerConfiguration>();

            if (configuration.AndroidServiceEnabled)
                services.AddHostedService<AndroidDeviceService>();

            if (configuration.IosServiceEnabled)
                services.AddHostedService<IosDeviceService>();

            services.AddHostedService<ReservationService>();
        }

        /// <summary>
        /// Configure the specified app, env and loggerFactory. This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <returns>The configure.</returns>
        /// <param name="app">App.</param>
        /// <param name="env">Env.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        /// <param name="applicationLifetime">Handle application lifecycle.</param>
        /// <param name="logger">Logger.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IApplicationLifetime applicationLifetime, IManagerLogger logger)
        {
            var appconfig = AppConfigurationProvider.Get<AppConfiguration>();

            applicationLifetime.ApplicationStopped.Register(OnShutdown);

            loggerFactory.AddConsole((logText, logLevel) =>
            {
                if (Debugger.IsAttached)
                {
                    return true;
                }

                if (logLevel >= appconfig.DefaultLogLevel)
                {
                    return true;
                }

                return false;
            }, appconfig.IncludeScopes);

            loggerFactory.AddFile("Logs/log-{Date}.txt", LogLevel.Trace);

            app.UseExceptionHandler(
                options =>
                {
                    options.Run(
                        async context =>
                        {
                            context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                            context.Response.ContentType = "text/html";
                            var ex = context.Features.Get<IExceptionHandlerFeature>();
                            if (ex != null)
                            {
                                var err = $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.StackTrace}";
                                await context.Response.WriteAsync(err).ConfigureAwait(false);
                                logger.Error(ex.Error.Message, ex.Error);
                            }
                        });
                }
            );

            //app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseMvcWithDefaultRoute();

            app.UseCors("AllowAllHeaders");

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "doc";
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }

        private void OnShutdown()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}

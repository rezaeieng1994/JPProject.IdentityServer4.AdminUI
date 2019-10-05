﻿using Jp.Infra.CrossCutting.Database;
using Jp.Infra.CrossCutting.IoC;
using Jp.Management.Configuration;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jp.Management
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(ILogger<Startup> logger, IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services, ILoggerFactory logger)
        {
            services.AddMvcCore().AddApiExplorer();

            // Identity Database
            services.AddAuthentication(Configuration);

            services.ConfigureCors();

            // Configure policies
            services.AddPolicies();

            // configure auth Server
            services.AddIdentityServerAuthentication(logger.CreateLogger<Startup>(), Configuration);

            // configure openapi
            services.AddSwagger(Configuration);

            // Config automapper
            services.AddAutoMapperSetup();
            // Adding MediatR for Domain Events and Notifications
            services.AddMediatR(typeof(Startup));

            // .NET Native DI Abstraction
            RegisterServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDefaultCors();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("./v1/swagger.json", "ID4 User Management");
                c.OAuthClientId("Swagger");
                c.OAuthAppName("User Management UI - full access");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }


        private void RegisterServices(IServiceCollection services)
        {
            // Adding dependencies from another layers (isolated from Presentation)
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            NativeInjectorBootStrapper.RegisterServices(services, Configuration);
        }
    }
}

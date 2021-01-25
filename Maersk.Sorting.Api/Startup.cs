using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;


namespace Maersk.Sorting.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers()
                .AddNewtonsoftJson(options => options.SerializerSettings.Converters.Add(new StringEnumConverter()));
            services.AddMemoryCache();
            services.AddSingleton<ISortJobProcessor, SortJobProcessor>();
            services.AddHostedService<SortJobHostedService>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Sorting   API", Version = "version 1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            app.UseMiddleware<Maersk.Sorting.Api.ExceptionHandler.ExceptionMiddleware>();

            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI(a => { a.SwaggerEndpoint("/swagger/v1/swagger.json", "TariffComparison API v1"); });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

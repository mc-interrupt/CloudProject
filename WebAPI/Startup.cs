using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Routing;

using Microsoft.AspNetCore.Cors.Infrastructure;

using Amazon.EC2;
using Amazon.S3;
using Amazon.ElasticMapReduce;
using Amazon.DynamoDBv2;

using WebAPI.Model.Services;

namespace WebAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set;}

        // public Startup(IConfiguration configuration)
        // {
        //     Configuration = configuration;
        // }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                              .SetBasePath(env.ContentRootPath)
                              .AddJsonFile("Configurations/TaskConfigurations.json", optional: false, reloadOnChange: true)
                              .AddJsonFile("Configurations/DynamoDBStructure.json", optional: false, reloadOnChange: true)
                              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true)
                              .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Action<CorsPolicyBuilder> devPolicy = builder => { builder.WithOrigins("http://localhost:4200")
            //                                                           .WithHeaders("content-type", "origin")
            //                                                           .AllowAnyMethod(); };

            Action<CorsPolicyBuilder> devPolicy = builder => { builder.AllowAnyOrigin()
                                                                      .WithHeaders("content-type", "origin")
                                                                      .AllowAnyMethod(); };

            services.AddCors(options => options.AddPolicy("Development", devPolicy));

            services.AddMvc();
            //services.AddRouting();
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonEC2>();
            services.AddAWSService<IAmazonS3>();
            services.AddAWSService<IAmazonElasticMapReduce>();
            services.AddAWSService<IAmazonDynamoDB>();

            // todo: this approach is not recommended
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddTransient<ITaskConfigurationService, TaskConfigurationService>();

            //services.Configure<JobConfiguration>(options => Configuration.GetSection("TaskConfigurations:Group1:Task1").Bind(options));
            // services.Configure<Func<string, JobConfiguration>>(options =>
            // {
            //     ITaskConfigurationService service = new TaskConfigurationService(Configuration);
            //     return service.GetTaskConfiguration()
            // });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("Development");

            //app.UseMvc(routes => routes.MapRoute("default", "{controller=Home}/{action=Index}/{value?}"));
            app.UseMvcWithDefaultRoute();
        }
    }
}

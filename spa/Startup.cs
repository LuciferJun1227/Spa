using System;
using System.IO;
using System.Linq;
using JavaScriptViewEngine;
using LogDashboard;
using LogDashboard.Authorization.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using spa.Controller;
using spa.Filter;
using spa.JavaScriptViewEngine.Utils;

namespace spa
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            ConfigHelper._configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //采用开源的logger查看组件查看本地日志文件
            services.AddLogDashboard(opt =>
            {
                var localUserName = ConfigHelper.GetConfig("BasicAuth:Name") ;
                var localPassword = ConfigHelper.GetConfig("BasicAuth:Pwd");
                if (!string.IsNullOrEmpty(localUserName) && !string.IsNullOrEmpty(localPassword))
                {
                    opt.AddAuthorizationFilter(new LogDashboardBasicAuthFilter(localUserName, localPassword));
                }
            });

            services.AddJsEngine(builder =>
            {
                builder.UseRenderEngine<RazorEngineBuilder>();
                builder.UseSingletonEngineFactory();
            });

           
            services.AddCors(o => o.AddPolicy("Any", r =>
            {
                r.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));

            services.AddRazorPages().AddNewtonsoftJson(options =>
                options.SerializerSettings.ContractResolver =
                    new DefaultContractResolver());

            services.AddHttpContextAccessor();

            services.AddScoped<BasicAuthFilter>();
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory logging)
        {

            ConfigHelper.ContentRootPath = env.ContentRootPath;
            ConfigHelper.WebRootPath = env.WebRootPath;
            if (string.IsNullOrEmpty(ConfigHelper.WebRootPath))
            {
                ConfigHelper.WebRootPath = Path.Combine(ConfigHelper.ContentRootPath, "wwwroot");
            }

            ConfigHelper.BackupPath = Path.Combine(env.ContentRootPath, "backup");
#if DEBUG
            app.UseDeveloperExceptionPage();
#endif

            //使用开源的本地日志组件
            app.UseLogDashboard();

            app.UseSpa();

            app.UseRouting();

            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "Spa",
                    pattern: "{*url}", defaults: new { controller = "Home", action = "Index" });

            });

        }

    }
}
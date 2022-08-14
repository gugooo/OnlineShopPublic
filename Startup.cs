using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AlcantaraNew.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlcantaraNew
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IHostingEnvironment ev)
        {
            //Configuration = configuration;
            var conf = new ConfigurationBuilder().SetBasePath(ev.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = conf.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            #region Localization
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            #endregion
            #region AddMVC
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddMvcLocalization()
                .AddDataAnnotationsLocalization();
            #endregion
            #region DBContext
            services.AddDbContext<Models.AlcantaraDBContext>(optins => optins.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            #endregion
            #region Registration Options
            services.AddIdentity<Models.User, IdentityRole>(options =>
            {
                //Registration Option
                options.Password.RequireNonAlphanumeric = false;//Password Options
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                //Login Options
                options.Lockout.DefaultLockoutTimeSpan = new TimeSpan(0, 15, 0);//15Minutes
            })
                .AddEntityFrameworkStores<Models.AlcantaraDBContext>()
                .AddDefaultTokenProviders();
            #endregion
            #region Session
            services.AddDistributedMemoryCache();
            services.AddSession();
            #endregion
            #region Facebook & Google
            services.AddAuthentication().AddGoogle(options => {
                options.ClientId = "ClientId";
                options.ClientSecret = "ClientSecret";
                options.CallbackPath = "/Account/GoogleResponse/";
            }).AddFacebook(options => {
                options.ClientId = "ClientId";
                options.ClientSecret = "ClientSecret";
                options.CallbackPath = "/Account/FacebookResponse/";
            });
            #endregion
            #region Cilture Info
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en"),
                    new CultureInfo("ru"),
                    new CultureInfo("hy")
                };

                options.DefaultRequestCulture = new RequestCulture("ru");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });
            #endregion
            #region SignalR
            services.AddSignalR(option => {
                option.ClientTimeoutInterval = TimeSpan.FromHours(1);
                option.HandshakeTimeout = TimeSpan.FromHours(1);
            });
            #endregion
            #region Caching
            services.AddTransient<CacheManager>();
            services.AddMemoryCache();
            #endregion
            services.AddTransient<IMainMenu, TwoLevelMenu>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRequestLocalization();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseSession();
            app.UseSignalR(routes =>
            {
                routes.MapHub<Classes.ChatHub>("/chatHub");
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

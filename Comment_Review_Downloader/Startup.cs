using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Comment_Review_Downloader.Data;
using Comment_Review_Downloader.Data.Interface;
using Comment_Review_Downloader.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Comment_Review_Downloader.Service.HostedServices;
using Microsoft.Extensions.Hosting;
using Comment_Review_Downloader.Service;
using Comment_Review_Downloader.Models;
using System.Net.Http;

namespace Comment_Review_Downloader
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddDbContext<CommentsDbContext>(options => options.UseSqlite("Data Source = CommentsTable.db"));

            services.AddSingleton<BackgroundReviewWorker>();
            services.AddSingleton<BackgroundEMailSender>();
            services.AddSingleton<IHostedService>(serviceProvider => serviceProvider.GetService<BackgroundReviewWorker>());
            services.AddSingleton<IHostedService>(serviceProvider => serviceProvider.GetService<BackgroundEMailSender>());

            services.AddSingleton<YoutubeCommentsFetcher>();
            services.AddSingleton<AmazonReviewFetcher>();

            services.AddTransient<Func<string, ICommentFetcher>>(serviceProvider => key => {
                switch (key)
                {
                    case AppConstants.Youtube:
                        return serviceProvider.GetService<YoutubeCommentsFetcher>();
                    case AppConstants.Amazon:
                        return serviceProvider.GetService<AmazonReviewFetcher>();
                    default:
                        throw new KeyNotFoundException(); // or maybe return null, up to you
                }
            });

            HttpClient httpClient = new HttpClient();
            services.AddSingleton(httpClient);

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
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
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            #region perform automatic migrations
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetService<CommentsDbContext>().Database.Migrate();
            }
            #endregion
        }
    }
}

using Comment_Review_Downloader.Data;
using Comment_Review_Downloader.Data.Interface;
using Comment_Review_Downloader.Models;
using Comment_Review_Downloader.Service;
using Comment_Review_Downloader.Service.HostedServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Comment_Review_Downloader
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _logger = logger;
            Configuration = configuration;
        }


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
                _logger.LogInformation($"Comment Fetcher Tranient: {key}");
                switch (key)
                {
                    case AppConstants.Youtube:
                        _logger.LogInformation($"Selected: {key}");
                        return serviceProvider.GetService<YoutubeCommentsFetcher>();
                    case AppConstants.Amazon:
                        _logger.LogInformation($"Selected: {key}");
                        return serviceProvider.GetService<AmazonReviewFetcher>();
                    default:
                        _logger.LogInformation($"Transient Error");
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

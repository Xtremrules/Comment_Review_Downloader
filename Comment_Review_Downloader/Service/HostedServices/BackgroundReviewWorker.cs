using Comment_Review_Downloader.Data;
using Comment_Review_Downloader.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Comment_Review_Downloader.Service.HostedServices
{
    public class BackgroundReviewWorker : BackgroundService
    {
        private readonly ILogger<BackgroundReviewWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Func<string, ICommentFetcher> _fetcherFactory;

        public BackgroundReviewWorker(
            ILogger<BackgroundReviewWorker> logger,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            Func<string, ICommentFetcher> fetcherFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _fetcherFactory = fetcherFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Review Fetcher started");
            stoppingToken.Register(() => _logger.LogDebug($"Shutting down Background Review Fetcher."));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //Let's wait for a message to appear in the queue
                    //If the token gets canceled, then we'll stop waiting
                    //since an OperationCanceledException will be thrown
                    _logger.LogInformation("waiting for new requests");
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<CommentsDbContext>();
                        await FetchAndSendRequests(dbContext);
                    }
                }
                catch (OperationCanceledException ex)
                {
                    //We need to terminate the delivery, so we'll just break the while loop
                    _logger.LogInformation(ex, "background review fetcher is down.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"failed to fetch reviews, {ex.Message}");
                }

                //await Task.Delay(10000, stoppingToken);
                // For 2 Mins
                await Task.Delay(120000, stoppingToken);
            }
            _logger.LogInformation("Background Review fetcher stopped");
        }

        private async Task FetchAndSendRequests(CommentsDbContext dbContext)
        {
            var requests = await dbContext.Comments.Where(cr => !cr.Fetched && !cr.Disabled).ToListAsync();
            if (requests.Any())
            {
                foreach (var request in requests)
                {
                    _logger.LogInformation($"Db Fetch Request ID: {request.ToString()}");
                    var host = new Uri(request.Url)?.Host;
                    _logger.LogInformation($"Host: {host}");
                    CommentDetails result = new CommentDetails();
                    switch (host)
                    {
                        case AppConstants.YoutubeHost:
                            _logger.LogInformation("Hello Youtube");
                            result = await _fetcherFactory(AppConstants.Youtube).FetchComments(request);
                            break;
                        case AppConstants.AmazonHost:
                            _logger.LogInformation("Hello Amazon");
                            result = await _fetcherFactory(AppConstants.Amazon).FetchComments(request);
                            break;
                        default:
                            _logger.LogInformation("We haven't implemented that yet");
                            break;
                    }

                    request.Fetched = result?.Filename == null ? false : true;
                    request.Location = result?.Filename;
                    request.NOC = result?.NOC;
                    request.Name = result?.Name;
                    request.UpdatedDate = DateTime.UtcNow;
                    request.Disabled = result == null ? true : false;
                    dbContext.Attach(request);
                    dbContext.Entry(request).State = EntityState.Modified;
                    _logger.LogInformation($"Comment Download Successfull: {request.ToString()}");
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
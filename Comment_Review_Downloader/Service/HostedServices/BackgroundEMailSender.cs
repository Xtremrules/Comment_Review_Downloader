using Comment_Review_Downloader.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite.Storage;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Comment_Review_Downloader.Models;
using System.IO;
using System.Net.Mime;

namespace Comment_Review_Downloader.Service.HostedServices
{
    public class BackgroundEMailSender : BackgroundService
    {
        private readonly ILogger<BackgroundEMailSender> _logger;
        private readonly SmtpClient _smtpClient;
        private readonly string _AdminContact;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public BackgroundEMailSender(
            ILogger<BackgroundEMailSender> logger,
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _smtpClient = CreateSmtpClient(configuration);
            _AdminContact = configuration["AdminContact"];
            _serviceScopeFactory = serviceScopeFactory;
        }

        private SmtpClient CreateSmtpClient(IConfiguration configuration)
        {
            return new SmtpClient
            {
                Host = configuration["Smtp:Host"],
                Port = configuration.GetValue<int>("Smtp:Port"),
                EnableSsl = configuration.GetValue<bool>("Smtp:Ssl"),
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(
                        userName: configuration["Smtp:Username"],
                        password: configuration["Smtp:Password"]
                    )
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Email Sender started");
            stoppingToken.Register(() => _logger.LogDebug($"Shutting down Background Email Sender."));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //Let's wait for a message to appear in the queue
                    //If the stoppingToken gets canceled, then we'll stop waiting
                    //since an OperationCanceledException will be thrown
                    _logger.LogInformation("waiting for new requests");
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<CommentsDbContext>();
                        await FetchAndSendEmail(dbContext);
                    }
                }
                catch (OperationCanceledException)
                {
                    //We need to terminate the delivery, so we'll just break the while loop
                    _logger.LogInformation("background email sender is down.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"failed to fetch email, {ex.Message}");
                }
                await Task.Delay(10000, stoppingToken);
            }
            _logger.LogInformation("Background Email Sender stopped");
        }

        private async Task FetchAndSendEmail(CommentsDbContext dbContext)
        {
            var requestLogs = await dbContext.Query<EmailWorkerModel>()
                .FromSql("Select c.Location, c.Name, cq.emailAddress, cq.Id from Comments" +
                " as c inner join CommentRequests as cq on c.id = cq.CommentId  " +
                "where c.Fetched = 1 and cq.emailed = 0").ToListAsync();//.CommentRequests.Where(
            if (requestLogs.Any())
            {
                foreach (var requestLog in requestLogs)
                {
                    _logger.LogInformation($"sending mail for {requestLog.Name}, {requestLog.Location}");
                    var commentRequest = await dbContext.CommentRequests.FindAsync(requestLog.Id);
                    commentRequest.emailed = await SendMail(requestLog);
                    commentRequest.dateSent = DateTime.Now;
                    dbContext.Attach(commentRequest);
                    dbContext.Entry(commentRequest).State = EntityState.Modified;
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        private async Task<bool> SendMail(EmailWorkerModel requestLog)
        {
            string fileName = "";
            Attachment attachment = null;
            if (requestLog.Location != null)
            {
                fileName = Path.Combine(AppConstants.FileDirectory, requestLog.Location);
                if (File.Exists(fileName))
                {
                    attachment = new Attachment(fileName, MediaTypeNames.Application.Octet);
                    attachment.Name = requestLog.Name + ".csv";
                }
            }

            MailMessage message = new MailMessage(
                _AdminContact,
                requestLog.emailAddress,
                "Your Comment Request is Ready",
                "Find attached your comments as requested");
            // Add the file attachment to this e-mail message.
            if (attachment != null)
            {
                message.Attachments.Add(attachment);
            }
            else
            {
                message.Body = "Sorry, the website you submitted is not yet supported.";
            }
            await _smtpClient.SendMailAsync(message);
            return true;
        }
    }
}
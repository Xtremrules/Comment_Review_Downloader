using Comment_Review_Downloader.Data;
using Comment_Review_Downloader.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

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
                    _logger.LogError($"failed to fetch email, {ex.Message}");
                }

                //await Task.Delay(10000, stoppingToken);
                // For 2 Mins
                await Task.Delay(120000, stoppingToken);
            }
            _logger.LogInformation("Background Email Sender stopped");
        }

        private async Task FetchAndSendEmail(CommentsDbContext dbContext)
        {
            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "Select c.Location, c.Name,c.Disabled, cq.emailAddress, cq.Id from Comments" +
                " as c inner join CommentRequests as cq on c.id = cq.CommentId  " +
                "where (c.Fetched = 1 or c.Disabled = 1) and cq.emailed = 0";
                await command.Connection.OpenAsync();
                DbDataReader reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        // do something with each in the list or just return the list
                        var id = reader["Id"].ToString();
                        _logger.LogInformation($"sending mail for {reader["Name"].ToString() }, {reader["Location"].ToString()}");
                        var commentRequest = await dbContext.CommentRequests.FindAsync(Convert.ToInt32(id));
                        var request = new EmailWorkerModel
                        {
                            emailAddress = reader["emailAddress"].ToString(),
                            Id = Convert.ToInt32(reader["Id"].ToString()),
                            Location = reader["Location"].ToString(),
                            Name = reader["Name"].ToString(),
                        };
                        commentRequest.emailed = await SendMail(request);
                        commentRequest.dateSent = DateTime.Now;
                        dbContext.Attach(commentRequest);
                        dbContext.Entry(commentRequest).State = EntityState.Modified;
                        await dbContext.SaveChangesAsync();
                    }

                    command.Connection.Close();
                }
                return;
            }
        }

        private async Task<bool> SendMail(EmailWorkerModel requestLog)
        {
            string fileName = "";
            Attachment attachment = null;
            if (!string.IsNullOrEmpty(requestLog.Location))
            {
                fileName = Path.Combine(AppConstants.FileDirectory, requestLog.Location);
                if (File.Exists(fileName))
                {
                    attachment = new Attachment(fileName, MediaTypeNames.Application.Octet)
                    {
                        Name = requestLog.Name + ".csv"
                    };
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
                message.Body = "Sorry, the website you submitted is not yet supported or Comment/Review is disabled.";
            }
            try
            {
                await _smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send Email, {ex.Message}");
            }
            return true;
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Common;
using Lykke.RabbitMqBroker.Publisher;
using MAVN.Service.CustomerManagement.Domain.Services;
using MAVN.Service.NotificationSystem.SubscriberContract;
using MAVN.Service.Sessions.Client;

namespace MAVN.Service.CustomerManagement.DomainServices
{
    public class PostProcessService : IPostProcessService
    {
        private readonly ISessionsServiceClient _sessionsServiceClient;
        private readonly IRabbitPublisher<EmailMessageEvent> _emailEventPublisher;

        public PostProcessService(ISessionsServiceClient sessionsServiceClient, IRabbitPublisher<EmailMessageEvent> emailEventPublisher)
        {
            _sessionsServiceClient = sessionsServiceClient;
            _emailEventPublisher = emailEventPublisher;
        }

        public async Task ClearSessionsAndSentEmailAsync(string customerId, string templateId, string subjectTemplateId)
        {
            var activeSessions = await _sessionsServiceClient.SessionsApi.GetActiveSessionsAsync(customerId);

            var tasks = new List<Task>();

            foreach (var session in activeSessions)
            {
                tasks.Add(_sessionsServiceClient.SessionsApi.DeleteSessionIfExistsAsync(session.SessionToken));
            }

            var emailEventTask = _emailEventPublisher.PublishAsync(new EmailMessageEvent
            {
                CustomerId = customerId,
                MessageTemplateId = templateId,
                SubjectTemplateId = subjectTemplateId,
                Source = $"{AppEnvironment.Name} - {AppEnvironment.Version}",
            });

            tasks.Add(emailEventTask);


            await Task.WhenAll(tasks);
        }
    }
}

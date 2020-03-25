using System.Threading.Tasks;

namespace Lykke.Service.CustomerManagement.Domain.Services
{
    public interface IPostProcessService
    {
        Task ClearSessionsAndSentEmailAsync(string customerId, string templateId, string subjectTemplateId);
    }
}

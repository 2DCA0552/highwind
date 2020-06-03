using System.Collections.Generic;
using System.Threading.Tasks;
using Highwind.Models;

namespace Highwind.Helpers.Interfaces
{
    public interface IDataRepository
    {
        // Words
        Task<IEnumerable<Client>> GetClients();
        Task<Client> GetClient(int id);
        Task<Client> GetClientByApiKey(string apiKey);
        Task<Client> GetClientByApplication(string application);
        Task<bool> AddClient(Client client);
        Task<bool> UpdateClient(int id, Client client);
        Task<bool> DeleteClient(int id);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Highwind.Helpers.Interfaces;
using Highwind.Models;
using Highwind.Settings;
using LiteDB;
using Microsoft.Extensions.Options;

namespace Highwind.Helpers
{
    public class LiteDbHelper : IDataRepository
    {
        private LiteDbSettings _settings;
        private LiteDatabase db;
        private LiteCollection<Client> clients;

        public LiteDbHelper(IOptions<LiteDbSettings> settings){
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            db = new LiteDatabase(_settings.Database);
            clients = db.GetCollection<Client>(_settings.Collection);
        }

        public async Task<bool> AddClient(Client client)
        {
            await Task.Run(() => {
                clients.Insert(client);
            });

            return true;
        }

        public async Task<bool> DeleteClient(int id)
        {
            await Task.Run(() => {
                clients.Delete(id);
            });

            return true;
        }

        public async Task<Client> GetClient(int id)
        {
            var c = await Task.Run(() => {
                return clients.FindById(id);
            });

            return c;
        }

        public async Task<Client> GetClientByApiKey(string apiKey)
        {
            var c = await Task.Run(() => {
                return clients.FindOne(client => client.ApiKey == apiKey);
            });

            return c;
        }

        public async Task<Client> GetClientByApplication(string application)
        {
            var c = await Task.Run(() => {
                return clients.FindOne(client => client.Application == application);
            });

            return c;
        }

        public async Task<IEnumerable<Client>> GetClients()
        {
            var c = await Task.Run(() => {
                return clients.FindAll();
            });

            return c;
        }

        public async Task<bool> UpdateClient(int id, Client client)
        {
            await Task.Run(() => {
                clients.Update(id, client);
            });

            return true;
        }
    }
}
using crudmongo.Configurations;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using crudmongo.Models;
using System;

namespace crudmongo.Services
{
    public class ElevatorService
    {
        private readonly IMongoCollection<Elevator> _elevatorCollection;

        // Événement pour notifier les changements de données
        public event Action<List<Elevator>> OnDataUpdated;

        public ElevatorService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
            var mongoDb = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
            _elevatorCollection = mongoDb.GetCollection<Elevator>(databaseSettings.Value.CollectionName);
        }

        public async Task<List<Elevator>> GetAllAsync()
        {
            return await _elevatorCollection.Find(_ => true).ToListAsync();
        }

        public async Task<List<Elevator>> GetAsync() =>
            await _elevatorCollection.Find(filter: _ => true).ToListAsync();

        public async Task<Elevator> GetAsync(string id) =>
            await _elevatorCollection.Find(filter: x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Elevator elevator)
        {
            await _elevatorCollection.InsertOneAsync(elevator);
            // Notifier les abonnés des changements
            NotifyDataChange();
        }

        public async Task UpdateAsync(Elevator elevator)
        {

            await _elevatorCollection.ReplaceOneAsync(e => e.Id == elevator.Id, elevator);
            // Notifier les abonnés des changements
            NotifyDataChange();
        }

        public async Task RemoveAsync(string id)
        {
            await _elevatorCollection.DeleteOneAsync(filter: x => x.Id == id);
            // Notifier les abonnés des changements
            NotifyDataChange();
        }

        private async void NotifyDataChange()
        {
            var updatedData = await GetAllAsync();
            OnDataUpdated?.Invoke(updatedData);
        }
    }
}

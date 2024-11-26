using crudmongo.Configurations;
using crudmongo.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace crudmongo.Services
{
    public class ElevatorService
    {
        private readonly IMongoCollection<Elevator> _elevatorCollection;

        public event Action<List<Elevator>>? OnDataUpdated;

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

        public async Task<Elevator?> GetAsync(string id)
        {
            return await _elevatorCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Elevator elevator)
        {
            await _elevatorCollection.InsertOneAsync(elevator);
            await NotifyDataChangeAsync();
        }

        public async Task<bool> UpdateAsync(Elevator elevator)
        {
            if (string.IsNullOrEmpty(elevator.Id))
            {
                Console.WriteLine("Elevator ID is null or empty.");
                return false;
            }

            try
            {
                var result = await _elevatorCollection.ReplaceOneAsync(
                    e => e.Id == elevator.Id,
                    elevator
                );

                if (result.IsAcknowledged && result.ModifiedCount > 0)
                {
                    await NotifyDataChangeAsync();
                    return true;
                }
                else
                {
                    Console.WriteLine($"Elevator with ID {elevator.Id} not found.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating elevator: {ex.Message}");
                return false;
            }
        }


        public async Task RemoveAsync(string id)
        {
            await _elevatorCollection.DeleteOneAsync(x => x.Id == id);
            await NotifyDataChangeAsync();
        }

        private async Task NotifyDataChangeAsync()
        {
            var updatedData = await GetAllAsync();
            OnDataUpdated?.Invoke(updatedData);
        }
    }
}

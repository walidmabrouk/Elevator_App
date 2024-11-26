using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using FirstWebApp.Domaine.Entities;

public class ProprietaireService
{
    private readonly IMongoCollection<Proprietaire> _proprietaireCollection;

    public ProprietaireService(IConfiguration config)
    {
        var client = new MongoClient(config.GetConnectionString("MongoDbConnection"));
        var database = client.GetDatabase("YourDatabaseName");
        _proprietaireCollection = database.GetCollection<Proprietaire>("Proprietaires");
    }

    public async Task CreateAsync(Proprietaire proprietaire)
    {
        await _proprietaireCollection.InsertOneAsync(proprietaire);
    }
}

using crudmongo.Services;
using FirstWebApp.Domaine.services;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketService
{
    private readonly ElevatorService _statusService;
    private readonly IMqttClientService _mqttClientService;  // Utilisation de l'interface IMqttClientService
    private WebSocketHandler _webSocketHandler;

    // Constructeur avec injection des services
    public WebSocketService(WebSocketHandler webSocketHandler, ElevatorService statusService, IMqttClientService mqttClientService)
    {
        _webSocketHandler = webSocketHandler;
        _statusService = statusService;
        _mqttClientService = mqttClientService;  // Utiliser l'interface, pas la classe concrète

        _statusService.OnDataUpdated += async (updatedData) =>
        {
            if (_webSocketHandler.State == WebSocketState.Open)
            {
                var jsonData = JsonConvert.SerializeObject(updatedData);
                await _webSocketHandler.SendMessageAsync(jsonData, CancellationToken.None);
            }
        };
    }

    // Gestion de la connexion WebSocket
    public async Task HandleWebSocketConnectionAsync(WebSocket webSocket, CancellationToken cancellationToken)
    {
        _webSocketHandler = new WebSocketHandler(webSocket);

        // Envoyer les données initiales via WebSocket
        var initialData = await _statusService.GetAllAsync();
        var jsonData = JsonConvert.SerializeObject(initialData);
        await _webSocketHandler.SendMessageAsync(jsonData, cancellationToken);

        // Recevoir les messages envoyés par le client et les publier sur MQTT
        await ReceiveMessagesAsync(cancellationToken);
    }

    // Méthode pour recevoir les messages du WebSocket
    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        while (_webSocketHandler.State == WebSocketState.Open)
        {
            var message = await _webSocketHandler.ReceiveMessageAsync(cancellationToken);
            Console.WriteLine($"Message reçu: {message}");

            // Publier le message reçu sur le broker MQTT
            try
            {
                const string topic = "test/received"; // Topic MQTT où le message sera publié
                await _mqttClientService.PublishMessageAsync(topic, message);
                Console.WriteLine($"Message publié au topic '{topic}': {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la publication du message MQTT : {ex.Message}");
            }
        }
    }
}

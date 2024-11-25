using crudmongo.Services;
using System.Net.WebSockets;
using Newtonsoft.Json;

public class WebSocketService
{
    private readonly ElevatorService _statusService;
    private WebSocketHandler _webSocketHandler;

    public WebSocketService(WebSocketHandler webSocketHandler, ElevatorService statusService)
    {
        _webSocketHandler = webSocketHandler;
        _statusService = statusService;

        // S'abonner aux changements des données
        _statusService.OnDataUpdated += async (updatedData) =>
        {
            if (_webSocketHandler.State == WebSocketState.Open)
            {
                var jsonData = JsonConvert.SerializeObject(updatedData);
                await _webSocketHandler.SendMessageAsync(jsonData, CancellationToken.None);
            }
        };
    }

    public async Task HandleWebSocketConnectionAsync(WebSocket webSocket, CancellationToken cancellationToken)
    {
        _webSocketHandler = new WebSocketHandler(webSocket);

        // Envoyer les données initiales
        var initialData = await _statusService.GetAllAsync();
        var jsonData = JsonConvert.SerializeObject(initialData);
        await _webSocketHandler.SendMessageAsync(jsonData, cancellationToken);

        // Recevoir les messages envoyés par le client
        await ReceiveMessagesAsync(cancellationToken);
    }

    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        while (_webSocketHandler.State == WebSocketState.Open)
        {
            var message = await _webSocketHandler.ReceiveMessageAsync(cancellationToken);
            Console.WriteLine($"Message reçu: {message}");
        }
    }
}

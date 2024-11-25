using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketHandler
{
    private readonly WebSocket _webSocket;

    public WebSocketHandler(WebSocket webSocket)
    {
        _webSocket = webSocket;
    }

    // Méthode pour envoyer des données au client WebSocket
    public async Task SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            await _webSocket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
        }
    }

    // Méthode pour recevoir des données du client WebSocket
    public async Task<string> ReceiveMessageAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[1024];
        var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
        return Encoding.UTF8.GetString(buffer, 0, result.Count);
    }

    // Méthode pour fermer la connexion WebSocket
    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Fermeture", cancellationToken);
    }

    // Vérifier l'état de la connexion WebSocket
    public WebSocketState State => _webSocket.State;
}

namespace FirstWebApp.Domaine.services
{
    public interface IMqttClientService 
    {
        Task ConnectAsync();

        Task PublishMessageAsync(string topic, string message);
    }
}

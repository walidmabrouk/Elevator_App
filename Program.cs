using crudmongo.Configurations;
using crudmongo.Services;
using FirstWebApp.Domaine.services;
using FirstWebApp.Infra.ServicesImp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(key: "MongoDatabase"));
builder.Services.AddSingleton<ElevatorService>();
builder.Services.AddSingleton<IMqttClientService, MqttClientService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Configurer WebSockets
app.UseWebSockets();

var mqttClientService = app.Services.GetRequiredService<IMqttClientService>();
await mqttClientService.ConnectAsync();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapGet("/", () => "Bienvenue sur le serveur WebSocket!");

app.Map("/ws", async (HttpContext context, CancellationToken cancellationToken, ElevatorService statusService) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var socket = await context.WebSockets.AcceptWebSocketAsync();


        // Créer le service WebSocket et gérer la connexion
        var webSocketHandler = new WebSocketHandler(socket);

        var webSocketService = new WebSocketService(webSocketHandler, statusService);

        // Traiter la connexion WebSocket
        await webSocketService.HandleWebSocketConnectionAsync(socket, cancellationToken);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

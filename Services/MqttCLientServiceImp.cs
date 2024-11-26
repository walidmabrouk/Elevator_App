using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using crudmongo.Services;
using crudmongo.Models;
using FirstWebApp.Domaine.Entities;
using FirstWebApp.Domaine.services;
using MongoDB.Bson;

namespace FirstWebApp.Infra.ServicesImp
{
    public class MqttClientService : IMqttClientService
    {
        private readonly IMqttClient _mqttClient;
        private readonly IMqttClientOptions _mqttOptions;
        private readonly ElevatorService _elevatorService;

        public MqttClientService(ElevatorService elevatorService)
        {
            _elevatorService = elevatorService;

            // Configure MQTT client options
            _mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId("AscenseurApiClient")
                .WithTcpServer("192.168.220.1", 1883)
                .WithCleanSession()
                .Build();

            _mqttClient = new MqttFactory().CreateMqttClient();

            ConfigureMqttClientHandlers();
        }

        private void ConfigureMqttClientHandlers()
        {
            // Handle successful connection
            _mqttClient.UseConnectedHandler(async e =>
            {
                Console.WriteLine("Connected to MQTT broker.");

                var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter("test/topic")
                    .Build();

                await _mqttClient.SubscribeAsync(subscribeOptions, CancellationToken.None);
                Console.WriteLine("Subscribed to topic: test/topic");
            });

            // Handle disconnection
            _mqttClient.UseDisconnectedHandler(e =>
            {
                Console.WriteLine("Disconnected from MQTT broker.");
            });

            // Handle incoming messages
            _mqttClient.UseApplicationMessageReceivedHandler(async e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                Console.WriteLine($"Message received on topic '{topic}': {payload}");

                try
                {
                    var proprietaire = JsonSerializer.Deserialize<Proprietaire>(payload);
                    if (proprietaire == null)
                    {
                        Console.WriteLine("Failed to deserialize the payload into a 'Proprietaire' object.");
                        return;
                    }

                    await ProcessProprietaireData(proprietaire, topic);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                }
            });
        }

        private async Task ProcessProprietaireData(Proprietaire proprietaire, string topic)
        {
            Console.WriteLine($"Processing owner data: {proprietaire.Id}");

            foreach (var immeuble in proprietaire.Immeubles)
            {
                Console.WriteLine($"  Building ID: {immeuble.Id}");
                foreach (var ascenseur in immeuble.Ascenseurs)
                {
                    Console.WriteLine($"    Elevator data: {ascenseur.Id}, State: {ascenseur.State}");

                    if (topic == "test/topic")
                    {
                        // Vérifier si l'ID est un ObjectId valide
                        string elevatorId = ascenseur.Id ?? "000000000000000000000000"; // Fallback ID par défaut
                        if (!ObjectId.TryParse(elevatorId, out var objectId))
                        {
                            Console.WriteLine($"Invalid ObjectId format for elevator ID: {elevatorId}");
                            continue;
                        }


                        var elevator = new Elevator
                        {
                            Id = ascenseur.Id,
                            State = ascenseur.State ?? "Unknown",
                            Floor = ascenseur.Floor ?? "0",
                            Direction = ascenseur.Direction ?? "Stationary"
                        };

                        try
                        {
                            var updateSuccess = await _elevatorService.UpdateAsync(elevator);

                            if (updateSuccess)
                            {
                                Console.WriteLine($"Elevator {ascenseur.Id} updated successfully.");
                            }
                            else
                            {
                                Console.WriteLine($"Failed to update elevator {ascenseur.Id}.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error updating elevator: {ex.Message}");
                        }
                    }
                }
            }
        }

        // Méthode pour vérifier si une chaîne est un ObjectId valide
        private bool IsValidObjectId(string id)
        {
            return !string.IsNullOrEmpty(id) && id.Length == 24 && id.All(c => Uri.IsHexDigit(c));
        }


        public async Task ConnectAsync()
        {
            try
            {
                await _mqttClient.ConnectAsync(_mqttOptions, CancellationToken.None);
                Console.WriteLine("Successfully connected to MQTT broker.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to MQTT broker: {ex.Message}");
            }
        }

        public async Task PublishMessageAsync(string topic, string message)
        {
            if (!_mqttClient.IsConnected)
            {
                Console.WriteLine("MQTT client is not connected. Attempting to reconnect...");
                await _mqttClient.ReconnectAsync();
            }

            if (_mqttClient.IsConnected)
            {
                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(message)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await _mqttClient.PublishAsync(mqttMessage, CancellationToken.None);
                Console.WriteLine($"Message published to topic '{topic}': {message}");
            }
            else
            {
                Console.WriteLine("Failed to publish message: MQTT client is not connected.");
            }
        }
    }
}

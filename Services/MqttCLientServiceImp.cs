using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirstWebApp.Domaine.services;
using FirstWebApp.Domaine.Entities;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FirstWebApp.Infra.ServicesImp
{
    public class MqttClientService : IMqttClientService
    {
        private readonly IMqttClient _mqttClient;
        private readonly IMqttClientOptions _mqttOptions;

        public MqttClientService()
        {
            // Initialize MQTT options
            _mqttOptions = new MqttClientOptionsBuilder()
                .WithClientId("AscenseurApiClient") // Unique client ID
                .WithTcpServer("192.168.248.141", 1883) // MQTT broker details
                .WithCleanSession() // Ensure a clean session
                .Build();

            // Create the MQTT client
            _mqttClient = new MqttFactory().CreateMqttClient();

            // Configure connection handler
            _mqttClient.UseConnectedHandler(async e =>
            {
                Console.WriteLine("Connected to MQTT broker.");

                // Subscribe to topics upon successful connection
                var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter("test/topic")
                    .Build();

                await _mqttClient.SubscribeAsync(subscribeOptions, CancellationToken.None);
                Console.WriteLine("Subscribed to topic: test/topic");
            });

            // Configure disconnection handler
            _mqttClient.UseDisconnectedHandler(e =>
            {
                Console.WriteLine("Disconnected from MQTT broker.");
            });

            // Configure message reception handler
            _mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                var proprietaire = JsonSerializer.Deserialize<Proprietaire>(payload);

                // Access deserialized data
                if (proprietaire != null)
                {
                    Console.WriteLine($"Owner ID: {proprietaire.Id}");
                    foreach (var immeuble in proprietaire.Immeubles)
                    {
                        Console.WriteLine($"  Building ID: {immeuble.Id}");
                        foreach (var ascenseur in immeuble.Ascenseurs)
                        {
                            
                            if (topic == "test/topic")
                            {
                                if (ascenseur.Etat == "ASCENSEUR EST EN ETAGE 0")
                                {
                                    Console.WriteLine($"    Elevator ID: {ascenseur.Id}, State: {ascenseur.Etat}");
                                }
                                else if (ascenseur.Etat == "ASCENSEUR EST EN ETAGE 1")
                                {
                                    Console.WriteLine($"    Elevator ID: {ascenseur.Id}, State: {ascenseur.Etat}");
                                }
                                else if (ascenseur.Etat == "ASCENSEUR EST MONTANT")
                                {
                                    Console.WriteLine($"    Elevator ID: {ascenseur.Id}, State: {ascenseur.Etat}");
                                }
                                else if (ascenseur.Etat == "ASCENSEUR EST DESCENDANT")
                                {
                                    Console.WriteLine($"    Elevator ID: {ascenseur.Id}, State: {ascenseur.Etat}");
                                }
                                else
                                {
                                    Console.WriteLine("Unhandled action in message.");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Unhandled topic: {topic}");
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Failed to deserialize the payload.");
                }

            });
            
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
                await _mqttClient.ReconnectAsync();
            }

            if (_mqttClient.IsConnected)
            {
                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic) // Target topic
                    .WithPayload(message) // Message payload
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await _mqttClient.PublishAsync(mqttMessage, CancellationToken.None);
                Console.WriteLine($"Message published to {topic}: {message}");
            }
            else
            {
                Console.WriteLine("MQTT client is not connected. Cannot publish message.");
            }

        }
    }
}

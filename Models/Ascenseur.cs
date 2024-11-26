using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FirstWebApp.Domaine.Entities
{
    public class Ascenseur
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("etat")]
        public string Etat { get; set; }
    }
}

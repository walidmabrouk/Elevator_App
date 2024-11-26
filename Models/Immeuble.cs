using crudmongo.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FirstWebApp.Domaine.Entities
{
    public class Immeuble
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("ascenseurs")]
        public List<Elevator> Ascenseurs { get; set; }

    }
}

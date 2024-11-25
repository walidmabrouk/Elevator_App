using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace FirstWebApp.Domaine.Entities
{
    public class Proprietaire
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("immeubles")]
        public List<Immeuble> Immeubles { get; set; }


    }
}

using Newtonsoft.Json;

namespace CardBrowserApi.Models
{
    public class Card
    {
        public string? Name { get; set; }
        public string? FileName { get; set; }
        public string? Img { get; set; }

        public Card()
        {

        }

        [JsonConstructor]
        public Card(string? name, string? fileName)
        {
            Name = name;
            FileName = fileName;
        }       
    }
}
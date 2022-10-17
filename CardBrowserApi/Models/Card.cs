using Newtonsoft.Json;

namespace CardBrowserApi.Models
{
    public class Card
    {

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? FileName { get; private set; }
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
using Newtonsoft.Json;

namespace CardBrowserApi.Models
{
    public class Card
    {

        //public int Id { get; private set; }
        public string? Name { get; private set; }
        public string? FileName { get; private set; }
        public byte[]? Img { get; set; }

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
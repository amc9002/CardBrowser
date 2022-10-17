using CardBrowserApi.Models;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

using static System.Net.Mime.MediaTypeNames;
using System.Text;

namespace CardBrowserApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CardController : ControllerBase
    {
        private readonly ILogger<CardController> _logger;

        public CardController(ILogger<CardController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Card>? Get()
        {
            using (StreamReader r = new("Data/cards.json"))
            {
                string json = r.ReadToEnd();

                List<Card>? cards = JsonConvert.DeserializeObject<List<Card>>(json);
                if (cards != null)
                {
                    foreach (var card in cards)
                    {
                        string? fileName = "Data/Img/" + card.FileName;
                        if (fileName != null)
                        {
                            byte[] imageArray = System.IO.File.ReadAllBytes(fileName);
                            string base64ImageRepresentation = Convert.ToBase64String(imageArray);
                            card.Img = base64ImageRepresentation;
                        }
                        else _logger.LogError("File doesn't exist");
                    }
                }
                else _logger.LogError("No cards file");

                return cards;
            }
        }
    }
}
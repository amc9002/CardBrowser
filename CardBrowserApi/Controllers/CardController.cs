using CardBrowserApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using System.IO;

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

        [HttpPost]
        public IActionResult Post(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    if (IsValidImage(fileBytes))
                    {
                        List<Card>? existingListOfCards;
                        string pathToFile = "Data/cards.json";
                        using (StreamReader r = new(pathToFile))
                        {

                            string existingJson = r.ReadToEnd();
                            existingListOfCards = JsonConvert.DeserializeObject<List<Card>>(existingJson);
                        }
                        int lastId = 0;
                        if (existingListOfCards != null)
                        {
                            lastId = existingListOfCards[existingListOfCards.Count - 1].Id;
                        }

                        var srcTromClient = "data:" + file.ContentType + ";base64," + Convert.ToBase64String(fileBytes);

                        var card = new Card
                        {
                            Id = lastId + 1,
                            Name = "Something",
                            Img = srcTromClient
                        };
                        existingListOfCards?.Add(card);
                        string updatedJson = JsonConvert.SerializeObject(card);

                        using (StreamWriter writer = new StreamWriter(pathToFile, false))
                        {
                            writer.Write(updatedJson);
                        }

                        return Ok(card);

                    }
                    else
                    {
                        return BadRequest("Not valid type of file");
                    }
                }
            }
            return BadRequest("File is empty or doesn't exist");
        }

        private bool IsValidImage(byte[] filebytes)
        {
            using (var streamForValidation = new MemoryStream(filebytes))
                try
                {
                    var isValidImage = System.Drawing.Image.FromStream(streamForValidation);
                }
                catch
                {
                    return false;
                }

            return true;
        }
    }
}
using CardBrowserApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Drawing;

namespace CardBrowserApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CardController : ControllerBase
    {

        private static string path = "Data/cards.json";

        private readonly ILogger<CardController> _logger;

        public CardController(ILogger<CardController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Card>? Get()
        {
            List<Card>? cards = LoadCards();
            if (cards == null) cards = new List<Card>();
            else
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
            return cards;
        }

        [HttpPost]
        public IActionResult Post(Card newCard)
        {
            if (newCard != null)
            {
                if (newCard.Img != null)
                {
                    var byteImg = Convert.FromBase64String(newCard.Img);
                    if (IsValidImage(byteImg))
                    {
                        List<Card>? existingListOfCards = LoadCards();
                        var card = new Card
                        {
                            Name = newCard.Name,
                            FileName = newCard.FileName,
                        };
                        using (var imageFile = new FileStream("Data/Img/" + newCard.FileName, FileMode.Create))
                        {
                            imageFile.Write(byteImg, 0, byteImg.Length);
                            imageFile.Flush();
                        }

                        existingListOfCards?.Add(card);
                        string updatedJson = JsonConvert.SerializeObject(existingListOfCards);

                        using (StreamWriter writer = new(path, false))
                        {
                            writer.Write(updatedJson);
                        }

                        return Ok(card);
                    }
                    else return BadRequest("Not valid type of file");
                }
            }
            return BadRequest("File is empty or doesn't exist");
        }

        private static List<Card>? LoadCards()
        {
            using StreamReader r = new(path);
            string json = r.ReadToEnd();

            List<Card>? cards = JsonConvert.DeserializeObject<List<Card>>(json);
            return cards;
        }

        private static bool IsValidImage(byte[] filebytes)
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
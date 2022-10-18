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
            if (newCard == null) return BadRequest("Card is empty or doesn't exist");
            if (newCard.Img == null) return BadRequest("No images in card");

            var byteImg = Convert.FromBase64String(newCard.Img);
            if (!IsValidImage(byteImg)) return BadRequest("Not valid type of file");

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
            if(existingListOfCards != null) SaveToFile(existingListOfCards);
            
            return Ok(card);
        }

        [HttpPut]
        public IActionResult Put(Card card)
        {
            if (card == null) return BadRequest("File is empty or doesn't exist");

            List<Card>? existingListOfCards = LoadCards();
            if(existingListOfCards == null) return NotFound("No cards in the store");

            foreach(var c in existingListOfCards)           
                if(card.FileName == c.FileName)
                {
                    card.Name = card.Name;
                    break;
                }
           
            if (existingListOfCards != null) SaveToFile(existingListOfCards);

            return Ok("Successfully updated");
        }

        //[HttpDelete("{fileName:string}")]
        [HttpDelete]
        public IActionResult Delete(string fileName)
        {
            var existingListOfCards = LoadCards();
            if (existingListOfCards == null) return NotFound("No cards in the store");

            foreach (var c in existingListOfCards)
                if (fileName == c.FileName)
                {
                    existingListOfCards.Remove(c);
                    break;
                }

            if (existingListOfCards != null) SaveToFile(existingListOfCards);

            return Ok("Successfully deleted");
        }

        private static List<Card>? LoadCards()
        {
            using StreamReader r = new(path);
            string json = r.ReadToEnd();

            List<Card>? cards = JsonConvert.DeserializeObject<List<Card>>(json);
            return cards;
        }

        private IActionResult SaveToFile(List<Card> cards)
        {
            
            string json = JsonConvert.SerializeObject(cards);
            using (StreamWriter writer = new(path, false))
            {
                writer.Write(string.Empty);
                writer.Write(json);
            }
            return Ok("Saved");
        }

        private static bool IsValidImage(byte[] filebytes)
        {
            using (var streamForValidation = new MemoryStream(filebytes))
                try
                {
                    var isValidImage = Image.FromStream(streamForValidation);
                }
                catch
                {
                    return false;
                }

            return true;
        }


    }
}
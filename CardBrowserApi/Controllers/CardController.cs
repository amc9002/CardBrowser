using CardBrowserApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Drawing;

namespace CardBrowserApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardController : ControllerBase
    {

        private static readonly string pathToJsonFile = "Data/cards.json";
        private static readonly string pathToImg = "Data/Img/";
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
                    string? fileName = pathToImg + card.FileName;
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
            if (newCard.Img == null) return NotFound("No images in card");

            var byteImg = Convert.FromBase64String(newCard.Img);
            if (!IsValidImage(byteImg)) return BadRequest("Not valid type of file");

            List<Card>? existingListOfCards = LoadCards();
            if (existingListOfCards != null)
                foreach (var c in existingListOfCards)
                    if (c.FileName == newCard.FileName) return BadRequest("The card with the same file name already exists");

            var card = new Card
            {
                Name = newCard.Name,
                FileName = newCard.FileName,
            };
            using (var imageFile = new FileStream(pathToImg + newCard.FileName, FileMode.Create))
            {
                imageFile.Write(byteImg, 0, byteImg.Length);
                imageFile.Flush();
            }

            existingListOfCards?.Add(card);
            if (existingListOfCards != null) SaveToFile(existingListOfCards);

            return Ok(card);
        }

        [HttpPut]
        public IActionResult Put(Card card)
        {
            if (card == null) return BadRequest("File is empty or doesn't exist");

            List<Card>? existingListOfCards = LoadCards();
            if (existingListOfCards == null) return BadRequest("No cards in the store");

            foreach (var c in existingListOfCards)
                if (card.FileName == c.FileName)
                {
                    c.Name = card.Name;
                    break;
                }

            SaveToFile(existingListOfCards);

            return Ok("Successfully updated");
        }

        [HttpDelete("{fileName}")]
        public IActionResult Delete(string fileName)
        {
            var existingListOfCards = LoadCards();
            if (existingListOfCards == null) return NotFound("No cards in the store");

            bool found = false;
            foreach (var c in existingListOfCards)
            {
                if (fileName == c.FileName)
                {
                    existingListOfCards.Remove(c);
                    found = true;
                    FileInfo file = new ("Data/Img/" + fileName);
                    file.Delete();
                    break;
                }
            }
            if (!found) return NotFound("No file with this file name");



            SaveToFile(existingListOfCards);

            return Ok("Successfully deleted");
        }

        private static List<Card>? LoadCards()
        {
            using StreamReader r = new(pathToJsonFile);
            string json = r.ReadToEnd();

            List<Card>? cards = JsonConvert.DeserializeObject<List<Card>>(json);
            return cards;
        }

        private IActionResult SaveToFile(List<Card> cards)
        {

            string json = JsonConvert.SerializeObject(cards);
            using (StreamWriter writer = new(pathToJsonFile, false))
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
using CardBrowserApi.Models;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace CardBrowserApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CardBrowserController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<CardBrowserController> _logger;

        public CardBrowserController(ILogger<CardBrowserController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<Card>? Get()
        {
            using (StreamReader r = new StreamReader("Data/cards.json"))
            {
                string json = r.ReadToEnd();

                List<Card>? cards = JsonConvert.DeserializeObject<List<Card>>(json);

                return cards;

            }

        }

    }

}
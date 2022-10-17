using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CardBrowser.Models;
using System.IO;
using System.Windows.Media.Imaging;
using System.Net.Http.Json;

namespace CardBrowser
{
    public class CardBrowserApiClient
    {
        public static List<Card> Get()
        {
            HttpClient client = new()
            {
                BaseAddress = new Uri("https://localhost:7191/")
            };
            HttpResponseMessage response = client.GetAsync("Card").Result;

            var emptyCards = new List<Card>();
            if (response.IsSuccessStatusCode)
            {
                string? json = response.Content.ReadAsStringAsync().Result;
                if (json == null)
                {
                    MessageBox.Show("Card doesn't exist");
                    return emptyCards;
                }

                List<Card>? cards = JsonConvert.DeserializeObject<List<Card>>(json);
                if (cards != null)
                {
                    foreach (var card in cards)
                    {
                        if (card.Img == null) continue;

                        byte[] bitImg = Convert.FromBase64String(card.Img);
                        if (bitImg == null || bitImg.Length == 0) continue;

                        card.BitmapImage = ByteArrayToImage(bitImg);
                    }
                    return cards;
                }
                return emptyCards;
            }
            MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            return emptyCards;
        }

        public static bool Post(Card newCard)
        {
            if (newCard == null) return false;

            HttpClient client = new()
            {
                BaseAddress = new Uri("https://localhost:7191/")
            };
            HttpResponseMessage response = client.GetAsync("Card").Result;
            if (response == null) return false;

            response = client.PostAsJsonAsync("api/person", newCard).Result;
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Succesfully posted");
            }
            else
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);

            return true;
        }

        public static BitmapImage ByteArrayToImage(byte[] bitImg)
        {
            var bitmapImage = new BitmapImage();
            using (var mem = new MemoryStream(bitImg))
            {
                mem.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = null;
                bitmapImage.StreamSource = mem;
                bitmapImage.EndInit();
            }
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}

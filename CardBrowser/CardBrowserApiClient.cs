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
        private readonly static Uri baseAddress = new ("https://localhost:7191/");
        private static readonly string pathToApi = "api/Card";
        public static List<Card> Get()
        {
            HttpClient client = new()
            {
                BaseAddress = baseAddress
            };
            HttpResponseMessage response = client.GetAsync(pathToApi).Result;

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
            MessageBox.Show(response.Content.ReadAsStringAsync().Result, "Error" + response.StatusCode);
            return emptyCards;
        }

        public static bool Post(Card newCard)
        {
            if (newCard == null) return false;

            HttpClient client = new()
            {
                BaseAddress = baseAddress
            };

            HttpResponseMessage response = client.PostAsJsonAsync(pathToApi, newCard).Result;
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Succesfully posted");
            }
            else
                MessageBox.Show(response.Content.ReadAsStringAsync().Result, "Error" + response.StatusCode);

            return true;
        }

        public static bool Put(Card updatedCard)
        {
            if (updatedCard == null) return false;

            HttpClient client = new()
            {
                BaseAddress = baseAddress
            };
            HttpResponseMessage response = client.PutAsJsonAsync(pathToApi, updatedCard).Result;
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Succesfully updated");
            }
            else
                MessageBox.Show(response.Content.ReadAsStringAsync().Result, "Error" + response.StatusCode);

            return true;
        }

        public static bool Delete(string filename)
        {
            if (filename == null) return false;

            HttpClient client = new()
            {
                BaseAddress = baseAddress
            };
            HttpResponseMessage response = client.DeleteAsync(pathToApi  + "/" + filename).Result;
            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show(response.Content.ReadAsStringAsync().Result, "Error" + response.StatusCode);
                return false;
            }

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

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
        private readonly static Uri baseAddress = new("https://localhost:7191/");

        private readonly static HttpClient client = new()
        {
            BaseAddress = baseAddress
        };

        private static readonly string pathToApi = "api/Card";
        public static CardBrowserApiResponse Get()
        {
            HttpResponseMessage response;
            var responseForMainWindow = new CardBrowserApiResponse();
            try
            {
                response = client.GetAsync(pathToApi).Result;
            }

            catch (Exception e)
            {
                responseForMainWindow.Error = $"Error: {e}";
                return responseForMainWindow;
            }

            if (response.IsSuccessStatusCode)
            {
                string? json = response.Content.ReadAsStringAsync().Result;
                if (json == null)
                {
                    responseForMainWindow.Error = "List of cards doesn't exist";
                    return responseForMainWindow;
                }

                List<Card>? cards = JsonConvert.DeserializeObject<List<Card>>(json);
                if (cards == null)
                {
                    responseForMainWindow.Error = "No cards in store";
                    responseForMainWindow.Data = new List<Card>();
                    return responseForMainWindow;
                }

                foreach (var card in cards)
                {
                    if (card.Img == null) continue;

                    byte[] bitImg = Convert.FromBase64String(card.Img);
                    if (bitImg == null || bitImg.Length == 0) continue;

                    card.BitmapImage = ByteArrayToImage(bitImg);
                }
                responseForMainWindow.Data = cards;
                return responseForMainWindow;
            }

            responseForMainWindow.Error = response.Content.ReadAsStringAsync().Result + ". Error: " + response.StatusCode;
            return responseForMainWindow;
        }

        public static CardBrowserApiResponse Post(Card newCard)
        {
            var responseForMainWindow = new CardBrowserApiResponse();
            if (newCard == null)
            {
                responseForMainWindow.Error = "No card chosen";
                return responseForMainWindow;
            }

            HttpResponseMessage response;
            try
            {
                response = client.PostAsJsonAsync(pathToApi, newCard).Result;
            }

            catch (Exception e)
            {
                responseForMainWindow.Error = $"Error: {e}";
                return responseForMainWindow;
            }

            if (!response.IsSuccessStatusCode)
                responseForMainWindow.Error = response.Content.ReadAsStringAsync().Result + ". Error: " + response.StatusCode;               

            return responseForMainWindow;
        }

        public static CardBrowserApiResponse Put(Card updatedCard)
        {
            var responseForMainWindow = new CardBrowserApiResponse();
            if (updatedCard == null) 
            {
                responseForMainWindow.Error = "No card chosen";
                return responseForMainWindow;
            }

            HttpResponseMessage response;
            try
            {
                response = client.PutAsJsonAsync(pathToApi, updatedCard).Result;
            }

            catch (Exception e)
            {
                responseForMainWindow.Error = $"Error: {e}";
                return responseForMainWindow;
            }

            if (!response.IsSuccessStatusCode)
                responseForMainWindow.Error = response.Content.ReadAsStringAsync().Result + ". Error: " + response.StatusCode;

            return responseForMainWindow;
        }

        public static CardBrowserApiResponse Delete(Card сardToDelete)
        {
            var responseForMainWindow = new CardBrowserApiResponse();
            if (сardToDelete.FileName == null) 
            {
                responseForMainWindow.Error = "No card chosen";
                return responseForMainWindow;
            }

            HttpResponseMessage response;
            try
            {
                response = client.DeleteAsync(pathToApi + "/" + сardToDelete.FileName).Result;
            }

            catch (Exception e)
            {
                responseForMainWindow.Error = $"Error: {e}";
                return responseForMainWindow;
            }

            if (!response.IsSuccessStatusCode)
                responseForMainWindow.Error = response.Content.ReadAsStringAsync().Result + ". Error: " + response.StatusCode;

            return responseForMainWindow;
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

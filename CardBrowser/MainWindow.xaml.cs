using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using Microsoft.Win32;

namespace CardBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadCards();
        }

        public void LoadCards()
        {
            List<Card> cards = (List<Card>)Get();
            foreach (var card in cards)
            {
                listCards.Items.Add(card);
            }
        }

        public static ICollection<Card> Get()
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
                        card.BitmapImage = bitmapImage;
                    }
                return cards;
                }
                
                return emptyCards;

            }
            MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            return emptyCards;
        }

        public static bool Post(Card? newCard)
        {
            if(newCard == null) return false;
 
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

        public class Card
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? FileName { get; set; }
            public string? Img { get; set; }
            public byte[]? BitImg { get; set; }           
            public BitmapImage? BitmapImage { get; set; }
            

        }

        private void Click_UploadFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog browseFiles = new OpenFileDialog();
            browseFiles.Filter = "Image files (*.png;*.jpg)|*.png;*.jpg|All files (*.*)|*.*";
            if(browseFiles.ShowDialog() == true)
            {
                //txtEditor.Text = File.ReadAllText(openFileDialog.FileName);
            }
        }
    }

}

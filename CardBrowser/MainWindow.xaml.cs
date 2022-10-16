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
                        card.BitImg = Convert.FromBase64String(card.Img);
                    }
                return cards;
                }
                
                return emptyCards;

            }
            MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
            return emptyCards;
        }

        public void LoadCards()
        {
            List<Card> cards = (List<Card>)Get();
            foreach (var card in cards)
            {
                listCards.Items.Add(card);
            }

        }
        public class Card
        {
            public string? Name { get; set; }
            public string? FileName { get; set; }
            public string? Img { get; set; }
            public byte[]? BitImg { get; set; }    
        }

    }




}

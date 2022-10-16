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



        public static ICollection<Card>? Get()
        {
            HttpClient client = new()
            {
                BaseAddress = new Uri("https://localhost:7191/")
            };

            HttpResponseMessage response = client.GetAsync("Card").Result;

            if (response.IsSuccessStatusCode)
            {
                string? json = response.Content.ToString();
                if (json != null)
                {
                    List<Card>? cards = JsonConvert.DeserializeObject<List<Card>>(json);
                    return cards;
                }
                else
                {
                    MessageBox.Show("Card doesn't exist");
                    return null;
                }
            }
            else
            {
                MessageBox.Show("Error Code" + response.StatusCode + " : Message - " + response.ReasonPhrase);
                return null;
            }
        }

        public static void LoadCards()
        {
            DataTable? cards = (DataTable)Get();
            if (cards != null)
            {
                for (int i = 0; i < cards.Rows.Count; i++)
                {
                    Card dataCard = new()
                    {
                        Img = cards.Rows[i][0].ToString(),
                        Name = cards.Rows[i][1].ToString()
                    };

                    ListCards.Items.Add(dataCard);
                }
            }


        }
        public class Card
        {
            public string? Name { get; set; }
            public string? FileName { get; set; }
            public string? Img { get; set; }
        }

    }




}

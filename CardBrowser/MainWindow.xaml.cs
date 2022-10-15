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
        }

        public static ICollection<Card>? Get()
        {
            HttpClient client = new()
            {
                BaseAddress = new Uri("http://localhost:7191/")
            };

            HttpResponseMessage response = client.GetAsync("cards").Result;

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
    }

    public class Card
    {
        public string? Name { get; set; }
        public string? FileName { get; set; }
        public string? Img { get; set; }
    }
}

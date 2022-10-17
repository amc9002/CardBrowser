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
using CardBrowser.Models;
using Path = System.IO.Path;

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
            var cards = CardBrowserApiClient.Get();
            foreach (var card in cards)
                listCards.Items.Add(card);
        }

         private void Click_AddCard(object sender, RoutedEventArgs e)
        {
            OpenFileDialog browseFiles = new()
            {
                Filter = "Image files (*.png;*.jpg)|*.png;*.jpg|All files (*.*)|*.*"
            };

            if (browseFiles.ShowDialog() == true)
            {
                string fullPath = browseFiles.FileName;
                fullPathBox.Text = fullPath;
                path.Text = Path.GetFileName(fullPath);
                byte[] fileBody = File.ReadAllBytes(fullPath);

                if (fileBody == null || fileBody.Length == 0) return;

                bigImage.Source = CardBrowserApiClient.ByteArrayToImage(fileBody);
                

            }
        }

        private void Click_UploadFile(object sender, RoutedEventArgs e)
        {
            if (path.Text != null && cardName.Text != "")
            {
                byte[] fileBody = File.ReadAllBytes(fullPathBox.Text);
                string base64ImageRepresentation = Convert.ToBase64String(fileBody);
                var newCard = new Card
                {
                    Name = cardName.Text,
                    FileName = Path.GetFileName(path.Text),
                    Img = base64ImageRepresentation
                };
                CardBrowserApiClient.Post(newCard);
                LoadCards();
            }
        }
    
    }
}

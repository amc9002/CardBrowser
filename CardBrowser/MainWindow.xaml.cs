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
using System.Security.Policy;
using System.Windows.Controls.Primitives;

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
            listCards.Items.Clear();
            var response = CardBrowserApiClient.Get();
            if (response.Error != null)
            {
                MessageBox.Show(response.Error);
                return;
            }

            if (response.Data != null)
            {
                var cards = (List<Card>)response.Data;
                foreach (var card in cards)
                    listCards.Items.Add(card);
            }
            return;
        }



        private void Click_Browse(object sender, RoutedEventArgs e)
        {
            OpenFileDialog browseFiles = new()
            {
                Filter = "Image files (*.png;*.jpg)|*.png;*.jpg|All files (*.*)|*.*"
            };

            if (browseFiles.ShowDialog() == true)
            {
                string fullPathFromBrowser = browseFiles.FileName;
                fullPath.Text = fullPathFromBrowser;
                path.Text = Path.GetFileName(fullPathFromBrowser);
                byte[] bitImg = File.ReadAllBytes(fullPathFromBrowser);

                if (bitImg == null || bitImg.Length == 0) return;

                bigImage.Source = CardBrowserApiClient.ByteArrayToImage(bitImg);
                cardName.Text = string.Empty;            
            }
        }

        private void Click_UploadFile(object sender, RoutedEventArgs e)
        {
            if (fullPath.Text == String.Empty)
            {
                MessageBox.Show("No new card has been selected");
                return;
            }
            
            if (!cardName.Text.Any(c => char.IsLetter(c))
                && string.IsNullOrEmpty(cardName.Text))
            {
                MessageBox.Show("Enter Name of card, please");
                return;
            }

            MessageBoxResult permission = MessageBox.Show(
                "Are you sure?",
                "Cancel",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel,
                MessageBoxOptions.DefaultDesktopOnly);

            if (permission == MessageBoxResult.Cancel)
            {
                return;
            }

            

            byte[] fileBody = File.ReadAllBytes(fullPath.Text);
            string base64ImageRepresentation = Convert.ToBase64String(fileBody);
            var newCard = new Card
            {
                Name = cardName.Text,
                FileName = Path.GetFileName(path.Text),
                Img = base64ImageRepresentation
            };

            var response = CardBrowserApiClient.Post(newCard);
            if (response.Error == null)
            {
                MessageBox.Show("Succesfully posted");
                fullPath.Text = string.Empty;
            }

            else MessageBox.Show(response.Error);

            LoadCards();
        }

        private void ListCards_Click(object sender, MouseButtonEventArgs e)
        {
            if(fullPath.Text != string.Empty)
            {
                MessageBoxResult permission = MessageBox.Show(
               "File hasn't been uploaded?",
               "Cancel",
               MessageBoxButton.OKCancel,
               MessageBoxImage.Warning,
               MessageBoxResult.Cancel,
               MessageBoxOptions.DefaultDesktopOnly);

                if (permission == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            var item = (Card)((ListView)sender).SelectedItem;
            if (item != null)
            {
                cardName.Text = item.Name;
                path.Text = item.FileName;
                if (item.Img == null)
                {
                    MessageBox.Show("No picture");
                    return;
                }
                byte[] bitImg = Convert.FromBase64String(item.Img);
                bigImage.Source = CardBrowserApiClient.ByteArrayToImage(bitImg);
                fullPath.Text = string.Empty;
            }
        }

        private void Click_SaveNewName(object sender, RoutedEventArgs e)
        {
            string existingName = cardName.Text;

            if (!cardName.Text.Any(c => char.IsLetter(c))
            && string.IsNullOrEmpty(cardName.Text))
            {
                MessageBox.Show("Enter Name of card, please");
                return;
            }

            MessageBoxResult permission = MessageBox.Show(
                "Are you sure?",
                "Update name",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel,
                MessageBoxOptions.DefaultDesktopOnly);

            if (permission == MessageBoxResult.Cancel)
            {
                cardName.Text = existingName;
                return;
            }

            var editedCard = new Card
            {
                Name = cardName.Text,
                FileName = path.Text,
            };

            var response = CardBrowserApiClient.Put(editedCard);
            if (response.Error == null)
                MessageBox.Show("Succesfully updated");

            else MessageBox.Show(response.Error);

            LoadCards();
        }

        private void Click_DeleteCard(object sender, RoutedEventArgs e)
        {
            MessageBoxResult permission = MessageBox.Show(
                "Are you sure?",
                "Delete card",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel,
                MessageBoxOptions.DefaultDesktopOnly);

            if (permission == MessageBoxResult.Cancel) return;

            var cardToDelete = new Card
            {
                FileName = path.Text
            };

            var response = CardBrowserApiClient.Delete(cardToDelete);
            if (response.Error != null)
            {
                MessageBox.Show(response.Error);
                return;
            }

            MessageBox.Show("Succesfully deleted");

            cardName.Text = string.Empty;
            path.Text = string.Empty;
            fullPath.Text = string.Empty;
            bigImage.Source = null;

            LoadCards();

        }
    }
}

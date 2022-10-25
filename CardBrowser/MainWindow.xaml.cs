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
using System.Drawing.Printing;


namespace CardBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string lastName = string.Empty;
        public MainWindow()
        {
            InitializeComponent();
            LoadCards();
        }

        public void LoadCards()
        {
            var response = CardBrowserApiClient.Get();
            if (response.Error != null)
            {
                MessageBox.Show(response.Error);
                return;
            }

            if (response.Data != null)
            {
                listCards.Items.Clear();
                var cards = (List<Card>)response.Data;
                foreach (var card in cards)
                    listCards.Items.Add(card);
            }

            changeNameButton.IsEnabled = false;
            saveNameButton.IsEnabled = false;
            uploadButton.IsEnabled = false;
            deleteButton.IsEnabled = false;
            cardName.IsReadOnly = true;

            return;
        }

        private void Click_Browse(object sender, RoutedEventArgs e)
        {
            cardName.IsReadOnly = true;

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

                changeNameButton.IsEnabled = false;
                saveNameButton.IsEnabled = false;
                uploadButton.IsEnabled = true;
                deleteButton.IsEnabled = false;

                return;
            }
        }

        private void Click_UploadFile(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(fullPath.Text))
            {
                MessageBox.Show("No file has been selected");
                return;
            }

            cardName.IsReadOnly = false;

            if (!cardName.Text.Any(c => char.IsLetter(c))
                || string.IsNullOrEmpty(cardName.Text))
            {
                MessageBox.Show("Enter Name of card, please");
                return;
            }

            cardName.IsReadOnly = true;

            MessageBoxResult permission = MessageBox.Show(
                "Are you sure?",
                "Cancel",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel,
                MessageBoxOptions.DefaultDesktopOnly);

            if (permission == MessageBoxResult.Cancel) return;

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
            if (!string.IsNullOrEmpty(fullPath.Text))
            {
                MessageBoxResult permission = MessageBox.Show(
               "File hasn't been uploaded?",
               "Cancel",
               MessageBoxButton.OKCancel,
               MessageBoxImage.Warning,
               MessageBoxResult.Cancel,
               MessageBoxOptions.DefaultDesktopOnly);

                if (permission == MessageBoxResult.Cancel) return;
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

            changeNameButton.IsEnabled = true;
            saveNameButton.IsEnabled = false;
            deleteButton.IsEnabled = true;
            uploadButton.IsEnabled = false;
            cardName.IsReadOnly = true;

        }

        private void Click_ChangeName(object sender, RoutedEventArgs e)
        {
            if (lastName == string.Empty)
                lastName = cardName.Text;

            changeNameButton.IsEnabled = false;
            saveNameButton.IsEnabled = true;
            cardName.IsReadOnly = false;
        }

        private void Click_SaveName(object sender, RoutedEventArgs e)
        {
            if (!cardName.Text.Any(c => char.IsLetter(c))
                        && string.IsNullOrEmpty(cardName.Text))
            {
                MessageBox.Show("Enter Name of card, please"); ;
                return;
            }

            cardName.IsReadOnly = true;

            MessageBoxResult permission = MessageBox.Show(
                "Are you sure?",
                "Update name",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel,
                MessageBoxOptions.DefaultDesktopOnly);

            if (permission == MessageBoxResult.Cancel)
            {
                cardName.Text = lastName;
                lastName = string.Empty;
                cardName.IsReadOnly = true;
                saveNameButton.IsEnabled = false;
                changeNameButton.IsEnabled = true;

                return;
            }

            lastName = string.Empty;


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
            changeNameButton.IsEnabled = false;
            deleteButton.IsEnabled = false;

            LoadCards();

        }

        private void Click_Sort(object sender, RoutedEventArgs e)
        {
            var response = CardBrowserApiClient.Get();
            if (response.Error != null)
            {
                MessageBox.Show(response.Error);
                return;
            }

            if (response.Data != null)
            {
                listCards.Items.Clear();
                var cards = (List<Card>)response.Data;
                var orderedCards = cards.OrderBy(x => x.Name);
                foreach (var card in orderedCards)
                    listCards.Items.Add(card);
            }
        }
    }
}

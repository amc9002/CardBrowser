using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace CardBrowser.Models
{
    public class Card
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? FileName { get; set; }
        public string? Img { get; set; }
        public BitmapImage? BitmapImage { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CardBrowser
{
    public class CardBrowserApiResponse
    {
        public string? Error { get; set; }
        public object? Data { get; set; }        
    }
}

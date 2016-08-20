using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoodVibesWeb.Models
{
    public class Song
    {
        public string title { get; set; }
        public string song_artist { get; set; }
        public string song_name { get; set; }
        public string song_url { get; set; }
        public string duration { get; set; }
        public string source { get; set; }
        public string permaurl { get; set; }
        public int id { get; set; }
        public string username { get; set; }
        public DateTime song_added { get; set; }
    }
}
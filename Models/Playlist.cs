using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoodVibesWeb.Models
{
    public class Playlist
    {
        public string playlist_name { get; set; }
        public int playlist_id { get; set; }
        public bool deletable { get; set; }
    }
}
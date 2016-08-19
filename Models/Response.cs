using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoodVibesWeb.Models
{
    public class Response
    {
        public string type { get; set; }
        public string msg { get; set; }

        public Response(string type, string msg)
        {
            this.type = type;
            this.msg = msg;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GoodVibesWeb.Models;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace GoodVibesWeb.Controllers
{
    public class SoundCloudDataController : ApiController
    {
        [Route("api/searchsoundcloud/{keyword}")]
        [HttpGet]
        public HttpResponseMessage searchSoundCloud(string keyword)
        {
            string html = string.Empty;
            string url = "http://api.soundcloud.com/tracks?linked_partitioning=1&client_id=" + ServerData.sc_clientid + "&q=" + keyword + "&limit=100";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);


            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }

            List<Song> result = new List<Song>();
            dynamic dynJson = JsonConvert.DeserializeObject(html);
            foreach (var item in dynJson.collection)
            {
                if (item.streamable == true)
                {
                    Song s = new Song();
                    s.title = item.title;
                    s.song_url = item.id;
                    s.duration = item.duration;
                    s.source = "soundcloud";
                    s.permaurl = item.permalink_url;
                    result.Add(s);
                }
            }

            HttpResponseMessage response2 = Request.CreateResponse(HttpStatusCode.OK);
            response2.Headers.Add("Access-Control-Allow-Origin", "*");
            response2.Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json");

            return response2;
        }
    }
}

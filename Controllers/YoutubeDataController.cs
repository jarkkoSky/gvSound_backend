using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using GoodVibesWeb.Models;
using Newtonsoft.Json;
using System.Text;

namespace GoodVibesWeb.Controllers
{
    public class YoutubeDataController : ApiController
    {
        [Route("api/searchyoutube/{keyword}/")]
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> SearchYoutube(string keyword)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = ServerData.apikey,
                ApplicationName = this.GetType().ToString()
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = keyword;
            searchListRequest.MaxResults = 50;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            List<Song> videos = new List<Song>();

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var searchResult in searchListResponse.Items)
            {
                if (searchResult.Id.Kind == "youtube#video")
                {
                    Song s = new Song();
                    s.title = searchResult.Snippet.Title;
                    s.song_url = searchResult.Id.VideoId;
                    videos.Add(s);
                }
            }

            string result = JsonConvert.SerializeObject(videos);
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Content = new StringContent(result, Encoding.UTF8, "application/json");

            return response;
        }
    }
}

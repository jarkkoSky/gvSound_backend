using System;
using System.Web;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using GoodVibesWeb.Models;
using System.Text;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;


namespace GoodVibesWeb.Controllers
{
    public class SongDataController : ApiController
    {
        string sConn =
            ServerData.sconn;

        [Route("api/getplaylists/{username}/")]
        [HttpGet]
        public HttpResponseMessage GetPlaylists(string username)
        {
            string result;
            List<Playlist> playlists = new List<Playlist>();
            try
            {
                using (SqlConnection sc = new SqlConnection(sConn))
                {
                    sc.Open();
                    string sql = "SELECT playlist_id, playlist_name,deletable FROM dbo.playlists where username = @username or username = 'all_users' order by deletable, playlist_name";
                    SqlCommand com = new SqlCommand(sql, sc);
                    com.Parameters.AddWithValue("@username", username);

                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Playlist plist = new Playlist();
                            plist.playlist_id = int.Parse(reader["playlist_id"].ToString());
                            plist.playlist_name = reader["playlist_name"].ToString();
                            plist.deletable = (bool)reader["deletable"];
                            playlists.Add(plist);
                        }
                    }
                }

                result = JsonConvert.SerializeObject(playlists);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(result, Encoding.UTF8, "application/json");
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                return response;
            }
            catch
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
                response.Content = new StringContent("Error occured while getting playlists, Refresh page!", Encoding.UTF8, "application/json");
                return response;
            }
        }

        [Route("api/getplaylist/{username}/{playlist_id}/")]
        [HttpGet]
        public HttpResponseMessage GetPlaylist(string username, int playlist_id)
        {
            string result;
            List<Song> playlist = new List<Song>();

            try
            {
                using (SqlConnection sc = new SqlConnection(sConn))
                {
                    sc.Open();
                    string sql = "SELECT song_url, song_artist, song_name, id, creation_date, UPPER(username) as username FROM dbo.song_data where playlist_id = @playlist_id order by creation_date desc";
                    SqlCommand com = new SqlCommand(sql, sc);
                    com.Parameters.AddWithValue("@username", username);
                    com.Parameters.AddWithValue("@playlist_id", playlist_id);

                    using (SqlDataReader reader = com.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Song song = new Song();
                            song.song_url = reader["song_url"].ToString();
                            song.song_artist = reader["song_artist"].ToString();
                            song.song_name = reader["song_name"].ToString();
                            song.id = int.Parse(reader["id"].ToString());
                            song.song_added = (DateTime)reader["creation_date"];
                            song.username = reader["username"].ToString();
                            playlist.Add(song);
                        }
                    }
                }

                result = JsonConvert.SerializeObject(playlist);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(result, Encoding.UTF8, "application/json");
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                return response;
            }
            catch
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent("Error occured while getting playlist data, refresh page!", Encoding.UTF8, "application/json");
                return response;
            }
        }

        [Route("api/insertplaylist/{username}/")]
        [HttpPost]
        public HttpResponseMessage InsertPlaylist(Playlist playlist, string username)
        {
            try
            {
                int playlist_id;

                using (SqlConnection sc = new SqlConnection(sConn))
                {
                    sc.Open();
                    string sql = "insert into playlists(playlist_name, username, deletable) values(@playlist_name, @username, 1)";
                    SqlCommand com = new SqlCommand(sql, sc);
                    com.Parameters.AddWithValue("@playlist_name", playlist.playlist_name);
                    com.Parameters.AddWithValue("@username", username);
                    com.ExecuteNonQuery();

                    sql = "select playlist_id from playlists where @playlist_name = playlist_name and @username = username";
                    com = new SqlCommand(sql, sc);
                    com.Parameters.AddWithValue("@playlist_name", playlist.playlist_name);
                    com.Parameters.AddWithValue("@username", username);
                    playlist_id = int.Parse(com.ExecuteScalar().ToString());
                }

                Playlist newlist = new Playlist();
                newlist.playlist_name = playlist.playlist_name;
                newlist.playlist_id = playlist_id;
                newlist.deletable = true;

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(JsonConvert.SerializeObject(newlist), Encoding.UTF8, "application/json");
                return response;
            }
            catch
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
                Response r = new Response("error", "Error occured while inserting new playlist, try again!");
                response.Content = new StringContent(JsonConvert.SerializeObject(r), Encoding.UTF8, "application/json");
                return response;
            }
        }



        [Route("api/insertsong/{username}/{playlist_id}/")]
        [HttpPost]
        public HttpResponseMessage Post(Song s, string username, int playlist_id)
        {
            HttpResponseMessage response;


            if (s.song_url.Length != 11)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest);
                response.Content = new StringContent("Video ID invalid. Must be 11 characters", Encoding.UTF8, "application/json");
                response.Headers.Add("Access-Control-Allow-Origin", "*");

                return response;
            }
            else
            {
                try
                {
                    using (SqlConnection sc = new SqlConnection(sConn))
                    {
                        sc.Open();
                        string sql = "select song_url from song_data where playlist_id = @playlist_id and song_url = @song_url";
                        SqlCommand com = new SqlCommand(sql, sc);
                        com.Parameters.AddWithValue("@playlist_id", playlist_id);
                        com.Parameters.AddWithValue("@song_url", s.song_url);
                        object o = com.ExecuteScalar();
                        if (o == null)
                        {
                            sql = "Insert into song_data (song_url,song_name,song_artist,username, playlist_id) values (@song_url,@song_name,@song_artist,@username,@playlist_id)";
                            com = new SqlCommand(sql, sc);
                            com.Parameters.AddWithValue("@song_url", s.song_url);
                            com.Parameters.AddWithValue("@song_name", s.song_name);
                            com.Parameters.AddWithValue("@song_artist", s.song_artist);
                            com.Parameters.AddWithValue("@username", username);
                            com.Parameters.AddWithValue("@playlist_id", playlist_id);
                            com.ExecuteNonQuery();
                        }
                        else
                        {
                            response = Request.CreateResponse(HttpStatusCode.BadRequest);
                            response.Content = new StringContent("Song already exists in this playlist", Encoding.UTF8, "application/json");
                            response.Headers.Add("Access-Control-Allow-Origin", "*");

                            return response;
                        }
                    }
                    response = Request.CreateResponse(HttpStatusCode.OK);
                    Response r = new Response("success", "Song Added");
                    response.Content = new StringContent(JsonConvert.SerializeObject(r), Encoding.UTF8, "application/json");

                    return response;
                }
                catch (Exception ex)
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest);
                    response.Content = new StringContent("Error occured while adding song, try again!" + ex.Message, Encoding.UTF8, "application/json");
                    response.Headers.Add("Access-Control-Allow-Origin", "*");

                    return response;
                }
            }
        }

        [Route("api/deleteplaylist/{username}/{playlist_id}/")]
        [HttpDelete]
        public HttpResponseMessage DeletePlaylist(string username, string playlist_id)
        {
            try
            {
                using (SqlConnection sc = new SqlConnection(sConn))
                {
                    sc.Open();
                    string sql = "Delete from song_data where playlist_id = @playlist_id";
                    SqlCommand com = new SqlCommand(sql, sc);
                    com.Parameters.AddWithValue("@playlist_id", playlist_id);
                    com.ExecuteNonQuery();

                    sql = "Delete from playlists where playlist_id = @playlist_id and username = @username and deletable = 1";
                    com = new SqlCommand(sql, sc);
                    com.Parameters.AddWithValue("@playlist_id", playlist_id);
                    com.Parameters.AddWithValue("@username", username);
                    com.ExecuteNonQuery();
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                Response r = new Response("success", "Playlist deleted");
                response.Content = new StringContent(JsonConvert.SerializeObject(r), Encoding.UTF8, "application/json");

                return response;
            }
            catch (Exception ex)
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
                Response r = new Response("error", ex.Message);
                response.Content = new StringContent(JsonConvert.SerializeObject(r), Encoding.UTF8, "application/json");
                return response;
            }
        }

        [Route("api/deletesong/{song_id}/")]
        [HttpDelete]
        public HttpResponseMessage Delete(int song_id)
        {
            try
            {
                using (SqlConnection sc = new SqlConnection(sConn))
                {
                    sc.Open();
                    string sql = "Delete from song_data where id = @song_id ";
                    SqlCommand com = new SqlCommand(sql, sc);
                    com.Parameters.AddWithValue("@song_id", song_id);
                    com.ExecuteNonQuery();
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                Response r = new Response("success", "Song deleted");
                response.Content = new StringContent(JsonConvert.SerializeObject(r), Encoding.UTF8, "application/json");
                return response;
            }
            catch
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
                Response r = new Response("error", "Error occured while deleting song, try again!");
                response.Content = new StringContent(JsonConvert.SerializeObject(r), Encoding.UTF8, "application/json");
                return response;
            }
        }
    }
}

using GoodVibesWeb.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;

namespace GoodVibesWeb.Controllers
{
    public class AuthController : ApiController
    {
        string sConn =
          ServerData.sconn;

        [Route("api/auth")]
        [HttpPut]
        public HttpResponseMessage Auth(User u)
        {
            try
            {
                string hashedpassword = sha256(u.password + ServerData.salt);
                string passwordfromdb;

                using (SqlConnection sc = new SqlConnection(sConn))
                {
                    sc.Open();
                    string sql = "SELECT password from users where username = @username";
                    SqlCommand com = new SqlCommand(sql, sc);
                    com.Parameters.AddWithValue("@username", u.username);
                    object o = com.ExecuteScalar();
                    if (o != null)
                        passwordfromdb = o.ToString();
                    else
                        passwordfromdb = "";
                }

                if (hashedpassword == passwordfromdb)
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StringContent(Base64Encode(u.username), Encoding.UTF8, "application/json");
                    return response;
                }
                else
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Forbidden);
                    response.Content = new StringContent("Invalid password or username, try again!", Encoding.UTF8, "application/json");
                    return response;
                }
            }
            catch
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
                response.Content = new StringContent("Error happened while logging in", Encoding.UTF8, "application/json");
                return response;
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        [Route("api/createuser")]
        [HttpPost]
        public HttpResponseMessage CreateUser(User u)
        {
            try
            {
                HttpResponseMessage response;

                using (SqlConnection sc = new SqlConnection(sConn))
                {
                    sc.Open();
                    string sql = "SELECT username from users where username = @username";
                    SqlCommand com = new SqlCommand(sql, sc);
                    com.Parameters.AddWithValue("@username", u.username);
                    object o = com.ExecuteScalar();
                    if (o == null)
                    {
                        sql = "select username from users where email = @email";
                        com = new SqlCommand(sql, sc);
                        com.Parameters.AddWithValue("@email", u.email);
                        object e = com.ExecuteScalar();
                        if (e == null)
                        {
                            sql = "insert into users(username, password, email) values(@username, @password, @email)";
                            com = new SqlCommand(sql, sc);
                            com.Parameters.AddWithValue("@username", u.username);
                            com.Parameters.AddWithValue("@password", sha256(u.password + "suola123666"));
                            com.Parameters.AddWithValue("@email", u.email);
                            com.ExecuteNonQuery();

                            sql = "insert into playlists(playlist_name, username, deletable) values (@playlist_name, @username, 0)";
                            com = new SqlCommand(sql, sc);
                            com.Parameters.AddWithValue("@playlist_name", "My Playlist");
                            com.Parameters.AddWithValue("@username", u.username);
                            com.ExecuteNonQuery();

                            response = Request.CreateResponse(HttpStatusCode.Created);
                            response.Content = new StringContent("Registration succesful. You can now log in and enjoy!", Encoding.UTF8, "text/plain");
                            return response;
                        }
                        else
                        {
                            response = Request.CreateResponse(HttpStatusCode.BadRequest);
                            response.Content = new StringContent("Email already in use!", Encoding.UTF8, "application/json");
                            return response;
                        }
                    }
                    else
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest);
                        response.Content = new StringContent("Username already in use!", Encoding.UTF8, "application/json");
                        return response;
                    }
                }
            }
            catch
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
                response.Content = new StringContent("Error happened while registering user, Try again!", Encoding.UTF8, "application/json");
                return response;
            }
        }
        public static string sha256(string password)
        {
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            System.Text.StringBuilder hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
    }

}

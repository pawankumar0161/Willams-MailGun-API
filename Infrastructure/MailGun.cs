using MailGun_API.DTO;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Net.Mail;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using System.Net;
using System.Configuration;

namespace MailGun_API.Infrastructure
{
    public class MailGun : IMailGun
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        public MailGun(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public bool SaveMailGunEmail(MailGunDTO mailGunDTO)
        {
            string insertSql = "INSERT INTO ProjectEmail(messageid, dateinserted, recipients, cc, bcc, sender, [from], subject, bodyhtml, bodyplain, strippedtext, strippedsignature, strippedhtml, attachments, messageurl, contentidmap, messageheaders) VALUES (@messageid, @dateinserted, @recipients, @cc, @bcc, @sender, @from, @subject, @bodyhtml, @bodyplain, @strippedtext, @strippedsignature, @strippedhtml, @attachments, @messageurl, @contentidmap, @messageheaders)";
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = _configuration["MailGun:ConnectionString"];
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(insertSql, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@messageid", !string.IsNullOrWhiteSpace(mailGunDTO.MessageId) ? mailGunDTO.MessageId : string.Empty);
                sqlCommand.Parameters.AddWithValue("@dateinserted", DateTime.UtcNow);
                sqlCommand.Parameters.AddWithValue("@recipients", !string.IsNullOrWhiteSpace(mailGunDTO.Recipient) ? mailGunDTO.Recipient : string.Empty);
                sqlCommand.Parameters.AddWithValue("@cc", !string.IsNullOrWhiteSpace(mailGunDTO.CC) ? mailGunDTO.CC : string.Empty);
                sqlCommand.Parameters.AddWithValue("@bcc", !string.IsNullOrWhiteSpace(mailGunDTO.BCC) ? mailGunDTO.BCC : string.Empty);
                sqlCommand.Parameters.AddWithValue("@sender", !string.IsNullOrWhiteSpace(mailGunDTO.Sender) ? mailGunDTO.Sender : string.Empty);
                sqlCommand.Parameters.AddWithValue("@from", !string.IsNullOrWhiteSpace(mailGunDTO.From) ? mailGunDTO.From : string.Empty);
                sqlCommand.Parameters.AddWithValue("@subject", !string.IsNullOrWhiteSpace(mailGunDTO.Subject) ? mailGunDTO.Subject : string.Empty);
                sqlCommand.Parameters.AddWithValue("@bodyhtml", !string.IsNullOrWhiteSpace(mailGunDTO.BodyHtml) ? mailGunDTO.BodyHtml : string.Empty);
                sqlCommand.Parameters.AddWithValue("@bodyplain", !string.IsNullOrWhiteSpace(mailGunDTO.BodyPlain) ? mailGunDTO.BodyPlain : string.Empty);
                sqlCommand.Parameters.AddWithValue("@strippedtext", !string.IsNullOrWhiteSpace(mailGunDTO.StrippedText) ? mailGunDTO.StrippedText : string.Empty);
                sqlCommand.Parameters.AddWithValue("@strippedsignature", !string.IsNullOrWhiteSpace(mailGunDTO.StrippedSignature) ? mailGunDTO.StrippedSignature : string.Empty);
                sqlCommand.Parameters.AddWithValue("@strippedhtml", !string.IsNullOrWhiteSpace(mailGunDTO.StrippedHtml) ? mailGunDTO.StrippedHtml : string.Empty);
                sqlCommand.Parameters.AddWithValue("@attachments", !string.IsNullOrWhiteSpace(mailGunDTO.Attachments) ? mailGunDTO.Attachments : string.Empty);
                sqlCommand.Parameters.AddWithValue("@messageurl", !string.IsNullOrWhiteSpace(mailGunDTO.MessageUrl) ? mailGunDTO.MessageUrl : string.Empty);
                sqlCommand.Parameters.AddWithValue("@contentidmap", !string.IsNullOrWhiteSpace(mailGunDTO.ContentIdMap) ? mailGunDTO.ContentIdMap : string.Empty);
                sqlCommand.Parameters.AddWithValue("@messageheaders", !string.IsNullOrWhiteSpace(mailGunDTO.MessageHeaders) ? mailGunDTO.MessageHeaders : string.Empty);

                int result = sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
                var test = File.ReadAllText(@"C:\Users\Bhagat\Desktop\json.txt");
                _ = SaveAttachments(test);
                return result > 0 ? true : false;
            }
        }

        public bool SaveAttachments(string jsonAttachments)
        {
            var attachmentResponse = new List<MailGunAttachment>();
           
            var memoryStreamAttachments = new MemoryStream(Encoding.Unicode.GetBytes(jsonAttachments));
            var serializerAttachments = JsonConvert.SerializeObject(jsonAttachments);
            var attachmentsList  = JsonConvert.DeserializeObject<List<MailGunAttachment>>(jsonAttachments);
            var path = Path.Combine(_environment.ContentRootPath, "/EmailAttachments");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            foreach (var attachment in attachmentsList)
            {
                var attachmentContent = GetAttachmentFromMailGun(attachment.Url);
                File.WriteAllBytes(path +"/"+ attachment.Name, Encoding.ASCII.GetBytes(attachmentContent));
            }
            return true;
        }

        private string GetAttachmentFromMailGun(string url)
        {
            try
            {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                var plainTextBytes = Encoding.UTF8.GetBytes("api:key-99pmrcd-qae62n1b5pdoz30bt7r5osl2");
                string val = Convert.ToBase64String(plainTextBytes);
                request.Headers.Add("Authorization", "Basic " + val);
                //request.Headers.Add("Authorization", "Basic YXBpOmtleS05OXBtcmNkLXFhZTYybjFiNXBkb3ozMGJ0N3I1b3NsMg==");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                var data = reader.ReadToEnd();
                //byte[] bytes = Encoding.ASCII.GetBytes(data);
                return data;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //private CredentialCache GetCredential(string url)
        //{
        //    CredentialCache credentialCache = new CredentialCache();
        //    credentialCache.Add(new Uri(url), "Basic", new NetworkCredential(ConfigurationManager.AppSettings["MailGun:MGPrivateKeyUserName"], ConfigurationManager.AppSettings["MailGun:PrivateKey"]));
        //    return credentialCache;
        //}
    }
}

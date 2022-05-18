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
            string insertSql = "INSERT INTO Emails(messageID, datesent, recipients, cc, bcc, sender, [from], subject, [body-plain], [stripped-text], [stripped-signature], [stripped-html], attachments, [message-url], [content-id-map], [message-headers]) VALUES (@messageid, @dateinserted, @recipients, @cc, @bcc, @sender, @from, @subject, @bodyplain, @strippedtext, @strippedsignature, @strippedhtml, @attachments, @messageurl, @contentidmap, @messageheaders)";
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
                //var test = File.ReadAllText(@"C:\Users\Bhagat\Desktop\json.txt");
                // _ = SaveAttachments(test);
                if (!string.IsNullOrEmpty(mailGunDTO.Attachments))
                {
                    _ = SaveAttachments(mailGunDTO.Attachments);
                }
               
                return result > 0 ? true : false;
            }
        }

        public bool SaveAttachments(string jsonAttachments)
        {
            var attachmentsList  = JsonConvert.DeserializeObject<List<MailGunAttachment>>(jsonAttachments);
            var path = Path.Combine(_environment.ContentRootPath, "/EmailAttachments/");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            foreach (var attachment in attachmentsList)
            {
                GetAttachmentFromMailGun(attachment.Url, attachment.Name, path);
            }
            return true;
        }

        private void GetAttachmentFromMailGun(string url, string name, string path)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                var plainTextBytes = Encoding.UTF8.GetBytes(_configuration["MailGun:PrivateKey"]);
                string val = Convert.ToBase64String(plainTextBytes);
                request.Headers.Add("Authorization", "Basic " + val);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                MemoryStream ms = new MemoryStream();
                var buffer = new Byte[4096];
                var blockSize = stream.Read(buffer,0,4096);
                if (blockSize > 0)
                {
                    ms.Write(buffer, 0, blockSize);
                }
                using (FileStream fs = new FileStream(path+name, FileMode.Create))
                {
                    fs.Write(ms.ToArray(),0,Convert.ToInt32(ms.Length));
                }

            }
            catch
            {
                throw;
            }
        }
    }
}

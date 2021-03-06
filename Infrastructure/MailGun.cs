using MailGun_API.DTO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Data;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net.Mail;

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
            //_ = SaveDataToResponseTable(mailGunDTO);
            //string input = mailGunDTO.MessageHeaders;
            //var keyValuePairs = input.Split(new string[] { "To" }, StringSplitOptions.None);
            //var select = keyValuePairs.Select(value => value.Split('='));
            //var key = keyValuePairs.ToDictionary(pair => pair[0], pair => pair[1]);
            //MailMessage message = new MailMessage();
            
            //Dictionary<string, string> keyValuePairs = input.Split(',')
            //.Select(value => value.Split('='))
            //.ToDictionary(pair => pair[0], pair => pair[1]);

            //string studentId = keyValuePairs["To"];

            bool isEmailExist = FindEmailByMessageId(mailGunDTO.MessageId);
            
            if (!isEmailExist)
            {
                string insertSql = "INSERT INTO Emails(messageID,body, datesent, recipients, cc, bcc, sender, [from], subject, [body-plain], [stripped-text], [stripped-signature], [stripped-html], attachments, [message-url], [content-id-map], [message-headers]) OUTPUT INSERTED.ID VALUES (@messageid, @bodyHtml, @dateinserted, @recipients, @cc, @bcc, @sender, @from, @subject, @bodyplain, @strippedtext, @strippedsignature, @strippedhtml, @attachments, @messageurl, @contentidmap, @messageheaders)";
                using (SqlConnection sqlConnection = new SqlConnection())
                {
                    string toHeader = string.Empty;//_httpContextAccessor.HttpContext.Request.Headers["To"];
                    
                    sqlConnection.ConnectionString = _configuration["MailGun:ConnectionString"];
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand(insertSql, sqlConnection);
                    sqlCommand.Parameters.AddWithValue("@messageid", !string.IsNullOrWhiteSpace(mailGunDTO.MessageId) ? mailGunDTO.MessageId : string.Empty);
                    sqlCommand.Parameters.AddWithValue("@bodyHtml", !string.IsNullOrWhiteSpace(mailGunDTO.BodyHtml) ? mailGunDTO.BodyHtml : string.Empty);
                    sqlCommand.Parameters.AddWithValue("@dateinserted", DateTime.UtcNow);
                    sqlCommand.Parameters.AddWithValue("@recipients", !string.IsNullOrWhiteSpace(mailGunDTO.To) ? mailGunDTO.To : string.Empty);
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
                    var outputIdParam = new SqlParameter();
                    outputIdParam = new SqlParameter("@id", SqlDbType.Int);
                    outputIdParam.Direction = ParameterDirection.Output;
                    sqlCommand.Parameters.Add(outputIdParam);

                    var result = sqlCommand.ExecuteScalar();
                    sqlConnection.Close();

                    var id = Convert.ToInt32(result);

                    //var test = File.ReadAllText(@"C:\Users\Bhagat\Desktop\json.txt");
                    // _ = SaveAttachments(test, Convert.ToInt32(id));
                    if (!string.IsNullOrEmpty(mailGunDTO.Attachments))
                    {
                        _ = SaveAttachments(mailGunDTO.Attachments, Convert.ToInt32(id));
                    }

                    return Convert.ToInt32(result) > 0;
                }
            }
            return false;

        }

        public bool SaveDataToResponseTable(MailGunDTO mailGunDTO)
        {
            string jsonString = JsonConvert.SerializeObject(mailGunDTO);
            string insertSql = "INSERT INTO MailgunResponse(Response) VALUES (@response)";
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = _configuration["MailGun:ConnectionString"];
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(insertSql, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@response", jsonString);

                var result = sqlCommand.ExecuteScalar();
                sqlConnection.Close();

                return Convert.ToInt32(result) > 0;
            }
        }

        public bool FindEmailByMessageId(string messageId)
        {
            //string jsonString = JsonConvert.SerializeObject(mailGunDTO);
            messageId = string.IsNullOrEmpty(messageId) ? string.Empty : messageId;
            string insertSql = "SELECT id FROM Emails WHERE messageID = @messageId";
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = _configuration["MailGun:ConnectionString"];
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(insertSql, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@messageId", messageId);

                var result = sqlCommand.ExecuteScalar();
                sqlConnection.Close();

                return Convert.ToInt32(result) > 0;
            }
        }

        public bool SaveAttachments(string jsonAttachments, int id)
        {
            var attachmentsList = JsonConvert.DeserializeObject<List<MailGunAttachment>>(jsonAttachments);
            var path = "D:/Sites/OFEC-Projects/MailgunAPI/EmailAttachments/" + id + "/";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            foreach (var attachment in attachmentsList)
            {
                GetAttachmentFromMailGun(attachment.Url, attachment.Name, path);
                InsertAttachment(id, attachment.Name, "EmailAttachments/" + id + "/" + attachment.Name);
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
                var buffer = new byte[16384];
                int blockSize;
                do
                {
                    blockSize = stream.Read(buffer, 0, 16384);
                    if (blockSize > 0)
                    {
                        ms.Write(buffer, 0, blockSize);
                    }
                } while (blockSize > 0);

                using (FileStream fs = new FileStream(path + name, FileMode.Create))
                {
                    fs.Write(ms.ToArray(), 0, Convert.ToInt32(ms.Length));
                }

            }
            catch
            {
                throw;
            }
        }

        private void InsertAttachment(int InboxID, string FileName, string FilePath)
        {
            string insertSql = "INSERT INTO InboxAttachments(InboxID, FileName, FilePath) VALUES (@InboxID, @FileName, @FilePath)";
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = _configuration["MailGun:ConnectionString"];
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(insertSql, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@InboxID", InboxID);
                sqlCommand.Parameters.AddWithValue("@FileName", !string.IsNullOrWhiteSpace(FileName) ? FileName : string.Empty);
                sqlCommand.Parameters.AddWithValue("@FilePath", !string.IsNullOrWhiteSpace(FilePath) ? FilePath : string.Empty);
                sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
            }
        }
    }
}

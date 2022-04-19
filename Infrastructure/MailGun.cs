using MailGun_API.DTO;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;

namespace MailGun_API.Infrastructure
{
    public class MailGun : IMailGun
    {
        private readonly IConfiguration _configuration;
        public MailGun(IConfiguration configuration) 
        {
            _configuration = configuration;
        }
        public bool SaveMailGunEmail(MailGunDTO mailGunDTO)
        {
            string insertSql = "INSERT INTO Mail(Sender, Recipient, MailSubject, BodyHtml, BodyPlain, MailDate) VALUES (@Sender,@Recipient,@MailSubject,@BodyHtml,@BodyPlain,'{0:g}')";
            DateTime timestamp = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "US/Pacific");
            string insertQuery = String.Format(insertSql, timestamp);
            MySqlConnection sqlConnection = new MySqlConnection(_configuration["MailGun:ConnectionString"]);
            sqlConnection.Open();
            MySqlCommand sqlCommand = new MySqlCommand(insertQuery, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@Sender", mailGunDTO.Sender);
            sqlCommand.Parameters.AddWithValue("@Recipient", mailGunDTO.Recipient);
            sqlCommand.Parameters.AddWithValue("@MailSubject", mailGunDTO.Subject);
            sqlCommand.Parameters.AddWithValue("@BodyHtml", mailGunDTO.BodyHtml);
            sqlCommand.Parameters.AddWithValue("@BodyPlain", mailGunDTO.BodyPlain);
            int result = sqlCommand.ExecuteNonQuery();
            sqlConnection.Close();
            return result > 0 ? true : false;
        }
    }
}

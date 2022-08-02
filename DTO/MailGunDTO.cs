using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailGun_API.DTO
{
    public class MailGunDTO
    {
        public string Recipient { get; set; }
        public string Sender { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        [BindProperty(Name = "body-plain")]
        public string BodyPlain { get; set; }
        [BindProperty(Name = "body-html")]
        public string BodyHtml { get; set; }
        [BindProperty(Name = "attachments")]
        public string Attachments { get; set; }
        [BindProperty(Name = "Message-ID")]
        public string MessageId { get; set; }
        [BindProperty(Name = "cc")]
        public string CC { get; set; }
        [BindProperty(Name = "bcc")]
        public string BCC { get; set; }
        [BindProperty(Name = "stripped-text")]
        public string StrippedText { get; set; } 
        [BindProperty(Name = "stripped-signature")]
        public string StrippedSignature { get; set; }
        [BindProperty(Name = "stripped-html")]
        public string StrippedHtml { get; set; }
        [BindProperty(Name = "message-url")]
        public string MessageUrl { get; set; }
        [BindProperty(Name = "content-id-map")]
        public string ContentIdMap { get; set; }
        [BindProperty(Name = "message-headers")]
        public string MessageHeaders { get; set; }
        public string To { get; set; }
    }
}

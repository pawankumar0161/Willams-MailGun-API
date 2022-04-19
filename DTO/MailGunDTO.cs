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
    }
}

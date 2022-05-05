using Microsoft.AspNetCore.Mvc;

namespace MailGun_API.DTO
{
    public class MailGunAttachment
    {
        [BindProperty(Name = "name")]
        public string Name { get; set; }
        [BindProperty(Name = "content-type")]
        public string ContentType { get; set; }
        [BindProperty(Name = "size")]
        public long Size { get; set; }
        [BindProperty(Name = "url")]
        public string Url { get; set; }
    }
}

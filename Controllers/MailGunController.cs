using MailGun_API.DTO;
using MailGun_API.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailGun_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailGunController : ControllerBase
    {
        private readonly IMailGun _mailGun;
        public MailGunController(IMailGun mailGun) { _mailGun = mailGun; }
        [HttpPost]
        public IActionResult SaveMailGunEmail([FromForm] MailGunDTO mailGunDTO) 
        {
            bool isEmailSavedToDB = _mailGun.SaveMailGunEmail(mailGunDTO);
            return isEmailSavedToDB ? CreatedAtAction(nameof(SaveMailGunEmail), mailGunDTO) : UnprocessableEntity(mailGunDTO);
        }
    }
}

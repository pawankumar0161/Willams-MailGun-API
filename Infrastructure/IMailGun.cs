using MailGun_API.DTO;

namespace MailGun_API.Infrastructure
{
    public interface IMailGun
    {
        bool SaveMailGunEmail(MailGunDTO mailGunDTO);
        bool SaveAttachments(string jsonAttachments, int id);
    }
}

using BookWeb.Helper;
using BookWeb.Services.IServices;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BookWeb.Services;

public class EmailServices : IEmailServices
{
    private readonly ILogger<EmailServices> _logger;
    private readonly MailSettings _mailSettings;

    public EmailServices(ILogger<EmailServices> logger, IOptions<MailSettings> mailSettings)
    {
        _logger = logger;
        _logger.LogInformation("Create MailService");
        _mailSettings = mailSettings.Value;
    }

    public async Task Send(string to, string subject, string html)
    {
        var email = new MimeMessage();
        email.Sender = new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail);
        email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        var builder = new BodyBuilder();
        builder.HtmlBody = html;
        email.Body = builder.ToMessageBody();
        
        // dùng SmtpClient của MailKit
        // using gửi xong xóa để k làm chậm hệ thống
        using var smtp = new MailKit.Net.Smtp.SmtpClient();

        try
        {
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(email);
        }
        catch (Exception ex)
        {
            // Gửi mail thất bại, nội dung email sẽ lưu vào thư mục mailssave
            System.IO.Directory.CreateDirectory("mailssave");
            var emailsavefile = string.Format(@"mailssave/{0}.eml", Guid.NewGuid());
            await email.WriteToAsync(emailsavefile);
            
            _logger.LogInformation("Lỗi gửi mail, lưu tại - " + emailsavefile);
            _logger.LogError(ex.Message);
        }
        
        smtp.Disconnect(true);
        
        _logger.LogInformation("Send mail to " + to);
    }
}

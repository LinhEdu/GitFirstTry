using BookWeb.Services.IServices;
using MailKit.Security;
using MimeKit;

namespace AppdevBookShop.Services;

public class EmailServices : IEmailServices
{
    private readonly ILogger<EmailServices> _logger;

    public EmailServices(ILogger<EmailServices> logger)
    {
        _logger = logger;
        _logger.LogInformation("Create MailService");
    }

    public async Task Send(string to, string subject, string html)
    {
        var DisplayName = "BookWeb";
        var Mail = "";
        var Password = "";
        var Host = "smtp.gmail.com";
        var Port = 587;

        var email = new MimeMessage();
        email.Sender = new MailboxAddress(DisplayName, Mail);
        email.From.Add(new MailboxAddress(DisplayName, Mail));
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
            smtp.Connect(Host, Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(Mail, Password);
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
namespace BookWeb.Services.IServices;

public interface IEmailServices
{
    Task Send(string to, string subject, string html);
}
using System;
using System.Threading.Tasks;
using HorseRacing.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace HorseRacing.Infrastructure.ExternalServices;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var mailSettings = _configuration.GetSection("MailSettings");
        var host = mailSettings["Host"] ?? "smtp.gmail.com";
        var port = int.Parse(mailSettings["Port"] ?? "587");
        var fromMail = mailSettings["Mail"] ?? "horseracing.noreply@gmail.com";
        var password = mailSettings["Password"] ?? string.Empty;
        var displayName = mailSettings["DisplayName"] ?? "Horse Racing Admin";

        Console.WriteLine($"[DEBUG SMTP] Email: {fromMail}");
        Console.WriteLine($"[DEBUG SMTP] Password Length: {password.Length}");
        Console.WriteLine($"[DEBUG SMTP] Password Ends With: {(password.Length >= 3 ? password.Substring(password.Length - 3) : "Too short")}");

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(displayName, fromMail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        try
        {
            await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(fromMail, password);
            await smtp.SendAsync(email);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Không thể gửi email qua SMTP: {ex.Message}", ex);
        }
        finally
        {
            await smtp.DisconnectAsync(true);
        }
    }
}

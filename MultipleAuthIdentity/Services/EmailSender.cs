using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MultipleAuthIdentity.Services;
using SendGrid.Helpers.Mail;
using SendGrid;
using MimeKit;
using System.Net.Mail;
using NuGet.Packaging;
using System.Linq;

namespace MultipleAuthIdentity.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger _logger;

    public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
                       ILogger<EmailSender> logger)
    {
        Options = optionsAccessor.Value;
        _logger = logger;
    }

    public AuthMessageSenderOptions Options { get; } //Set with Secret Manager.

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("Licenta", "marianuta112@gmail.com"));

        IEnumerable<InternetAddress> address = new[] { MailboxAddress.Parse(toEmail) };

        emailMessage.To.AddRange(address);
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message };

        using var client = new MailKit.Net.Smtp.SmtpClient();
        
            client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.Authenticate("marianuta112@gmail.com", "zsyewjodzrntnmeq");

            client.Send(emailMessage);
            client.Disconnect(true);
        
        //if (string.IsNullOrEmpty(Options.SendGridKey))
        //{
        //    throw new Exception("Null SendGridKey");
        //}
        //await Execute(Options.SendGridKey, subject, message, toEmail);
    }

    public async Task Execute(string apiKey, string subject, string message, string toEmail)
    {
        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress("marianuta112@gmail.com", "Password Recovery"),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));

        // Disable click tracking.
        // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
        msg.SetClickTracking(false, false);
        var response = await client.SendEmailAsync(msg);
        _logger.LogInformation(response.IsSuccessStatusCode
                               ? $"Email to {toEmail} queued successfully!"
                               : $"Failure Email to {toEmail}");
    }
}
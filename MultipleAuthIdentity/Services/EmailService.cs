﻿using MimeKit;
using MultipleAuthIdentity.Models;
using System.Net.Mail;
using MailKit.Net.Smtp;

namespace MultipleAuthIdentity.Services
{
   
        public class EmailService : IEmailService
        {
            private readonly EmailConfiguration _emailConfig;
            public EmailService(EmailConfiguration emailConfig) => _emailConfig = emailConfig;
            public void SendEmail(Message message)
            {
                var emailMessage = CreateEmailMessage(message);
                Send(emailMessage);
            }

            private MimeMessage CreateEmailMessage(Message message)
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("email", _emailConfig.From));
                emailMessage.To.AddRange(message.To);
                emailMessage.Subject = message.Subject;
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };

                return emailMessage;
            }

            private void Send(MimeMessage mailMessage)
            {
                using var client = new MailKit.Net.Smtp.SmtpClient();
                try
                {
                    client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(_emailConfig.UserName, _emailConfig.Password);

                    client.Send(mailMessage);
                    client.Disconnect(true);
                }
                catch
                {
                    //log an error message or throw an exception or both.
                    throw;
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        
    }
}

using InstruLearn_Application.BLL.Service.IService;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderPassword = _configuration["EmailSettings:SenderPassword"];
            var senderName = _configuration["EmailSettings:SenderName"];

            using var message = new MailMessage();
            message.From = new MailAddress(senderEmail, senderName);
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHtml;

            using var client = new SmtpClient(smtpServer, smtpPort);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(senderEmail, senderPassword);

            await client.SendMailAsync(message);
        }

        public async Task SendVerificationEmailAsync(string email, string username, string token)
        {
            // Use the token provided by AuthService - don't generate a new one
            var subject = "Verify Your InstruLearn Account";
            var body = $@"
              <html>
                 <body>
                   <h2>Welcome to InstruLearn!</h2>
                   <p>Hello {username},</p>
                   <p>Thank you for registering with InstruLearn. Your email verification code is:</p>
                   <h3 style=""font-size: 24px; padding: 10px; background-color: #f0f0f0; text-align: center; letter-spacing: 5px;"">{token}</h3>
                   <p>This code will expire in 24 hours.</p>
                   <p>If you didn't create an account, please ignore this email.</p>
                   <p>Best regards,</p>
                   <p>The InstruLearn Team</p>
                 </body>
               </html>";

            await SendEmailAsync(email, subject, body);
        }
    }
}
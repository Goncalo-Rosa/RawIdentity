using MailKit.Net.Smtp;
using MimeKit;

namespace RawIdentity
{
    public class EmailComposer
    {
        public string emailAddressSender { get; set; }
        public string emailAddressRecipient { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
        public string smtpServiceAddress { get; set; }
        public int port { get; set; }
        public string password { get; set; }

        public void SendEmail()
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(emailAddressSender, emailAddressSender));
            message.To.Add(new MailboxAddress(emailAddressRecipient, emailAddressRecipient));
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                client.Connect(smtpServiceAddress, port, false);
                client.Authenticate(emailAddressSender, password);
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}
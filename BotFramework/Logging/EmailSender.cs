using System.Diagnostics;
using System.Net.Mail;
using System.Text.Json;

public class EmailSender
{
    SmtpClient smtpClient;
    string _sender;
    string _password;
    string _recipient;
    public EmailSender()
    {
        string jsonString = File.ReadAllText("Logging/LoggingConfig.json");
        JsonDocument info = JsonDocument.Parse(jsonString);
        _sender = info.RootElement.GetProperty("sender").GetString();
        _password = info.RootElement.GetProperty("password").GetString();
        _recipient = info.RootElement.GetProperty("recipient").GetString();

        System.Net.NetworkCredential credentials = new System.Net.NetworkCredential(
            _sender, _password);
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

        smtpClient = new SmtpClient()
        {
            Port = 587,
            Host = "smtp.gmail.com",
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = credentials,
            EnableSsl = true
        };
    }

    public void SendEmail(string subject, string message, string recipient)
    {
        MailMessage email = new MailMessage(_sender, recipient, subject, message);
        try
        {
            Console.WriteLine("try send email");
            smtpClient.Send(email);
            Console.WriteLine("Email sent!");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        
    }

    public void SendEmail(string subject, string message, string[] recipients)
    {
        foreach (string recipient in recipients)
        {
            SendEmail(subject, message, recipient);
        }
    }

    public void SendEmailAttachment(string subject, string message, Attachment attachment, string recipient)
    {
        MailMessage email = new MailMessage(_sender, recipient, subject, message);
        email.Attachments.Add(attachment);
        smtpClient.Send(email);
    }
}


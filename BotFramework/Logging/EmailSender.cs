
using System.Net.Mail;
using System.Text.Json;

public class EmailSender
{
    private SmtpClient smtpClient;
    private string _sender;
    private string _password;

    public EmailSender()
    {
        string jsonString = File.ReadAllText("Logging/LoggingConfig.json");
        JsonDocument info = JsonDocument.Parse(jsonString);
        _sender = info.RootElement.GetProperty("sender").GetString();
        _password = info.RootElement.GetProperty("password").GetString();

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
            Log.Debug("try send email");
            smtpClient.Send(email);
            Log.Debug("Email sent!");
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
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
        try
        {
            Log.Debug("try send email");
            smtpClient.Send(email);
            Log.Debug("Email sent!");
        }
        catch (Exception ex)
        {
            Log.Error(ex.ToString());
        }
    }

    public void SendEmailAttachment(string subject, string message, Attachment attachment, string[] recipients)
    {
        foreach (string recipient in recipients)
        {
            SendEmailAttachment(subject, message, attachment, recipient);
        }
    }
}


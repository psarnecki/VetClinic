using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace VetClinicManager.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger _logger;

    public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
        ILogger<EmailSender> logger)
    {
        Options = optionsAccessor.Value;
        _logger = logger;
    }

    public AuthMessageSenderOptions Options { get; } //Set with Secret Manager

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrEmpty(Options.SendGridKey))
        {
            throw new Exception("Null SendGridKey");
        }
        await Execute(Options.SendGridKey, subject, message, toEmail);
    }

    public async Task Execute(string apiKey, string subject, string message, string toEmail)
    {
        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(
                Options.SenderEmail ?? "noreply@vetclinic.com",
                Options.SenderName ?? "VetClinic"),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));
        
        msg.SetClickTracking(false, false);
        var response = await client.SendEmailAsync(msg);
        
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Email to {toEmail} queued successfully!", toEmail);
        }
        else
        {
            var errorBody = await response.Body.ReadAsStringAsync();
            _logger.LogError("Failure sending email to {toEmail}. Status: {Status}. Error: {Error}", 
                toEmail, response.StatusCode, errorBody);
        }
    }
}
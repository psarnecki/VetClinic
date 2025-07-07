﻿using Microsoft.AspNetCore.Identity.UI.Services;

namespace VetClinicManager.Services;

public class DummyEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return Task.CompletedTask;
    }
}
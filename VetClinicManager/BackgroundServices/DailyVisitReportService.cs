using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using VetClinicManager.Models;
using VetClinicManager.Services;
using VetClinicManager.Services.Reports;

namespace VetClinicManager.BackgroundServices;

public class DailyVisitReportService : BackgroundService
{
    private readonly ILogger<DailyVisitReportService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly TimeOnly _sendTime = new TimeOnly(8, 0);

    public DailyVisitReportService(ILogger<DailyVisitReportService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyVisitReportService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNextRun();
            _logger.LogInformation("DailyVisitReportService: next report scheduled in {Delay}.", delay);

            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
                await SendDailyReportAsync();
        }

        _logger.LogInformation("DailyVisitReportService stopped.");
    }

    private TimeSpan CalculateDelayUntilNextRun()
    {
        var now = DateTime.Now;
        var nextRun = now.Date.Add(_sendTime.ToTimeSpan());

        if (now.TimeOfDay >= _sendTime.ToTimeSpan())
            nextRun = nextRun.AddDays(1);

        return nextRun - now;
    }

    private async Task SendDailyReportAsync()
    {
        _logger.LogInformation("DailyVisitReportService: generating daily open visits reports for {Date}.", DateTime.UtcNow.Date);

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var visitService = scope.ServiceProvider.GetRequiredService<IVisitService>();
            var emailSender = (EmailSender)scope.ServiceProvider.GetRequiredService<IEmailSender>();
            var reportGenerator = scope.ServiceProvider.GetRequiredService<VisitReportGenerator>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            var allOpenVisits = await visitService.GetOpenVisitsForReportAsync();

            if (!allOpenVisits.Any())
            {
                _logger.LogInformation("DailyVisitReportService: no open visits found. Skipping reports.");
                return;
            }

            var globalRecipients = new HashSet<string>();
            foreach (var role in new[] { "Admin", "Receptionist" })
            {
                var usersInRole = await userManager.GetUsersInRoleAsync(role);
                foreach (var user in usersInRole)
                {
                    if (!string.IsNullOrEmpty(user.Email))
                        globalRecipients.Add(user.Email);
                }
            }

            if (globalRecipients.Any())
            {
                var globalPdfData = reportGenerator.GenerateOpenVisitsReport(allOpenVisits);

                var globalReportName = $"global-visits-report-{DateTime.Now:yyyy-MM-dd}.pdf";
                var globalSubject = $"Daily Global Report - {DateTime.Now:yyyy-MM-dd} ({allOpenVisits.Count} visits)";

                foreach (var email in globalRecipients)
                {
                    await emailSender.SendEmailWithAttachmentAsync(email, globalSubject,
                        "Attached is the comprehensive list of all open visits.", globalPdfData, globalReportName);
                }
                _logger.LogInformation("Sent global report to {Count} admins/receptionists.", globalRecipients.Count);
            }

            var vets = await userManager.GetUsersInRoleAsync("Vet");
            int vetsEmailed = 0;

            foreach (var vet in vets)
            {
                if (string.IsNullOrEmpty(vet.Email)) continue;
                
                var vetVisits = allOpenVisits.Where(v => v.AssignedVetId == vet.Id).ToList();

                if (vetVisits.Any())
                {
                    var vetPdfData = reportGenerator.GenerateOpenVisitsReport(vetVisits);

                    var vetReportName = $"my-visits-{DateTime.Now:yyyy-MM-dd}.pdf";
                    var vetSubject = $"Your Daily Visits - {DateTime.Now:yyyy-MM-dd} ({vetVisits.Count} visits)";

                    await emailSender.SendEmailWithAttachmentAsync(vet.Email, vetSubject,
                        "Here is the list of your scheduled visits for today.", vetPdfData, vetReportName);
                    
                    vetsEmailed++;
                }
            }

            _logger.LogInformation("Sent individual reports to {Count} vets.", vetsEmailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DailyVisitReportService: failed to generate or send daily reports.");
        }
    }
}
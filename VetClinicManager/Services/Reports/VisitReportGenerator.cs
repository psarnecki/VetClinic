using System.ComponentModel.DataAnnotations;
using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VetClinicManager.Models;
using VetClinicManager.Models.Enums;

namespace VetClinicManager.Services.Reports;

public class VisitReportGenerator
{
    public byte[] GenerateOpenVisitsReport(IEnumerable<Visit> openVisits)
    {
        var document = new OpenVisitsDocument(openVisits);
        return document.GeneratePdf();
    }
}

public class OpenVisitsDocument : IDocument
{
    private readonly IEnumerable<Visit> _visits;
    private readonly DateTime _generatedAt;

    public OpenVisitsDocument(IEnumerable<Visit> visits)
    {
        _visits = visits;
        _generatedAt = DateTime.Now;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(40);
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().AlignCenter().Text(x =>
            {
                x.CurrentPageNumber();
                x.Span(" / ");
                x.TotalPages();
            });
        });
    }

    void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Daily Operational Report").SemiBold().FontSize(24);
                column.Item().Text($"For Date: {_generatedAt:dd.MM.yyyy}").FontSize(12).FontColor(Colors.Grey.Darken2);
                column.Item().Text($"Generated: {_generatedAt:HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
            });
            
            row.ConstantItem(100).AlignRight().Column(col => 
            {
                col.Item().Text("VetClinic").FontSize(16).SemiBold();
                col.Item().Text("Management").FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Item().Row(row =>
            {
                var total = _visits.Count();
                var urgent = _visits.Count(v => v.Priority == VisitPriority.Critical || v.Priority == VisitPriority.Urgent);
                var scheduled = _visits.Count(v => v.Status == VisitStatus.Scheduled);
                var inProgress = _visits.Count(v => v.Status == VisitStatus.InProgress);

                row.RelativeItem().Component(new StatCard("Total Visits", total.ToString(), "#212529"));
                row.ConstantItem(10);
                row.RelativeItem().Component(new StatCard("Urgent/Critical", urgent.ToString(), "#dc3545"));
                row.ConstantItem(10);
                row.RelativeItem().Component(new StatCard("Scheduled", scheduled.ToString(), "#0d6efd"));
                row.ConstantItem(10);
                row.RelativeItem().Component(new StatCard("In Progress", inProgress.ToString(), "#ffc107"));
            });

            column.Item().PaddingTop(25).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Detailed Schedule").FontSize(16).SemiBold();

            if (!_visits.Any())
            {
                column.Item().PaddingTop(20).Text("No open visits for the selected period.").Italic();
            }
            else
            {
                column.Item().PaddingTop(10).Element(ComposeTable);
            }
        });
    }

    void ComposeTable(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(45);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                static IContainer HeaderStyle(IContainer c) =>
                    c.Background("#212529").Padding(5).DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold().FontSize(9));

                header.Cell().Element(HeaderStyle).Text("Time");
                header.Cell().Element(HeaderStyle).Text("Patient (Owner)");
                header.Cell().Element(HeaderStyle).Text("Veterinarian");
                header.Cell().Element(HeaderStyle).Text("Visit Title");
                header.Cell().Element(HeaderStyle).Text("Priority");
                header.Cell().Element(HeaderStyle).Text("Status");
            });

            foreach (var visit in _visits.OrderBy(v => v.ScheduledAt))
            {
                table.Cell().Element(CellStyle).Text($"{visit.ScheduledAt:HH:mm}");

                table.Cell().Element(CellStyle).Column(col =>
                {
                    col.Item().Text(visit.Animal?.Name ?? "N/A").SemiBold();
                    col.Item().Text($"{visit.Animal?.Owner?.FirstName} {visit.Animal?.Owner?.LastName}").FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                table.Cell().Element(CellStyle).Text(visit.AssignedVet?.LastName ?? "Unassigned");
                table.Cell().Element(CellStyle).Text(visit.Title ?? "-").FontSize(9);

                var pColor = visit.Priority == VisitPriority.Critical ? "#dc3545" 
                    : (visit.Priority == VisitPriority.Urgent ? "#fd7e14" 
                    : "#0d6efd");
                
                table.Cell().Element(CellStyle).Text(GetEnumDisplayName(visit.Priority)).FontColor(pColor).SemiBold();
                table.Cell().Element(CellStyle).Text(GetEnumDisplayName(visit.Status)).FontSize(8);
            }
        });
    }

    static IContainer CellStyle(IContainer container)
    {
        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignMiddle();
    }

    private static string GetEnumDisplayName(Enum enumValue)
    {
        return enumValue.GetType()
            .GetMember(enumValue.ToString())
            .FirstOrDefault()
            ?.GetCustomAttribute<DisplayAttribute>()
            ?.GetName() ?? enumValue.ToString();
    }

    private class StatCard : IComponent
    {
        private string Title { get; }
        private string Value { get; }
        private string Color { get; }

        public StatCard(string title, string value, string color)
        {
            Title = title;
            Value = value;
            Color = color;
        }

        public void Compose(IContainer container)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten5)
                .Padding(5)
                .Column(col =>
                {
                    col.Item().AlignCenter().Text(Title).FontSize(8).FontColor(Colors.Grey.Darken1);
                    col.Item().AlignCenter().Text(Value).FontSize(14).SemiBold().FontColor(Color);
                });
        }
    }
}
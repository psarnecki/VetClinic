using System.ComponentModel.DataAnnotations;
using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VetClinicManager.DTOs.Visits;

namespace VetClinicManager.Services.Reports;

public class VisitDetailsReport : IDocument
{
    private readonly VisitDetailsVetRecDto _model;
    private readonly bool _isStaffView;

    public VisitDetailsReport(VisitDetailsVetRecDto model, bool isStaffView)
    {
        _model = model;
        _isStaffView = isStaffView;
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
                column.Item().Text(_model.Title).FontSize(24).SemiBold().FontColor("#212529");
                column.Item().Text($"Visit Report #{_model.Id} - {_model.Animal.Name}").FontSize(10).FontColor(Colors.Grey.Medium);
            });
            row.ConstantItem(100).AlignRight().Text("VetClinic").FontSize(16).SemiBold().FontColor(Colors.Grey.Darken2);
        });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor("#0d6efd").Column(col => 
                {
                    col.Item().Background("#0d6efd").Padding(5).Text("Visit Details").FontColor(Colors.White).SemiBold();
                    
                    col.Item().Padding(10).Column(details => 
                    {
                        void RowItem(string label, string val) => 
                            details.Item().PaddingBottom(4).Text(t => { t.Span($"{label}: ").SemiBold(); t.Span(val); });

                        details.Item().PaddingBottom(4).Row(r => {
                            r.AutoItem().Text("Status: ").SemiBold();
                            r.AutoItem().PaddingHorizontal(5).Background("#0d6efd").PaddingHorizontal(4).Text(GetDisplayName(_model.Status)).FontColor(Colors.White).FontSize(9);
                        });
                        
                        if (_isStaffView)
                        {
                            string priorityColor = (_model.Priority.ToString() == "Critical" || _model.Priority.ToString() == "Urgent") ? "#dc3545" : "#ffc107";
                            string priorityTextColor = (_model.Priority.ToString() == "Critical" || _model.Priority.ToString() == "Urgent") ? "#ffffff" : "#000000";

                            details.Item().PaddingBottom(4).Row(r => {
                                r.AutoItem().Text("Priority: ").SemiBold();
                                r.AutoItem().PaddingHorizontal(5).Background(priorityColor).PaddingHorizontal(4).Text(GetDisplayName(_model.Priority)).FontColor(priorityTextColor).FontSize(9);
                            });
                        }

                        RowItem("Date", $"{_model.CreatedDate:dd.MM.yyyy HH:mm}");
                        RowItem("Animal", $"{_model.Animal.Name} ({_model.Animal.Species})");
                        
                        if (_model.Owner != null)
                            RowItem("Owner", $"{_model.Owner.FirstName} {_model.Owner.LastName}");
                            
                        if (_model.AssignedVet != null)
                            RowItem("Vet", $"{_model.AssignedVet.FirstName} {_model.AssignedVet.LastName}");
                    });
                });

                row.ConstantItem(20);

                row.RelativeItem().Border(1).BorderColor("#6c757d").Column(col => 
                {
                    col.Item().Background("#6c757d").Padding(5).Text("Description").FontColor(Colors.White).SemiBold();
                    col.Item().Padding(10).Text(_model.Description ?? "No description.");
                });
            });

            column.Item().PaddingTop(30).Text("Visit History & Prescriptions").FontSize(18).SemiBold();
            column.Item().PaddingTop(10).Element(ComposeTable);
        });
    }

    void ComposeTable(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(70);
                if (_isStaffView) columns.RelativeColumn(2);
                columns.RelativeColumn(3);
                columns.RelativeColumn(5);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderStyle).Text("Date");
                if (_isStaffView) header.Cell().Element(HeaderStyle).Text("Updated by");
                header.Cell().Element(HeaderStyle).Text("Notes");
                header.Cell().Element(HeaderStyle).Text("Prescriptions (Medication & Dosage)");
                
                static IContainer HeaderStyle(IContainer container) => 
                    container.Background("#212529").Padding(6).DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold());
            });

            foreach (var update in _model.Updates.OrderByDescending(u => u.UpdateDate))
            {
                table.Cell().Element(CellStyle).Text($"{update.UpdateDate:dd.MM.yyyy}");
                
                if (_isStaffView) 
                    table.Cell().Element(CellStyle).Text(update.UpdatedByVetName);
                
                table.Cell().Element(CellStyle).Text(update.Notes ?? "-");

                table.Cell().Element(CellStyle).Column(c => {
                    if (update.Prescriptions?.Any() == true)
                    {
                        foreach(var p in update.Prescriptions) 
                        {
                            c.Item().PaddingBottom(4).Column(item => 
                            {
                                item.Item().Text(p.MedicationName).SemiBold().FontColor(Colors.Blue.Medium);
                                item.Item().Text(p.Dosage).FontSize(9).FontColor(Colors.Grey.Darken2);
                            });
                        }
                    }
                    else 
                    {
                        c.Item().Text("-").FontColor(Colors.Grey.Medium);
                    }
                });

                static IContainer CellStyle(IContainer container) => 
                    container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6);
            }
        });
    }

    private string GetDisplayName(Enum enumValue)
    {
        return enumValue.GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()?
            .GetName() ?? enumValue.ToString();
    }
}
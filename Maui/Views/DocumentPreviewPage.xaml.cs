using System.Net.Http;
using Construction.Maui.Models;

namespace Construction.Maui.Views;

/// <summary>
/// Document preview page code-behind. Loads the same sample PDF used by the
/// React/Blazor ports into the Syncfusion SfPdfViewer; the data set has no real
/// per-file content, so every preview opens the sample (mirrors React's
/// SAMPLE_PDF_URL approach). The header title + subtitle come from the selected
/// RecentDocumentDto to match the React modal.
/// </summary>
public partial class DocumentPreviewPage : ContentPage
{
    public DocumentPreviewPage(RecentDocumentDto doc)
    {
        InitializeComponent();
        DocTitle.Text = doc.Title;
        DocSubtitle.Text = $"{doc.Type} · Rev {doc.Revision ?? "—"} · Submitted {doc.SubmittedDate:MMM d, yyyy}";

        // Load the sample PDF asynchronously; suppress errors (non-fatal).
        _ = LoadPdfAsync();
    }

    private async Task LoadPdfAsync()
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            var bytes = await http.GetByteArrayAsync(MauiProgram.SamplePdfUrl);
            PdfViewer.DocumentSource = new MemoryStream(bytes);
        }
        catch
        {
            PdfViewer.IsVisible = false;
            ErrorLabel.IsVisible = true;
        }
    }

    private void OnGoBack(object? sender, EventArgs e) => Navigation.PopAsync();
}

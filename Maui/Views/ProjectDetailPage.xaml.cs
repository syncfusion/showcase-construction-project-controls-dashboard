using Construction.Maui.Models;
using Construction.Maui.ViewModels;

namespace Construction.Maui.Views;

/// <summary>
/// Project Detail page code-behind — pushed onto the NavigationPage stack.
/// The 5 tabs (Overview · Schedule · Cost · RFIs · Submittals) live inside a
/// Syncfusion SfTabView declared in XAML; the back button and the Document
/// Workflow row-tap (opens the sample PDF viewer) are the only interactions
/// handled here. Everything else binds to <see cref="ProjectDetailViewModel"/>.
/// </summary>
public partial class ProjectDetailPage : ContentPage
{
    private readonly ProjectDetailViewModel _vm;

    public ProjectDetailPage(ProjectDetailViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    /// <summary>Back to the Projects list (pops this detail page off the stack).</summary>
    private void OnGoBack(object? sender, EventArgs e) => Navigation.PopAsync();

    /// <summary>Document Workflow row-tap — matches the React ProjectDetail page,
    /// where selecting a document row opens a PDF preview modal. Here we push a
    /// dedicated <see cref="DocumentPreviewPage"/> (a SfPdfViewer over the sample
    /// PDF) since MAUI has no global modal; the dev can replace the sample URL
    /// with a real per-document stream later. Reselection is cleared so the same
    /// row can be tapped again.</summary>
    private async void OnDocumentSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count == 0) return;
        if (e.CurrentSelection[0] is not RecentDocumentDto doc) return;

        // Clear the selection so the same row can be tapped again later.
        if (sender is CollectionView cv) cv.SelectedItem = null;

        // The sample data set has no real per-file content, so every preview opens
        // the same sample PDF (mirrors React's SAMPLE_PDF_URL approach).
        await Navigation.PushAsync(new DocumentPreviewPage(doc));
    }
}

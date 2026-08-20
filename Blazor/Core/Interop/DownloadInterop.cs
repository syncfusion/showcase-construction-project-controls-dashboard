using Microsoft.JSInterop;

namespace Construction.Blazor.Core.Interop;

public class DownloadInterop(IJSRuntime js)
{
    public ValueTask DownloadTextFileAsync(string filename, string contentType, string content) =>
        js.InvokeVoidAsync("downloadInterop.downloadFile", filename, contentType, content);
}

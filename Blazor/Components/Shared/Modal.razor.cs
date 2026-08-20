using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Construction.Blazor.Components.Shared;

public partial class Modal : ComponentBase
{
    [Parameter] public bool Open { get; set; }
    [Parameter, EditorRequired] public string Title { get; set; } = "";
    [Parameter] public string? Subtitle { get; set; }
    [Parameter] public string Size { get; set; } = "md";
    [Parameter] public EventCallback OnClosed { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? FooterContent { get; set; }

    [Inject] private IJSRuntime Js { get; set; } = default!;

    private ElementReference _panelRef;
    private bool _wasOpen;

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            await OnClosed.InvokeAsync();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Open && !_wasOpen)
        {
            _wasOpen = true;
            await Js.InvokeVoidAsync("modalInterop.lockScroll");
            await _panelRef.FocusAsync();
        }
        else if (!Open && _wasOpen)
        {
            _wasOpen = false;
            await Js.InvokeVoidAsync("modalInterop.unlockScroll");
        }
    }
}

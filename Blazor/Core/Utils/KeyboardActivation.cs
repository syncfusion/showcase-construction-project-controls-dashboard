using Microsoft.AspNetCore.Components.Web;

namespace Construction.Blazor.Core.Utils;

/// <summary>Keyboard equivalent for elements using role="button" (clickable table rows/cards)
/// so Enter/Space activate them the same way a click would — mirrors onActivateKey in the
/// React/Angular ports.</summary>
public static class KeyboardActivation
{
    public static void OnActivateKey(KeyboardEventArgs e, Action handler)
    {
        if (e.Key is "Enter" or " ")
        {
            handler();
        }
    }
}

namespace Construction.Blazor.Components.Layout;

public record NavigationItem(string Id, string Label, string Path, string Icon, string Priority);

public static class NavigationItems
{
    public static readonly IReadOnlyList<NavigationItem> Primary =
    [
        new("dashboard", "Dashboard", "/", "layout-dashboard", "P0"),
        new("projects", "Projects", "/projects", "briefcase", "P0"),
        new("cost-control", "Cost Control", "/cost-control", "wallet", "P0"),
        new("risks", "Risks & Issues", "/risks", "alert-triangle", "P0"),
    ];

    public static readonly IReadOnlyList<NavigationItem> Secondary =
    [
        new("site-map", "Site Map", "/site-map", "map", "P1"),
        new("documents", "Documents", "/documents", "file-text", "P1"),
        new("contractors", "Contractors", "/contractors", "users", "P2"),
        new("workflows", "Workflows", "/workflows", "git-branch", "P1"),
        new("reports", "Reports", "/reports", "bar-chart-3", "P2"),
    ];
}

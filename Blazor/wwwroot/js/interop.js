// Small interop surface for the handful of things Blazor Server can't do from C# directly:
// reading/writing localStorage (theme persistence) and triggering a client-side file download
// (CSV export). Mirrors the equivalent helpers in the React/Angular ports.

window.themeInterop = {
  getStoredTheme: function () {
    return localStorage.getItem('theme');
  },
  setStoredTheme: function (theme) {
    localStorage.setItem('theme', theme);
  },
  prefersDark: function () {
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
  },
  // data-theme lives on the .app-shell div (inside <body>), which Blazor can reach
  // reactively. <html>/<body> themselves are outside the interactive component tree
  // (rendered once by App.razor), so anything that inherits color/background straight
  // from them — e.g. a bare <h1> with no explicit color — never sees theme changes
  // unless <html> also carries the attribute. Set it imperatively here to close that gap.
  setHtmlTheme: function (theme) {
    document.documentElement.setAttribute('data-theme', theme);
  },
  // Syncfusion's theme (Grid, Chart, Schedule, Maps, Diagram, PdfViewer chrome) ships as a
  // static stylesheet that has no idea about our data-theme attribute or CSS variables, so
  // light/dark switching for Syncfusion widgets means swapping the stylesheet file itself.
  setSyncfusionTheme: function (isDark) {
    const link = document.getElementById('syncfusion-theme');
    if (!link) return;
    const file = isDark ? 'material-dark.css' : 'material.css';
    if (!link.href.endsWith(file)) {
      link.href = '_content/Syncfusion.Blazor.Themes/' + file;
    }
  },
};

window.modalInterop = {
  lockScroll: function () {
    document.body.style.overflow = 'hidden';
  },
  unlockScroll: function () {
    document.body.style.overflow = '';
  },
};

window.tokenInterop = {
  // Read the resolved value of a CSS custom property from :root. Syncfusion
  // SVG ignores var(--foo) references inside chart text styles, tooltip
  // fills, and palette entries, so Blazor has to resolve them to literal
  // hex/rgb strings before binding them to Syncfusion component props.
  getCssVar: function (name) {
    const value = getComputedStyle(document.documentElement).getPropertyValue(name);
    return value ? value.trim() : null;
  },
};

window.downloadInterop = {
  downloadFile: function (filename, contentType, content) {
    const blob = new Blob([content], { type: contentType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  },
};

import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { routes } from './app.routes';

// NOTE: NO Syncfusion theme CSS is imported. Every Syncfusion component
// (Grid, Chart, HeatMap, Diagram, Maps, Schedule, PDFViewer) is styled
// entirely from the in-house token CSS at src/styles/tokens.css — see
// .e-grid, .e-heatmap, etc. in src/styles/global.css. This is the same
// zero-theme strategy the React app uses.
//
// provideAnimationsAsync() is required by Syncfusion Grid's paging/sort/
// filter/animation runtime (BrowserAnimationsModule under the hood).
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(),
    provideAnimationsAsync(),
  ],
};

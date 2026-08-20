import { bootstrapApplication } from '@angular/platform-browser';
import { registerLicense } from '@syncfusion/ej2-base';
import { appConfig } from './app/app.config';
import { App } from './app/app';
import { environment } from './environments/environment';

if (environment.syncfusionLicenseKey) {
  registerLicense(environment.syncfusionLicenseKey);
}

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));

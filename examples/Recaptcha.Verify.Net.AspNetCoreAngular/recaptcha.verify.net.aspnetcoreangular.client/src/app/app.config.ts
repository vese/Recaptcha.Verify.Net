import { provideHttpClient } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { RECAPTCHA_V3_SITE_KEY } from 'ng-recaptcha-2';
import { environment } from '../environments/environment';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    { provide: RECAPTCHA_V3_SITE_KEY, useFactory: () => environment.recaptchaKey }
  ]
};

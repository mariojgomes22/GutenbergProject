import { ApplicationConfig, importProvidersFrom, provideZoneChangeDetection } from '@angular/core';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideTranslateService } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';
import { HTTP_INTERCEPTORS } from '@angular/common/http';

import { MsalModule, MsalInterceptor } from '@azure/msal-angular';
import { PublicClientApplication, InteractionType, BrowserCacheLocation } from '@azure/msal-browser';

import { routes } from './app.routes';
import { environment } from '../environments/environment';

const msalInstance = new PublicClientApplication({
    auth: {
        clientId: environment.msalConfig.clientId,
        authority: `https://login.microsoftonline.com/${environment.msalConfig.tenantId}`,
        redirectUri: environment.msalConfig.redirectUri
    },
    cache: {
        cacheLocation: BrowserCacheLocation.LocalStorage
    }
});

export const appConfig: ApplicationConfig = {
    providers: [
        provideZoneChangeDetection({ eventCoalescing: true }),
        provideRouter(routes),
        provideHttpClient(withInterceptorsFromDi()),
        provideAnimations(),
        provideTranslateService({
            defaultLanguage: 'en',
            loader: provideTranslateHttpLoader({
                prefix: '/assets/i18n/',
                suffix: '.json'
            })
        }),
        importProvidersFrom(
            MsalModule.forRoot(
                msalInstance,
                {
                    interactionType: InteractionType.Redirect,
                    authRequest: { scopes: ['user.read'] }
                },
                {
                    interactionType: InteractionType.Redirect,
                    protectedResourceMap: new Map([
                        [`${environment.apiUrl}/*`, [environment.apiScope]]
                    ])
                }
            )
        ),
        {
            provide: HTTP_INTERCEPTORS,
            useClass: MsalInterceptor,
            multi: true
        }
    ]
};

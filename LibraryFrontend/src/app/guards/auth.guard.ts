import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { MsalService, MsalBroadcastService } from '@azure/msal-angular';
import { InteractionStatus } from '@azure/msal-browser';
import { filter, map, take } from 'rxjs/operators';

export const authGuard: CanActivateFn = () => {
    const msalService = inject(MsalService);
    const msalBroadcastService = inject(MsalBroadcastService);
    const router = inject(Router);

    return msalBroadcastService.inProgress$.pipe(
        filter(status => status === InteractionStatus.None),
        take(1),
        map(() => {
            const accounts = msalService.instance.getAllAccounts();
            return accounts.length > 0 ? true : router.createUrlTree(['/login']);
        })
    );
};

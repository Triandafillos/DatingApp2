import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

export const adminGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const toast = inject(ToastrService);

  if (accountService.roles().includes('Admin') || accountService.roles().includes('Moderator')) {
    return true;
  }
  else {
    toast.error("You can't enter this area");
    return false;
  }
};

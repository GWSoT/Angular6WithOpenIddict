import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AccountRoutingModule } from './account-routing.module';
import { LoginComponent } from './login/login.component';
import { OidcSignInComponent } from '../oidc-sign-in/oidc-sign-in.component';
import { LogoutComponent } from '../logout/logout.component';

@NgModule({
  imports: [
    CommonModule,
    AccountRoutingModule
  ],
  declarations: [LoginComponent, OidcSignInComponent, LogoutComponent]
})
export class AccountModule { }

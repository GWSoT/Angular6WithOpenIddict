import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { OidcSignInComponent } from '../oidc-sign-in/oidc-sign-in.component';
import { LogoutComponent } from '../logout/logout.component';

const routes: Routes = [
  { path: 'account/login', component: LoginComponent },
  { path: 'account/logout', component: LogoutComponent },
  { path: 'account/sign-in', component: OidcSignInComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AccountRoutingModule { }

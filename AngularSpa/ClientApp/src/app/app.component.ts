import { Component } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'ClientApp';
  isAuthenticated = false;

  constructor(_securityService: OidcSecurityService) {
    _securityService.getIsAuthorized().subscribe(result => {
      this.isAuthenticated = result
      console.log(result)
    })
    console.log(_securityService.getState())
    console.log(_securityService.getToken())
  }
}

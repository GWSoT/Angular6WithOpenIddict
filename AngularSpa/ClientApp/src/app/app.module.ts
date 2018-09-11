import { BrowserModule } from '@angular/platform-browser';
import { NgModule, APP_INITIALIZER } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HttpClientModule } from '@angular/common/http';

import {
  AuthModule,
  OidcSecurityService,
  OpenIDImplicitFlowConfiguration,
  OidcConfigService,
  AuthWellKnownEndpoints
} from 'angular-auth-oidc-client';
import { AccountModule } from './account/account.module';


export function loadConfig(oidcConfigService: OidcConfigService) {
  console.log('APP_INITIALIZER STARTING');
  return () => oidcConfigService.load_using_stsServer('http://localhost:12345');
}

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AccountModule,
    AppRoutingModule,
    HttpClientModule,
    AuthModule.forRoot()
  ],
  providers: [
    OidcConfigService,
    {
        provide: APP_INITIALIZER,
        useFactory: loadConfig,
        deps: [OidcConfigService],
        multi: true
    },
  ],
  bootstrap: [AppComponent]
})

export class AppModule {
  constructor(
      private oidcSecurityService: OidcSecurityService,
      private oidcConfigService: OidcConfigService,
  ) {
      this.oidcConfigService.onConfigurationLoaded.subscribe(() => {
      const openIDImplicitFlowConfiguration = new OpenIDImplicitFlowConfiguration();

      openIDImplicitFlowConfiguration.stsServer = 'http://localhost:12345';
      openIDImplicitFlowConfiguration.redirect_url = 'http://localhost:9000';
      // The Client MUST validate that the aud (audience) Claim contains its client_id value registered at the Issuer identified by the iss (issuer) Claim as an audience.
      // The ID Token MUST be rejected if the ID Token does not list the Client as a valid audience, or if it contains additional audiences not trusted by the Client.
      openIDImplicitFlowConfiguration.client_id = 'angular6';
      openIDImplicitFlowConfiguration.response_type = 'id_token token';
      openIDImplicitFlowConfiguration.scope = 'openid api1';
      openIDImplicitFlowConfiguration.post_logout_redirect_uri = 'http://localhost:9000/sign-out';
      openIDImplicitFlowConfiguration.start_checksession = false;
      openIDImplicitFlowConfiguration.silent_renew = true;
      openIDImplicitFlowConfiguration.post_login_route = '';
      // HTTP 403
      openIDImplicitFlowConfiguration.forbidden_route = '/Forbidden';
      // HTTP 401
      openIDImplicitFlowConfiguration.unauthorized_route = '/Unauthorized';
      openIDImplicitFlowConfiguration.log_console_warning_active = true;
      openIDImplicitFlowConfiguration.log_console_debug_active = false;
      // id_token C8: The iat Claim can be used to reject tokens that were issued too far away from the current time,
      // limiting the amount of time that nonces need to be stored to prevent attacks.The acceptable range is Client specific.
      openIDImplicitFlowConfiguration.max_id_token_iat_offset_allowed_in_seconds = 10;

      const authWellKnownEndpoints = new AuthWellKnownEndpoints();
      authWellKnownEndpoints.setWellKnownEndpoints(this.oidcConfigService.wellKnownEndpoints);

      this.oidcSecurityService.setupModule(openIDImplicitFlowConfiguration, authWellKnownEndpoints);

      });

      console.log('APP STARTING');
  }
}

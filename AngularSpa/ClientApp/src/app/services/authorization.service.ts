import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthorizationService {


  isAuthenticated: boolean;
  authObservbale: BehaviorSubject<boolean>;
  
  constructor() { 
    this.isAuthenticated = false;
    this.authObservbale.next(this.isAuthenticated);
  }

  public setAuthStatus() {
    
  }

}

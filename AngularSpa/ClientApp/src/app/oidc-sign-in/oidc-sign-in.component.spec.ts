import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OidcSignInComponent } from './oidc-sign-in.component';

describe('OidcSignInComponent', () => {
  let component: OidcSignInComponent;
  let fixture: ComponentFixture<OidcSignInComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ OidcSignInComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OidcSignInComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

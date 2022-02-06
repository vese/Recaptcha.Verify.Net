import { Component, OnDestroy } from '@angular/core';
import { ReCaptchaV3Service } from 'ng-recaptcha';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Credentials } from '../models/credentials';
import { LoginService } from '../services/login.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnDestroy {
  public success: boolean = null;

  private ngUnsubscribe: Subject<void> = new Subject<void>();

  constructor(
    private reCaptchaV3Service: ReCaptchaV3Service,
    private loginService: LoginService
  ) { }

  ngOnDestroy(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }

  login_InBody(): void {
    this.login((c) => this.loginService.login_InBody(c));
  }

  login_RecaptchaAttribute_InHeader(): void {
    this.login((c) => this.loginService.login_RecaptchaAttribute_InHeader(c));
  }

  login_RecaptchaAttribute_InQuery(): void {
    this.login((c) => this.loginService.login_RecaptchaAttribute_InQuery(c));
  }

  login_RecaptchaAttribute_InForm(): void {
    this.login((c) => this.loginService.login_RecaptchaAttribute_InForm(c));
  }

  private login(loginFunc: (credentials: Credentials) => Observable<any>): void {
    this.success = null;
    this.reCaptchaV3Service.execute('login')
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe(
        token => {
          const credentials = new Credentials('', '', token);

          loginFunc(credentials)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
              result => {
                this.success = true;
              },
              error => {
                this.success = false;
              },
            );
        });
  }
}

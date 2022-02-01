import { Component, OnDestroy } from '@angular/core';
import { ReCaptchaV3Service } from 'ng-recaptcha';
import { Subject } from 'rxjs';
import { LoginService } from '../services/login.service';
import { takeUntil } from 'rxjs/operators';
import { Credentials } from '../models/credentials';

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

  login(): void {
    this.success = null;
    this.reCaptchaV3Service.execute('login')
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe(
        token => {
          const credentials = new Credentials('', '', token);

          this.loginService.login(credentials)
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

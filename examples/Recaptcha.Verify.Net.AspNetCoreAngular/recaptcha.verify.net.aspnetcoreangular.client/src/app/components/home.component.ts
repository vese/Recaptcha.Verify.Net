import { ChangeDetectionStrategy, Component, inject, OnDestroy, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RecaptchaV3Module, ReCaptchaV3Service } from 'ng-recaptcha-2';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Credentials } from '../models/credentials';
import { LoginService } from '../services/login.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RecaptchaV3Module],
  providers: [LoginService],
  templateUrl: './home.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeComponent implements OnDestroy {
  public readonly success = signal<boolean | null>(null);

  private readonly loginService = inject(LoginService);

  private readonly reCaptchaV3Service = inject(ReCaptchaV3Service);

  private readonly ngUnsubscribe: Subject<void> = new Subject<void>();

  private readonly grecaptcha = toSignal(this.reCaptchaV3Service.recaptchaLoader.ready);

  ngOnDestroy(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }

  login_InBody(): void {
    this.login((c) => this.loginService.login_InBody(c));
  }

  login_InHeader(): void {
    this.login((c) => this.loginService.login_InHeader(c));
  }

  login_InQuery(): void {
    this.login((c) => this.loginService.login_InQuery(c));
  }

  login_InForm(): void {
    this.login((c) => this.loginService.login_InForm(c));
  }

  private login(loginFunc: (credentials: Credentials) => Observable<any>): void {
    this.grecaptcha()?.reset(environment.recaptchaKey as any);

    this.success.set(null);
    this.reCaptchaV3Service.execute('login')
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe(
        token => {
          const credentials = new Credentials('login', 'password', token);

          loginFunc(credentials)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe({
              next: result => {
                this.success.set(true);
              },
              error: error => {
                this.success.set(false);
              },
            });
        });
  }
}

import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Credentials } from "../models/credentials";

@Injectable({
  providedIn: 'root'
})
export class LoginService {
  private http = inject(HttpClient);

  public login_InBody(credentials: Credentials): Observable<any> {
    return this.http.post('/api/login/loginInBody', credentials);
  }

  public login_InHeader(credentials: Credentials): Observable<any> {
    const formData = this.getFormData(credentials);

    return this.http.post('/api/login/Login', formData, {
      headers: {
        'X-Recaptcha-Token': credentials.recaptchaToken ?? ''
      }
    });
  }

  public login_InQuery(credentials: Credentials): Observable<any> {
    const formData = this.getFormData(credentials);

    return this.http.post('/api/login/Login', formData, {
      params: {
        recaptchaTokenInQuery: credentials.recaptchaToken ?? ''
      }
    });
  }

  public login_InForm(credentials: Credentials): Observable<any> {
    const formData = this.getFormData(credentials);
    formData.append("recaptchaTokenInForm", credentials.recaptchaToken ?? '');

    return this.http.post('/api/login/Login', formData);
  }

  private getFormData(credentials: Credentials) {
    const formData = new FormData();
    formData.append("login", credentials.login ?? '');
    formData.append("password", credentials.password ?? '');
    formData.append("recaptchaToken", credentials.recaptchaToken ?? '');
    return formData;
  }
}

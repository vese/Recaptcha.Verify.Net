import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Credentials } from "../models/credentials";

@Injectable({
  providedIn: 'root'
})
export class LoginService {
  constructor(private http: HttpClient) { }

  public login_InBody(credentials: Credentials): Observable<any> {
    return this.http.post('/api/login', credentials);
  }

  public login_RecaptchaAttribute_InHeader(credentials: Credentials): Observable<any> {
    return this.http.post('/api/login/Login_RecaptchaAttribute', credentials, {
      headers: {
        'recaptchaTokenInHeader': credentials.recaptchaToken
      }
    });
  }

  public login_RecaptchaAttribute_InQuery(credentials: Credentials): Observable<any> {
    return this.http.post('/api/login/Login_RecaptchaAttribute', credentials, {
      params: {
        'recaptchaTokenInQuery': credentials.recaptchaToken
      }
    });
  }

  public login_RecaptchaAttribute_InForm(credentials: Credentials): Observable<any> {
    var formData: any = new FormData();
    formData.append("login", credentials.login);
    formData.append("password", credentials.password);
    formData.append("recaptchaToken", credentials.recaptchaToken);
    formData.append("recaptchaTokenInForm", credentials.recaptchaToken);

    return this.http.post('/api/login/Login_RecaptchaAttribute', formData);
  }
}

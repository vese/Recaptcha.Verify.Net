import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { Credentials } from "../models/credentials";

@Injectable({
  providedIn: 'root'
})
export class LoginService {
  constructor(private http: HttpClient) { }

  public login(credentials: Credentials): Observable<any> {
    return this.http.post('/api/login', credentials)
  }
}

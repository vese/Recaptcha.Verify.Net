import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { RecaptchaV3Module, RECAPTCHA_V3_SITE_KEY } from 'ng-recaptcha';
import { environment } from '../environments/environment';
import { AppComponent } from './app.component';
import { HomeComponent } from './home/home.component';
import { LoginService } from './services/login.service';


@NgModule({
  declarations: [
    AppComponent,
    HomeComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' }
    ]),
    RecaptchaV3Module
  ],
  providers: [
    { provide: RECAPTCHA_V3_SITE_KEY, useValue: environment.recaptchaKey },
    LoginService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

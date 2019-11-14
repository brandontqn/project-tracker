import { Injectable } from '@angular/core';
import { AppConfigService } from './app-config.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class RegisterService {

  constructor(private http: HttpClient) {
    console.log( 'Hello from RegisterService!' );
  }

  protected currentEndpoint = AppConfigService.settings.projectManagementServiceEnv.iis.registration;

  sendActivationEmail(email: string) {
    // const httpOptions = await this.getHeaders();
    const httpOptions = { 
      headers: new HttpHeaders({
        'Content-type': 'application/json'
      })
    };
    // const data = new Email(email);

    // console.log("email: " + email);
    // console.log("data.email: " + data.value);
    // console.log("endpoint: " + this.currentEndpoint);
    // console.log("httpOptions: " + httpOptions);

    return this.http.post<Email>(this.currentEndpoint, { value: email }, { headers: { 'Content-type': 'application/json' } });
  }

  validateToken(token: string) {
    // const url = "http://localhost:44364/api/tokens/ + token";
    const url = "https://localhost:44321/api/registration/validate/" + token;
    
    return this.http.post(url, "")
  }

  createOktaAccountWithCredentials(firstName: string, lastName: string, email: string, login: string, password: string) {
    // call the C# api endpoint for account creation ... or call directly the okta api?
    const url = "https://localhost:44321/api/registration/create";

    var account = new Account(firstName, lastName, email, login, password);

    return this.http.post(url, account)
  }
}

class Email {
  value: string;

  constructor(email: string) {
    this.value = email;
  }
}

class Account {
  firstName: string;
  lastName: string;
  email: string;
  login: string;
  password: string;

  constructor(fName: string, lName: string, email: string, login: string, pw: string) {
    this.firstName = fName;
    this.lastName = lName;
    this.email = email;
    this.login = login;
    this.password = pw;
  }
}
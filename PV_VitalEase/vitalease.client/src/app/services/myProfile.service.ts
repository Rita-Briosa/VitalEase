import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MyProfileService {

  // O endereço da sua API que retorna os logs
  private apiUrl = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api'; // Endereço do seu backend ASP.NET Core

  private apiUrlChangeBirthDate = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/changeBirthDate';
  private apiUrlChangeWeight = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/changeWeight';
  private apiUrlChangeHeight = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/changeHeight';
  private apiUrlChangeGender = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/changeGender';
  private apiUrlChangeHasHeartProblems = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/changeHasHeartProblems';
  private apiUrlChangePassword = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/changePassword';
  private apiUrlChangeUsername = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/changeUsername';
  private apiUrlChangeEmail = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/changeEmail';

  private apiUrlDeleteAcc = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/deleteAccount';
  private apiUrlValidatePassword = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/validatePassword';

  private apiUrlDeleteAccountResquest = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/deleteAccountRequest';
  private apiUrlDeleteAccountCancellation = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/deleteAccountCancellation';
  private apiUrlValidateDeleteAccountToken = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/ValidateDeleteAccountToken';
  constructor(private http: HttpClient) { }

  getProfileInfo(email: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/getProfileInfo/${email}`);
  }
  ////////////////////////

  validatePassword(email: string, password: string): Observable<any> {
    return this.http.post<any>(this.apiUrlValidatePassword, { email, password });
  }

  deleteAccountRequest(email: string): Observable<any> {
    return this.http.post<any>(this.apiUrlDeleteAccountResquest, {email});
  }

  deleteAccountCancellation(token: string): Observable<any> {
    return this.http.post<any>(this.apiUrlDeleteAccountCancellation, { token });
  }

  deleteUserAcc(email: string, token: string): Observable<any> {
    return this.http.delete<any>(this.apiUrlDeleteAcc, {
      body: { email, token }
    });
  }
  ///////////////////////////////

  validateToken(token: string): Observable<any> {

    const url = `${this.apiUrlValidateDeleteAccountToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }


  changeBirthDate(birthDate: string, email: string): Observable<any> {
    return this.http.post<any>(this.apiUrlChangeBirthDate, { birthDate, email });
  }

  changeUsername(username: string, email: string): Observable<any> {
    return this.http.post<any>(this.apiUrlChangeUsername, { username, email });
  }

  changeWeight(weight: number, email: string): Observable<any> {
    return this.http.post<any>(this.apiUrlChangeWeight, { weight, email });
  }

  changeHeight(height: number, email: string): Observable<any> {
    return this.http.post<any>(this.apiUrlChangeHeight, { height, email });
  }

  changeGender(gender: string, email: string): Observable<any> {
    return this.http.post<any>(this.apiUrlChangeGender, { gender, email });
  }

  changeHasHeartProblems(hasHeartProblems: boolean, email: string): Observable<any> {
    return this.http.post<any>(this.apiUrlChangeHasHeartProblems, { hasHeartProblems , email });
  }

  changePassword(oldPassword: string, newPassword: string, email: string): Observable<any> {
    return this.http.post<any>(this.apiUrlChangePassword, { oldPassword,newPassword, email });
  }

  changeEmail(password: string, email: string, newEmail: string): Observable<any> {
    return this.http.post<any>(this.apiUrlChangeEmail, { password, email, newEmail });
  }

}

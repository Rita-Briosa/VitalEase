import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MyProfileService {

  // O endereço da sua API que retorna os logs
  private apiUrl = 'https://localhost:7180/api'; // Endereço do seu backend ASP.NET Core

  private apiUrlChangeBirthDate = 'https://localhost:7180/api/changeBirthDate';
  private apiUrlChangeWeight = 'https://localhost:7180/api/changeWeight';
  private apiUrlDeleteAcc = 'https://localhost:7180/api/deleteAccount';
  private apiUrlChangeHeight = 'https://localhost:7180/api/changeHeight';
  private apiUrlChangeGender = 'https://localhost:7180/api/changeGender';
  private apiUrlChangeHasHeartProblems = 'https://localhost:7180/api/changeHasHeartProblems';
  private apiUrlChangePassword = 'https://localhost:7180/api/changePassword';
  private apiUrlChangeUsername = 'https://localhost:7180/api/changeUsername';
  private apiUrlChangeEmail = 'https://localhost:7180/api/changeEmail';

  constructor(private http: HttpClient) { }

  getProfileInfo(email: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/getProfileInfo/${email}`);
  }

  deleteUserAcc(email: string): Observable<any> {
    return this.http.delete(`${this.apiUrlDeleteAcc}/${(email)}`);
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

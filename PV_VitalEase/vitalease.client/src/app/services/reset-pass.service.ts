import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class ResetService {

  private apiUrlResetPassword = 'https://localhost:7180/resetPassword';

  constructor(private http: HttpClient) { }

  resetPassword(token: string | null, newPassword: string): Observable<any> {

    const body = { token, newPassword };


    return this.http.post<any>(this.apiUrlResetPassword, body);
  }

}

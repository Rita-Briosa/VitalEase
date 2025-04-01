import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class ResetService {

  private apiUrlResetPassword = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/resetPassword';

  private apiUrlValidateToken = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/validateTokenAtAccess';

  constructor(private http: HttpClient) { }

  resetPassword(token: string | null, newPassword: string): Observable<any> {
    const body = { token, newPassword };


    return this.http.post<any>(this.apiUrlResetPassword, body);
  }

  validateToken(token: string): Observable<any> {

    const url = `${this.apiUrlValidateToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }

}

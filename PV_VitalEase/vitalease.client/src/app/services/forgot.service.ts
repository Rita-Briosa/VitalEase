import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ForgotService {

  private apiUrl = 'https://localhost:7180/forgotPassword'; // Endereço atualizado do backend

  private apiUrlResetPassword = 'https://localhost:7180/resetPassword'; 
  constructor(private http: HttpClient) { }

  ForgotPassword(email: string): Observable<any> {
    return this.http.post<any>(this.apiUrl, { email }); // Garantindo que o email seja enviado como um objeto
  }

  resetPassword(email: string | null, newPassword: string): Observable<any> {
    const body = { email, newPassword };
    return this.http.post<any>(this.apiUrlResetPassword, body);
  }
}

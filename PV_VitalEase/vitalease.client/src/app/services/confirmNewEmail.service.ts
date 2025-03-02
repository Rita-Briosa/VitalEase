import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class ConfirmNewEmailService {
  private apiUrlConfirmNewEmailToken = 'https://localhost:7180/api/ConfirmNewEmailChange';

  private apiUrlValidateNewEmailToken = 'https://localhost:7180/api/ValidateNewEmailToken';

  private apiUrlCancelNewEmailToken = 'https://localhost:7180/api/CancelNewEmailChange';

  constructor(private http: HttpClient) { }

  validateToken(token: string): Observable<any> {

    const url = `${this.apiUrlValidateNewEmailToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }

  confirmNewEmailChange(token: string): Observable<any> {
    const url = `${this.apiUrlConfirmNewEmailToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }

  cancelNewEmailChange(token: string): Observable<any> {
    const url = `${this.apiUrlCancelNewEmailToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }
}

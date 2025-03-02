import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class ConfirmOldEmailService {
  private apiUrlConfirmOldEmailToken = 'https://localhost:7180/api/ConfirmOldEmailToken';

  private apiUrlValidateOldEmailToken = 'https://localhost:7180/api/ValidateOldEmailToken';

  private apiUrlCancelOldEmailToken = 'https://localhost:7180/api/CancelOldEmailChange';


  constructor(private http: HttpClient) { }

  validateToken(token: string): Observable<any> {

    const url = `${this.apiUrlValidateOldEmailToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }

  confirmOldEmailChange(token: string): Observable<any> {
    const url = `${this.apiUrlConfirmOldEmailToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }

  cancelOldEmailChange(token: string): Observable<any> {
    const url = `${this.apiUrlCancelOldEmailToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }
}

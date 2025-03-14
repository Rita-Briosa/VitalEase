import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class VerifyEmailService {
  private apiUrlValidateVerifyEmailToken = 'https://localhost:7180/api/ValidateVerifyEmailToken';

  constructor(private http: HttpClient) { }

  validateToken(token: string): Observable<any> {

    const url = `${this.apiUrlValidateVerifyEmailToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }
}

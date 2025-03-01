import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class ConfirmNewEmailService {
  private apiUrlValidateConfirmNewEmailToken = 'https://localhost:7180/api/ConfirmNewEmailToken';

  constructor(private http: HttpClient) { }

  validateToken(token: string): Observable<any> {

    const url = `${this.apiUrlValidateConfirmNewEmailToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }
}

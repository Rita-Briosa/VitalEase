import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ForgotService {

  private apiUrl = 'https://vitalease2025.3utilities.com/api/forgotPassword'; // Endereço atualizado do backend
 
  constructor(private http: HttpClient) { }

  ForgotPassword(email: string): Observable<any> {
  
    return this.http.post<any>(this.apiUrl, { email }); // Garantindo que o email seja enviado como um objeto
  }
}

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RegisterService {

  private apiUrl = 'https://localhost:7180/api/register'; // Endereço do seu backend

  constructor(private http: HttpClient) { }

  // Método de login para autenticar o usuário
  register(username: string, birthDate: Date, email: string,height: number, weight:number, gender: string, password: string, heartProblems:boolean): Observable<any> {

    return this.http.post(this.apiUrl, { username, birthDate, email, height, weight, gender, password, heartProblems });
  }
}

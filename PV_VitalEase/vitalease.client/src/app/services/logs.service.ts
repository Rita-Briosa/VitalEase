import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LogsService {

  // O endereço da sua API que retorna os logs
  private apiUrl = 'https://localhost:7180/getLogs'; // Endereço do seu backend ASP.NET Core

  constructor(private http: HttpClient) { }

  // Método para pegar os logs
  getLogs(): Observable<any> {
    return this.http.get(this.apiUrl); // Envia uma requisição GET para pegar os logs
  }
}

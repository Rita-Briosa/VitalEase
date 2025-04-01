import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LogsService {

  // O endereço da sua API que retorna os logs
  private apiUrl = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api'; // Endereço do seu backend ASP.NET Core

  constructor(private http: HttpClient) { }

  // Método para pegar os logs
  getLogs(): Observable<any> {
    return this.http.get(`${this.apiUrl}/getLogs`); // Envia uma requisição GET para pegar os logs
  }

  getLogsFilter(filters: any): Observable<any> {
    let params = new HttpParams();

    Object.keys(filters).forEach(key => {
      if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
        params = params.set(key, filters[key])
      }
    });

    return this.http.get(`${this.apiUrl}/getLogsFilter`, {params});
  }
}

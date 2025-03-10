import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ExercisesService {

  // O endereço da sua API que retorna os logs
  private apiUrl = 'https://localhost:7180/api'; // Endereço do seu backend ASP.NET Core

  constructor(private http: HttpClient) { }

  // Método para pegar os logs
  getExercises(): Observable<any> {
    return this.http.get(`${this.apiUrl}/getExercises`); // Envia uma requisição GET para pegar os logs
  }

}

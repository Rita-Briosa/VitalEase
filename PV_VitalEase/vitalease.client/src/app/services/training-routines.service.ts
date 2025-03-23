import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TrainingRoutinesService {

 
  private apiUrl = 'https://localhost:7180/api';

  private apiUrlAddRoutine = 'https://localhost:7180/api/addNewRoutine';

  constructor(private http: HttpClient) { }

  getRoutines(userId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/getRoutines`); 
  }

    getFilteredRoutines(filters: any): Observable < any > {
      let params = new HttpParams();

      Object.keys(filters).forEach(key => {
        if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
          params = params.set(key, filters[key])
        }
      });

      return this.http.get(`${this.apiUrl}/getFilteredRoutines`, { params });
  }

  // Método para pegar os logs
  getExercisesToAddToRoutine(): Observable<any> {
    return this.http.get(`${this.apiUrl}/getExercises`); // Envia uma requisição GET para pegar os logs
  }

  getExercises(routineId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/getExercisesFromRoutine/${routineId}`); 
  }

  getExercise(exerciseId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/getExerciseDetailsFromRoutine/${exerciseId}`);
  }

  getMedia(exerciseId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/getExerciseMediaFromRoutine/${exerciseId}`);
  }

  addRoutine(newName: string, newDescription: string, newType: string, newRoutineLevel: string, newNeeds: string, exercises: number[]): Observable<any> {

    return this.http.post<any>(this.apiUrlAddRoutine, { newName, newDescription, newType, newRoutineLevel, newNeeds, exercises });
    
    }

  }


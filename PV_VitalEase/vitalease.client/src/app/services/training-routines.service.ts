import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TrainingRoutinesService {

 
  private apiUrl = 'https://localhost:7180/api'; 

  constructor(private http: HttpClient) { }

  getRoutines(userId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/getRoutinesOnExercises/${userId}`); 
  }

  getExercises(routineId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/getExercisesFromRoutine/${routineId}`); 
  }

}

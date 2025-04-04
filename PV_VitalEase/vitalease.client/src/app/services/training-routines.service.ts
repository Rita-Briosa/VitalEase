import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TrainingRoutinesService {

 
  private apiUrl = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api';

  private apiUrlAddRoutine = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/addNewRoutine';

  constructor(private http: HttpClient) { }

  getRoutines(): Observable<any> {
    return this.http.get(`${this.apiUrl}/getRoutines`); 
  }

  getCustomTrainingRoutines(userId: number): Observable<any> {
    let params = new HttpParams();
    params = params.set('userId', userId);

    return this.http.get(`${this.apiUrl}/getCustomTrainingRoutines`, { params });
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

  addCustomRoutine(userId: number, newName: string, newDescription: string, newType: string, newRoutineLevel: string, newNeeds: string): Observable<any> {
    let exercises: number[] = [];
    return this.http.post<any>(`${this.apiUrl}/addNewCustomRoutine/${userId}`, { newName, newDescription, newType, newRoutineLevel, newNeeds, exercises });
  }

  deleteRoutine(routineId: number): Observable<any> {
    let params = new HttpParams();
    params.set('routineId', routineId);

    return this.http.delete<any>(`${this.apiUrl}/deleteRoutine/${routineId}`, { params });
  }

  deleteExerciseFromRoutine(routineId: number, exerciseId: number): Observable<any>{
    let params = new HttpParams();
    params.set('routineId', routineId);
    params.set('exerciseId', exerciseId);

    return this.http.delete<any>(`${this.apiUrl}/deleteExerciseFromRoutine/${routineId}/${exerciseId}`, {params});
  }

  getExerciseRoutine(routineId: number, exerciseId: number): Observable<any> {
    let params = new HttpParams()
      .set('routineId', routineId)
      .set('exerciseId', exerciseId);

    return this.http.get<any>(`${this.apiUrl}/getExerciseRoutine/${routineId}`, { params });
  }

  editExerciseRoutine(routineId: number, exerciseId: number, reps: number, duration: number, sets: number): Observable<any> {
    if (reps !== null && reps !== 0 && (duration === null || duration === 0) && (sets !== null && sets !== undefined && sets > 0)) {
      return this.http.put<any>(`${this.apiUrl}/editExerciseRoutine/${exerciseId}/${routineId}`, { reps, sets });
    } else if (duration !== null && duration !== 0 && (reps === null || reps === 0) && (sets !== null && sets !== undefined && sets > 0)) {
      return this.http.post<any>(`${this.apiUrl}/editExerciseRoutine/${exerciseId}/${routineId}`, { duration, sets });
    } else {
      return this.http.post<any>(`${this.apiUrl}/editExerciseRoutine/${exerciseId}/${routineId}`, { });
    }
  }
}

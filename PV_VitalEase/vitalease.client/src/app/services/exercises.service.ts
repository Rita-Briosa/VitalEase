import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ExercisesService {

  // O endereço da sua API que retorna os logs
  private apiUrl = 'https://localhost:7180/api'; // Endereço do seu backend ASP.NET Core

  private apiUrlAddRoutine = 'https://localhost:7180/api/addRoutine';

  private apiUrlAddExercise = 'https://localhost:7180/api/addExercise';
  constructor(private http: HttpClient) { }

  // Método para pegar os logs
  getExercises(): Observable<any> {
    return this.http.get(`${this.apiUrl}/getExercises`); // Envia uma requisição GET para pegar os logs
  }

  getMedia(exerciseId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/getMedia/${exerciseId}`); // Correto: agora o exerciseId é passado corretamente na URL
  }

  getFilteredExercises(filters: any): Observable<any> {
    let params = new HttpParams();

    Object.keys(filters).forEach(key => {
      if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
        params = params.set(key, filters[key])
      }
    });

    return this.http.get(`${this.apiUrl}/getFilteredExercises`, { params });
  }

  getRoutines(userId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/getRoutinesOnExercises/${userId}`); // Correto: agora o exerciseId é passado corretamente na URL
  }

  addRoutine(routineId: number, exerciseId: number, reps?: number, duration?: number, sets?: number): Observable<any> {

    if (reps !== null && reps !== 0 && (duration === null || duration === 0) && (sets !== null && sets !== undefined && sets > 0)) {
      return this.http.post<any>(this.apiUrlAddRoutine, { routineId, exerciseId, reps, sets });
    } else if (duration !== null && duration !== 0 && (reps === null || reps === 0) && (sets !== null && sets !== undefined && sets > 0)) {
      return this.http.post<any>(this.apiUrlAddRoutine, { routineId, exerciseId, duration, sets });
    } else {
      return this.http.post<any>(this.apiUrlAddRoutine, { routineId, exerciseId });
    }
   
  }

  addExercise(newName: string, newDescription: string, newType: string, newDifficultyLevel: string, newMuscleGroup: string, newEquipmentNecessary: string,
    newMediaName: string, newMediaType: string, newMediaUrl: string, newMediaName1: string, newMediaType1: string,
    newMediaUrl1: string, newMediaName2?: string, newMediaType2?: string, newMediaUrl2?: string): Observable<any> {

    if (newMediaName2 && newMediaType2 && newMediaUrl2) {
      return this.http.post<any>(this.apiUrlAddExercise, {
        newName, newDescription, newType, newDifficultyLevel, newMuscleGroup, newEquipmentNecessary,
        newMediaName, newMediaType, newMediaUrl, newMediaName1, newMediaType1, newMediaUrl1, newMediaName2, newMediaType2, newMediaUrl2
      });
    }
    else {
      return this.http.post<any>(this.apiUrlAddExercise, {
        newName, newDescription, newType, newDifficultyLevel, newMuscleGroup, newEquipmentNecessary,
        newMediaName, newMediaType, newMediaUrl, newMediaName1, newMediaType1, newMediaUrl1
      });
    }
  }

}

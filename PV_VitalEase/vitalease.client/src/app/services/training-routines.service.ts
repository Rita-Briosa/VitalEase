import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * @service TrainingRoutinesService
 * @description
 * The TrainingRoutinesService provides methods to interact with the backend API for managing training routines and associated exercises.
 * It allows you to retrieve all routines, custom training routines for a specific user, filtered routines based on criteria,
 * and exercises that can be added to a routine. In addition, it provides methods to retrieve detailed exercise information,
 * media related to an exercise, add routines (both standard and custom), delete routines, and edit or delete exercises within a routine.
 *
 * @dependencies
 * - HttpClient: Used to perform HTTP requests to the backend API.
 *
 * @usage
 * This service is provided in the root injector and can be injected into components that require functionality for managing training routines.
 */
@Injectable({
  providedIn: 'root'
})
export class TrainingRoutinesService {

 
  private apiUrl = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api';

  private apiUrlAddRoutine = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/addNewRoutine';

  constructor(private http: HttpClient) { }

  /**
   * @method getRoutines
   * @description
   * Retrieves all training routines.
   *
   * @returns An Observable that emits the list of routines.
   */
  getRoutines(): Observable<any> {
    return this.http.get(`${this.apiUrl}/getRoutines`); 
  }

  /**
   * @method getCustomTrainingRoutines
   * @description
   * Retrieves custom training routines for a specific user.
   *
   * @param userId - The ID of the user.
   * @returns An Observable that emits the list of custom training routines.
   */
  getCustomTrainingRoutines(userId: number): Observable<any> {
    let params = new HttpParams();
    params = params.set('userId', userId);

    return this.http.get(`${this.apiUrl}/getCustomTrainingRoutines`, { params });
  }

  /**
   * @method getFilteredRoutines
   * @description
   * Retrieves routines that match the provided filter criteria.
   *
   * @param filters - An object containing filter criteria.
   * @returns An Observable that emits the filtered list of routines.
   */
  getFilteredRoutines(filters: any): Observable < any > {
      let params = new HttpParams();

      Object.keys(filters).forEach(key => {
        if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
          params = params.set(key, filters[key])
        }
      });

      return this.http.get(`${this.apiUrl}/getFilteredRoutines`, { params });
  }

  /**
   * @method getExercisesToAddToRoutine
   * @description
   * Retrieves the list of exercises available for adding to a routine.
   *
   * @returns An Observable that emits the list of exercises.
   */
  getExercisesToAddToRoutine(): Observable<any> {
    return this.http.get(`${this.apiUrl}/getExercises`); // Envia uma requisição GET para pegar os logs
  }

  /**
   * @method getExercises
   * @description
   * Retrieves the exercises associated with a specific training routine.
   *
   * @param routineId - The ID of the routine.
   * @returns An Observable that emits the list of exercises in the routine.
   */
  getExercises(routineId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/getExercisesFromRoutine/${routineId}`); 
  }

  /**
   * @method getExercise
   * @description
   * Retrieves detailed information for a specific exercise within a routine.
   *
   * @param exerciseId - The ID of the exercise.
   * @returns An Observable that emits the exercise details.
   */
  getExercise(exerciseId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/getExerciseDetailsFromRoutine/${exerciseId}`);
  }

  /**
   * @method getMedia
   * @description
   * Retrieves media associated with a specific exercise.
   *
   * @param exerciseId - The ID of the exercise.
   * @returns An Observable that emits the media items for the exercise.
   */
  getMedia(exerciseId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/getExerciseMediaFromRoutine/${exerciseId}`);
  }

  /**
   * @method addRoutine
   * @description
   * Adds an exercise to a routine with optional parameters (reps, duration, sets).
   * Depending on the provided values, different payloads are sent to the backend.
   *
   * @param routineId - The ID of the routine.
   * @param exerciseId - The ID of the exercise.
   * @param reps - (Optional) Number of repetitions.
   * @param duration - (Optional) Duration for the exercise.
   * @param sets - (Optional) Number of sets.
   * @returns An Observable that emits the response from the backend.
   */
  addRoutine(newName: string, newDescription: string, newType: string, newRoutineLevel: string, newNeeds: string, exercises: number[]): Observable<any> {

    return this.http.post<any>(this.apiUrlAddRoutine, { newName, newDescription, newType, newRoutineLevel, newNeeds, exercises });
    
  }

  /**
   * @method addCustomRoutine
   * @description
   * Adds a new custom training routine for a specific user.
   *
   * @param userId - The ID of the user.
   * @param newName - The name of the new routine.
   * @param newDescription - A description of the new routine.
   * @param newType - The type of the routine.
   * @param newRoutineLevel - The difficulty level of the routine.
   * @param newNeeds - Specific needs associated with the routine.
   * @returns An Observable that emits the response from the backend.
   */
  addCustomRoutine(userId: number, newName: string, newDescription: string, newType: string, newRoutineLevel: string, newNeeds: string): Observable<any> {
    let exercises: number[] = [];
    return this.http.post<any>(`${this.apiUrl}/addNewCustomRoutine/${userId}`, { newName, newDescription, newType, newRoutineLevel, newNeeds, exercises });
  }

  /**
   * @method deleteRoutine
   * @description
   * Deletes a training routine.
   *
   * @param routineId - The ID of the routine to delete.
   * @returns An Observable that emits the response from the backend.
   */
  deleteRoutine(routineId: number): Observable<any> {
    let params = new HttpParams();
    params.set('routineId', routineId);

    return this.http.delete<any>(`${this.apiUrl}/deleteRoutine/${routineId}`, { params });
  }

  /**
   * @method deleteExerciseFromRoutine
   * @description
   * Deletes a specific exercise from a training routine.
   *
   * @param routineId - The ID of the routine.
   * @param exerciseId - The ID of the exercise to delete.
   * @returns An Observable that emits the response from the backend.
   */
  deleteExerciseFromRoutine(routineId: number, exerciseId: number): Observable<any>{
    let params = new HttpParams();
    params.set('routineId', routineId);
    params.set('exerciseId', exerciseId);

    return this.http.delete<any>(`${this.apiUrl}/deleteExerciseFromRoutine/${routineId}/${exerciseId}`, {params});
  }

  /**
   * @method getExerciseRoutine
   * @description
   * Retrieves the relation details (e.g., sets, reps) between a specific exercise and a training routine.
   *
   * @param routineId - The ID of the routine.
   * @param exerciseId - The ID of the exercise.
   * @returns An Observable that emits the relation details.
   */
  getExerciseRoutine(routineId: number, exerciseId: number): Observable<any> {
    let params = new HttpParams()
      .set('routineId', routineId)
      .set('exerciseId', exerciseId);

    return this.http.get<any>(`${this.apiUrl}/getExerciseRoutine/${routineId}`, { params });
  }

  /**
   * @method editExerciseRoutine
   * @description
   * Edits the relation details of a specific exercise in a training routine (e.g., updating reps, duration, or sets).
   * The payload sent depends on which parameters are provided.
   *
   * @param routineId - The ID of the routine.
   * @param exerciseId - The ID of the exercise.
   * @param reps - Number of repetitions (if applicable).
   * @param duration - Duration of the exercise (if applicable).
   * @param sets - Number of sets.
   * @returns An Observable that emits the response from the backend.
   */
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

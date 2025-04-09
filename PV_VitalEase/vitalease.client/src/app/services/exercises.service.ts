import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * @service ExercisesService
 * @description
 * The ExercisesService is responsible for managing all operations related to exercises and routines.
 * It communicates with the backend API to perform tasks such as retrieving exercises, filtering exercises,
 * fetching media associated with an exercise, and adding routines or exercises.
 *
 * The service constructs HTTP requests using HttpClient and returns Observables that emit the API responses.
 *
 * @dependencies
 * - HttpClient: Used to perform HTTP requests to the backend API.
 *
 * @usage
 * This service is provided in the root injector and can be injected into components that need to access or manipulate
 * exercise and routine data.
 */
@Injectable({
  providedIn: 'root'
})
export class ExercisesService {

  // O endereço da sua API que retorna os logs
  private apiUrl = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api'; // Endereço do seu backend ASP.NET Core

  private apiUrlAddRoutine = 'https://localhost:7180/api/addRoutine';

  private apiUrlAddExercise = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/addExercise';
  constructor(private http: HttpClient) { }

  /**
   * @method getExercises
   * @description
   * Retrieves the list of all exercises by sending a GET request to the '/getExercises' endpoint.
   *
   * @returns An Observable that emits the list of exercises.
   */
  getExercises(): Observable<any> {
    return this.http.get(`${this.apiUrl}/getExercises`); // Envia uma requisição GET para pegar os logs
  }

  /**
   * @method getMedia
   * @description
   * Retrieves the media associated with a specific exercise.
   *
   * @param exerciseId - The unique identifier of the exercise.
   * @returns An Observable that emits the media for the specified exercise.
   */
  getMedia(exerciseId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/getMedia/${exerciseId}`); // Correto: agora o exerciseId é passado corretamente na URL
  }

  /**
   * @method getFilteredExercises
   * @description
   * Retrieves exercises that match the provided filter criteria.
   * The filters are sent as query parameters in the GET request.
   *
   * @param filters - An object containing filter properties (e.g., type, difficultyLevel, muscleGroup, equipmentNeeded).
   * @returns An Observable that emits the filtered list of exercises.
   */
  getFilteredExercises(filters: any): Observable<any> {
    let params = new HttpParams();

    Object.keys(filters).forEach(key => {
      if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
        params = params.set(key, filters[key])
      }
    });

    return this.http.get(`${this.apiUrl}/getFilteredExercises`, { params });
  }

  /**
   * @method getRoutines
   * @description
   * Retrieves routines associated with a specific user by sending a GET request to the '/getRoutinesOnExercises/{userId}' endpoint.
   *
   * @param userId - The unique identifier of the user.
   * @returns An Observable that emits the list of routines for the user.
   */
  getRoutines(userId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/getRoutinesOnExercises/${userId}`); // Correto: agora o exerciseId é passado corretamente na URL
  }

  /**
   * @method addRoutine
   * @description
   * Adds an exercise to a routine by sending a POST request to the addRoutine endpoint.
   * The request payload may include additional details such as reps, duration, and sets, depending on which values are provided.
   *
   * @param routineId - The identifier of the routine.
   * @param exerciseId - The identifier of the exercise.
   * @param reps - (Optional) The number of repetitions.
   * @param duration - (Optional) The duration for the exercise.
   * @param sets - (Optional) The number of sets.
   * @returns An Observable that emits the response from the backend.
   */
  addRoutine(routineId: number, exerciseId: number, reps?: number, duration?: number, sets?: number): Observable<any> {

    if (reps !== null && reps !== 0 && (duration === null || duration === 0) && (sets !== null && sets !== undefined && sets > 0)) {
      return this.http.post<any>(this.apiUrlAddRoutine, { routineId, exerciseId, reps, sets });
    } else if (duration !== null && duration !== 0 && (reps === null || reps === 0) && (sets !== null && sets !== undefined && sets > 0)) {
      return this.http.post<any>(this.apiUrlAddRoutine, { routineId, exerciseId, duration, sets });
    } else {
      return this.http.post<any>(this.apiUrlAddRoutine, { routineId, exerciseId });
    }
   
  }

  /**
   * @method addExercise
   * @description
   * Adds a new exercise by sending a POST request to the addExercise endpoint.
   * If optional media fields (newMediaName2, newMediaType2, newMediaUrl2) are provided, they are included in the request.
   *
   * @param newName - The name of the exercise.
   * @param newDescription - A description of the exercise.
   * @param newType - The type of the exercise.
   * @param newDifficultyLevel - The difficulty level of the exercise.
   * @param newMuscleGroup - The muscle group targeted by the exercise.
   * @param newEquipmentNecessary - The equipment required for the exercise.
   * @param newMediaName - The primary media name.
   * @param newMediaType - The primary media type.
   * @param newMediaUrl - The primary media URL.
   * @param newMediaName1 - The secondary media name.
   * @param newMediaType1 - The secondary media type.
   * @param newMediaUrl1 - The secondary media URL.
   * @param newMediaName2 - (Optional) The tertiary media name.
   * @param newMediaType2 - (Optional) The tertiary media type.
   * @param newMediaUrl2 - (Optional) The tertiary media URL.
   * @returns An Observable that emits the response from the backend.
   */
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

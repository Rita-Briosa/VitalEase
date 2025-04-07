import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * @service RegisterService
 * @description
 * The RegisterService handles the user registration process by sending the user's details to the backend API.
 * It provides a method to register a new user by sending a POST request with all the required registration information.
 *
 * @dependencies
 * - HttpClient: Used to perform HTTP requests to the backend API.
 *
 * @usage
 * This service is provided in the root injector and can be injected into components that need to register new users.
 */
@Injectable({
  providedIn: 'root'
})
export class RegisterService {

  private apiUrl = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/register'; // Endereço do seu backend

  constructor(private http: HttpClient) { }

  /**
   * @method register
   * @description
   * Registers a new user by sending a POST request to the backend with the user's details.
   *
   * @param username - The user's chosen username.
   * @param birthDate - The user's birth date.
   * @param email - The user's email address.
   * @param height - The user's height (in centimeters).
   * @param weight - The user's weight (in kilograms).
   * @param gender - The user's gender.
   * @param password - The user's chosen password.
   * @param heartProblems - Boolean indicating if the user has heart problems.
   * @returns An Observable that emits the response from the backend.
   */
  register(username: string, birthDate: Date, email: string,height: number, weight:number, gender: string, password: string, heartProblems:boolean): Observable<any> {

    return this.http.post(this.apiUrl, { username, birthDate, email, height, weight, gender, password, heartProblems });
  }
}

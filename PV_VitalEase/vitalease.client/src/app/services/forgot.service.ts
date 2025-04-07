import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * @service ForgotService
 * @description
 * The ForgotService is responsible for handling password reset requests.
 * It sends the user's email address to the backend API to initiate the "forgot password" process.
 *
 * @dependencies
 * - HttpClient: Used to perform HTTP POST requests to the backend.
 *
 * @usage
 * This service is provided in the root injector and can be injected into components that need to trigger a password reset.
 */
@Injectable({
  providedIn: 'root'
})
export class ForgotService {

  private apiUrl = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/forgotPassword'; // Endereço atualizado do backend
 
  constructor(private http: HttpClient) { }

  /**
   * @method ForgotPassword
   * @description
   * Sends a POST request to the backend with the user's email address to initiate the forgot password process.
   *
   * @param email - The email address of the user requesting a password reset.
   * @returns An Observable that emits the response from the backend.
   */
  ForgotPassword(email: string): Observable<any> {
  
    return this.http.post<any>(this.apiUrl, { email }); // Garantindo que o email seja enviado como um objeto
  }
}

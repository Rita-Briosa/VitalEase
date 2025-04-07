import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * @service VerifyEmailService
 * @description
 * The VerifyEmailService handles the validation of email verification tokens.
 * It provides a method to validate a token by sending it as a query parameter to the backend API.
 *
 * @dependencies
 * - HttpClient: Used to perform HTTP GET requests to the backend.
 *
 * @usage
 * This service is provided in the root injector and can be injected into components that require email verification functionality.
 */
@Injectable({
  providedIn: 'root'
})

export class VerifyEmailService {
  private apiUrlValidateVerifyEmailToken = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/ValidateVerifyEmailToken';

  constructor(private http: HttpClient) { }

  /**
   * @method validateToken
   * @description
   * Validates the provided email verification token by sending it as a query parameter in a GET request.
   *
   * @param token - The email verification token to validate.
   * @returns An Observable that emits the response from the backend.
   */
  validateToken(token: string): Observable<any> {

    const url = `${this.apiUrlValidateVerifyEmailToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }
}

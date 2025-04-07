import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * @service ConfirmNewEmailService
 * @description
 * The ConfirmNewEmailService handles operations related to confirming a user's new email address.
 * It provides methods to validate, confirm, and cancel a new email change request by communicating with the backend API.
 * Each method sends the token as a query parameter in a GET request to the appropriate endpoint.
 *
 * @dependencies
 * - HttpClient: Used to perform HTTP requests to the backend.
 *
 * @usage
 * This service is provided in the root injector and can be injected into components that require email change verification functionality.
 */
@Injectable({
  providedIn: 'root'
})

export class ConfirmNewEmailService {
  private apiUrlConfirmNewEmailToken = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/ConfirmNewEmailChange';

  private apiUrlValidateNewEmailToken = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/ValidateNewEmailToken';

  private apiUrlCancelNewEmailToken = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/CancelNewEmailChange';

  constructor(private http: HttpClient) { }

  /**
   * @method validateToken
   * @description
   * Validates the new email token by sending it as a query parameter to the validation endpoint.
   *
   * @param token - The JWT token to validate.
   * @returns An Observable containing the backend response.
   */
  validateToken(token: string): Observable<any> {

    const url = `${this.apiUrlValidateNewEmailToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }

  /**
   * @method confirmNewEmailChange
   * @description
   * Confirms the new email change by sending the provided token as a query parameter to the confirmation endpoint.
   *
   * @param token - The JWT token used to confirm the new email change.
   * @returns An Observable containing the backend response.
   */
  confirmNewEmailChange(token: string): Observable<any> {
    const url = `${this.apiUrlConfirmNewEmailToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }

  /**
   * @method cancelNewEmailChange
   * @description
   * Cancels the new email change process by sending the provided token as a query parameter to the cancellation endpoint.
   *
   * @param token - The JWT token used to cancel the new email change.
   * @returns An Observable containing the backend response.
   */
  cancelNewEmailChange(token: string): Observable<any> {
    const url = `${this.apiUrlCancelNewEmailToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }
}

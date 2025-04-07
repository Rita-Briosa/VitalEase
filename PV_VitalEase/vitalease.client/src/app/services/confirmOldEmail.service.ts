import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * @service ConfirmOldEmailService
 * @description
 * The ConfirmOldEmailService handles operations related to confirming a user's old email address during an email change process.
 * It provides methods to validate, confirm, and cancel the old email change request by communicating with the backend API.
 * Each method sends the token as a query parameter in a GET request to the appropriate endpoint.
 *
 * @dependencies
 * - HttpClient: Used to perform HTTP requests to the backend.
 *
 * @usage
 * This service is provided in the root injector and can be injected into components that require functionality for verifying or canceling an old email change.
 */
@Injectable({
  providedIn: 'root'
})

export class ConfirmOldEmailService {
  private apiUrlConfirmOldEmailToken = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/ConfirmOldEmailToken';

  private apiUrlValidateOldEmailToken = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/ValidateOldEmailToken';

  private apiUrlCancelOldEmailToken = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/CancelOldEmailChange';


  constructor(private http: HttpClient) { }

  /**
   * @method validateToken
   * @description
   * Validates the old email token by sending it as a query parameter to the backend's validation endpoint.
   *
   * @param token - The JWT token to validate.
   * @returns An Observable that emits the backend response.
   */
  validateToken(token: string): Observable<any> {

    const url = `${this.apiUrlValidateOldEmailToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }

  /**
   * @method confirmOldEmailChange
   * @description
   * Confirms the old email change by sending the provided token as a query parameter to the confirmation endpoint.
   *
   * @param token - The JWT token used to confirm the old email change.
   * @returns An Observable that emits the backend response.
   */
  confirmOldEmailChange(token: string): Observable<any> {
    const url = `${this.apiUrlConfirmOldEmailToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }

  /**
   * @method cancelOldEmailChange
   * @description
   * Cancels the old email change process by sending the provided token as a query parameter to the cancellation endpoint.
   *
   * @param token - The JWT token used to cancel the old email change.
   * @returns An Observable that emits the backend response.
   */
  cancelOldEmailChange(token: string): Observable<any> {
    const url = `${this.apiUrlCancelOldEmailToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }
}

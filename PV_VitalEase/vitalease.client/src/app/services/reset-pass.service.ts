import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * @service ResetService
 * @description
 * The ResetService handles password reset operations for users who have forgotten their password.
 * It provides methods to reset the user's password and to validate the reset token.
 *
 * The service sends the user's reset token and new password to the backend API to complete the password reset process.
 * Additionally, it can validate whether a given token is still valid by sending a GET request with the token as a query parameter.
 *
 * @dependencies
 * - HttpClient: Used to perform HTTP requests to the backend API.
 *
 * @usage
 * This service is provided in the root injector and can be injected into components that require password reset functionality.
 */
@Injectable({
  providedIn: 'root'
})

export class ResetService {

  private apiUrlResetPassword = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/resetPassword';

  private apiUrlValidateToken = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/validateTokenAtAccess';

  constructor(private http: HttpClient) { }

  /**
   * @method resetPassword
   * @description
   * Sends a POST request to reset the user's password using the provided token and new password.
   *
   * @param token - The password reset token.
   * @param newPassword - The new password to be set.
   * @returns An Observable that emits the response from the backend.
   */
  resetPassword(token: string | null, newPassword: string): Observable<any> {
    const body = { token, newPassword };


    return this.http.post<any>(this.apiUrlResetPassword, body);
  }

  /**
   * @method validateToken
   * @description
   * Validates the provided reset token by sending it as a query parameter to the backend.
   *
   * @param token - The reset token to validate.
   * @returns An Observable that emits the backend's validation response.
   */
  validateToken(token: string): Observable<any> {

    const url = `${this.apiUrlValidateToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }

}

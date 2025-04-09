import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * @service MyProfileService
 * @description
 * The MyProfileService handles operations related to the user's profile. It provides methods to retrieve and update profile
 * information such as username, birth date, weight, height, gender, and heart health status. It also supports account deletion,
 * password validation, and email change processes.
 *
 * The service interacts with the backend API by making HTTP requests via HttpClient. Endpoints are provided for various operations,
 * including getting profile information, validating passwords, changing profile details, and handling account deletion requests.
 *
 * @dependencies
 * - HttpClient: Used to perform HTTP requests to the backend API.
 *
 * @usage
 * This service is provided in the root injector and can be injected into components that require profile management functionality.
 */
@Injectable({
  providedIn: 'root'
})
export class MyProfileService {

  // O endereço da sua API que retorna os logs
  private apiUrl = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api'; // Endereço do seu backend ASP.NET Core

  private apiUrlChangeBirthDate = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net//api/changeBirthDate';
  private apiUrlChangeWeight = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/changeWeight';
  private apiUrlChangeHeight = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/changeHeight';
  private apiUrlChangeGender = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/changeGender';
  private apiUrlChangeHasHeartProblems = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/changeHasHeartProblems';
  private apiUrlChangePassword = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/changePassword';
  private apiUrlChangeUsername = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/changeUsername';
  private apiUrlChangeEmail = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/changeEmail';

  private apiUrlDeleteAcc = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/deleteAccount';
  private apiUrlValidatePassword = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/validatePassword';

  private apiUrlDeleteAccountResquest = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/deleteAccountRequest';
  private apiUrlDeleteAccountCancellation = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/deleteAccountCancellation';
  private apiUrlValidateDeleteAccountToken = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/ValidateDeleteAccountToken';
  constructor(private http: HttpClient) { }

  /**
   * @method getProfileInfo
   * @description
   * Retrieves the profile information for a user based on their email address.
   *
   * @param email - The email address of the user.
   * @returns An Observable that emits the user's profile information.
   */
  getProfileInfo(email: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/getProfileInfo/${email}`);
  }
  ////////////////////////

  /**
   * @method validatePassword
   * @description
   * Validates the provided password for the specified email address.
   *
   * @param email - The user's email address.
   * @param password - The password to validate.
   * @returns An Observable that emits the result of the password validation.
   */
  validatePassword(email: string, password: string): Observable<any> {
    return this.http.post<any>(this.apiUrlValidatePassword, { email, password });
  }

  /**
   * @method deleteAccountRequest
   * @description
   * Sends a request to initiate the account deletion process for the given email address.
   *
   * @param email - The user's email address.
   * @returns An Observable that emits the response from the account deletion request.
   */
  deleteAccountRequest(email: string): Observable<any> {
    return this.http.post<any>(this.apiUrlDeleteAccountResquest, {email});
  }

  /**
   * @method deleteAccountCancellation
   * @description
   * Cancels an account deletion request using the provided token.
   *
   * @param token - The token associated with the deletion request.
   * @returns An Observable that emits the cancellation response.
   */
  deleteAccountCancellation(token: string): Observable<any> {
    return this.http.post<any>(this.apiUrlDeleteAccountCancellation, { token });
  }

  /**
   * @method deleteUserAcc
   * @description
   * Deletes the user's account by sending a DELETE request with the user's email and token.
   *
   * @param email - The user's email address.
   * @param token - The token associated with the account deletion request.
   * @returns An Observable that emits the response from the backend.
   */
  deleteUserAcc(email: string, token: string): Observable<any> {
    return this.http.delete<any>(this.apiUrlDeleteAcc, {
      body: { email, token }
    });
  }
  ///////////////////////////////

  /**
   * @method validateToken
   * @description
   * Validates the delete account token by sending it as a query parameter to the backend.
   *
   * @param token - The delete account token to validate.
   * @returns An Observable that emits the validation response.
   */
  validateToken(token: string): Observable<any> {

    const url = `${this.apiUrlValidateDeleteAccountToken}?token=${token}`; // Adiciona o token como query parameter
    return this.http.get<any>(url); // Chamada para o endpoint com GET
  }

  /**
   * @method changeBirthDate
   * @description
   * Updates the user's birth date.
   *
   * @param birthDate - The new birth date as a string.
   * @param email - The user's email address.
   * @returns An Observable that emits the response from the backend.
   */
  changeBirthDate(birthDate: string, email: string): Observable<any> {
    return this.http.post<any>(this.apiUrlChangeBirthDate, { birthDate, email });
  }

  /**
   * @method changeUsername
   * @description
   * Updates the user's username.
   *
   * @param username - The new username.
   * @param email - The user's email address.
   * @returns An Observable that emits the response from the backend.
   */
  changeUsername(username: string, email: string): Observable<any> {
    return this.http.post<any>(this.apiUrlChangeUsername, { username, email });
  }

  /**
   * @method changeWeight
   * @description
   * Updates the user's weight.
   *
   * @param weight - The new weight.
   * @param email - The user's email address.
   * @returns An Observable that emits the response from the backend.
   */
  changeWeight(weight: number, email: string): Observable<any> {
    return this.http.post<any>(this.apiUrlChangeWeight, { weight, email });
  }

  /**
   * @method changeHeight
   * @description
   * Updates the user's height.
   *
   * @param height - The new height.
   * @param email - The user's email address.
   * @returns An Observable that emits the response from the backend.
   */
  changeHeight(height: number, email: string): Observable<any> {
    return this.http.post<any>(this.apiUrlChangeHeight, { height, email });
  }

  /**
   * @method changeGender
   * @description
   * Updates the user's gender.
   *
   * @param gender - The new gender.
   * @param email - The user's email address.
   * @returns An Observable that emits the response from the backend.
   */
  changeGender(gender: string, email: string): Observable<any> {
    return this.http.post<any>(this.apiUrlChangeGender, { gender, email });
  }

  /**
   * @method changeHasHeartProblems
   * @description
   * Updates the user's heart health status.
   *
   * @param hasHeartProblems - Boolean indicating if the user has heart problems.
   * @param email - The user's email address.
   * @returns An Observable that emits the response from the backend.
   */
  changeHasHeartProblems(hasHeartProblems: boolean, email: string): Observable<any> {
    return this.http.post<any>(this.apiUrlChangeHasHeartProblems, { hasHeartProblems , email });
  }

  /**
   * @method changePassword
   * @description
   * Updates the user's password.
   *
   * @param oldPassword - The current password.
   * @param newPassword - The new password.
   * @param email - The user's email address.
   * @returns An Observable that emits the response from the backend.
   */
  changePassword(oldPassword: string, newPassword: string, email: string): Observable<any> {
    return this.http.post<any>(this.apiUrlChangePassword, { oldPassword,newPassword, email });
  }

  /**
   * @method changeEmail
   * @description
   * Initiates the process to change the user's email address.
   *
   * @param password - The user's current password (for verification).
   * @param email - The user's current email address.
   * @param newEmail - The new email address to change to.
   * @returns An Observable that emits the response from the backend.
   */
  changeEmail(password: string, email: string, newEmail: string): Observable<any> {
    return this.http.post<any>(this.apiUrlChangeEmail, { password, email, newEmail });
  }

}

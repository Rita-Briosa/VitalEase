import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';

/**
 * @service AuthService
 * @description
 * The AuthService handles user authentication and session management.
 * It provides methods to log in users, store and retrieve session tokens, validate tokens with the backend,
 * and perform logout operations.
 *
 * The service uses localStorage to persist the session token and includes the token in the HTTP headers for protected API calls.
 *
 * @dependencies
 * - HttpClient: Performs HTTP requests to the backend.
 * - Router: Navigates between routes, particularly for logout redirection.
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  [x: string]: any;

  private apiUrl = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api/login'; // Endereço do seu backend
  private sessionToken: string | null = null;
  private storageKey = 'sessionToken'; 
  constructor(private http: HttpClient, private router: Router) { }

  /**
   * @method login
   * @description
   * Sends a login request to the backend with the user's email, password, and a flag indicating whether the user wants to be remembered.
   * 
   * @param email - The user's email address.
   * @param password - The user's password.
   * @param rememberMe - Boolean flag indicating whether the user should remain logged in between sessions.
   * @returns An Observable that emits the server response.
   */
  login(email: string, password: string, rememberMe: boolean): Observable<any> {
    return this.http.post(`${this.apiUrl}`, { email, password, rememberMe });
  }

  /**
   * @method getAuthHeaders
   * @description
   * Returns HTTP headers that include the session token in the Authorization header.
   *
   * @returns An instance of HttpHeaders containing the Authorization header.
   */
  getAuthHeaders(): HttpHeaders {

    return new HttpHeaders({
      Authorization: `Bearer ${this.sessionToken}`,
    });

  }

  /**
   * @method setSessionToken
   * @description
   * Stores the provided session token in localStorage and in memory.
   *
   * @param token - The session token to store.
   */
  setSessionToken(token: string): void {
    localStorage.setItem(this.storageKey, token);
    this.sessionToken = token; // Save the token in memory
  }

  /**
   * @method getSessionToken
   * @description
   * Retrieves the session token from localStorage.
   *
   * @returns The session token if it exists; otherwise, null.
   */
  getSessionToken(): string | null {
    return localStorage.getItem(this.storageKey); // Return the token
  }

  /**
   * @method logout
   * @description
   * Clears the session token from both localStorage and memory, and navigates the user to the home page.
   */
  logout(): void {
    localStorage.removeItem(this.storageKey);
    this.sessionToken = null; // Clear the token
    this.router.navigate(['/']);
  }

  /**
    * @method validateSessionToken
    * @description
    * Validates the current session token with the backend by making an HTTP GET request with the token in the Authorization header.
    *
    * @returns An Observable that emits the validation response from the backend.
    */
  validateSessionToken(): Observable<any> {
    const token = this.getSessionToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`, // Attach the token in the Authorization header
    });

    return this.http.get(`${this.apiUrl}/validate-session`, { headers });
  }

  /**
   * @method isAuthenticated
   * @description
   * Checks if a session token exists in localStorage.
   *
   * @returns True if a token exists (indicating the user is authenticated), otherwise false.
   */
  isAuthenticated(): boolean {
    return !!this.getSessionToken(); // Check if a token exists in localStorage
  }

}



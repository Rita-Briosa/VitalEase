import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  [x: string]: any;

  private apiUrl = 'https://localhost:7180/login'; // Endereço do seu backend
  private sessionToken: string | null = null;
  private storageKey = 'sessionToken'; 
  constructor(private http: HttpClient, private router: Router) { }

  login(email: string, password: string, rememberMe: boolean): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, { email, password, rememberMe });
  }

  // Armazenar informações do usuário no localStorage ou sessionStorage
 /* storeUserInfo(user: any, rememberMe: boolean): void {
    const userData = JSON.stringify(user); // Stringificar os dados do usuário
    if (rememberMe) {
      // Armazenar no localStorage para uma sessão persistente
      localStorage.setItem('userInfo', userData);
    } else {
      // Armazenar no sessionStorage para uma sessão temporária
      sessionStorage.setItem('userInfo', userData);
    }
  }*/


 /* getUserInfo(): any {
    // Verificar o localStorage primeiro (caso o "Remember Me" tenha sido marcado)
    const userInfo = localStorage.getItem('userInfo') || sessionStorage.getItem('userInfo');
    return userInfo ? JSON.parse(userInfo) : null;
  }*/

  // Verificar se o usuário está logado
  /*isLoggedIn(): boolean {
    // Verifica se as informações do usuário estão armazenadas
    return this.getUserInfo() !== null;
  }*/

  // Método de logout para remover as informações do usuário
  /*logout(): void {
    localStorage.removeItem('userInfo');
    sessionStorage.removeItem('userInfo');
  }*/

  // Auth Service: Include the token in the headers
  getAuthHeaders(): HttpHeaders {

    return new HttpHeaders({
      Authorization: `Bearer ${this.sessionToken}`,
    });

  }

  // Set the session token after login
  setSessionToken(token: string): void {
    localStorage.setItem(this.storageKey, token);
    this.sessionToken = token; // Save the token in memory
  }

  // Get the session token
  getSessionToken(): string | null {
    return localStorage.getItem(this.storageKey); // Return the token
  }

  // Logout (clear the session token)
  logout(): void {
    localStorage.removeItem(this.storageKey);
    this.sessionToken = null; // Clear the token
    this.router.navigate(['/']);
  }

  // Validate the token with the backend
  validateSessionToken(): Observable<any> {
    const token = this.getSessionToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`, // Attach the token in the Authorization header
    });

    return this.http.get(`${this.apiUrl}/validate-session`, { headers });
  }

  // Check if the user is authenticated
  isAuthenticated(): boolean {
    return !!this.getSessionToken(); // Check if a token exists in localStorage
  }

}



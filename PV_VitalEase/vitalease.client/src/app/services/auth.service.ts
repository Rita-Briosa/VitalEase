import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  [x: string]: any;

  private apiUrl = 'https://localhost:7180/login'; // Endereço do seu backend
  private sessionToken: string | null = null;
  constructor(private http: HttpClient, private router: Router) { }

  // Método de login para autenticar o usuário
  login(email: string, password: string): Observable<any> {
    return this.http.post(this.apiUrl, { email, password });
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

  isAuthenticated(): boolean {
    return !!this.sessionToken; // Check if the session token exists
  }

  // Set the session token after login
  setSessionToken(token: string): void {
    this.sessionToken = token; // Save the token in memory
  }

  // Get the session token
  getSessionToken(): string | null {
    return this.sessionToken; // Return the token
  }

  // Logout (clear the session token)
  logout(): void {
    this.sessionToken = null; // Clear the token
    this.router.navigate(['/']);
  }

  // Validate the session token with the backend
  validateSessionToken(): Observable<any> {
    const headers = new HttpHeaders({
      Authorization: `Bearer ${this.sessionToken}`,
    });

    // Send a request to the backend to validate the token
    return this.http.get(`${this.apiUrl}/validate-session`, { headers });
  }
}



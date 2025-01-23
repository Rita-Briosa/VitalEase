import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = 'https://vitalease2025.3utilities.com/api/login'; // Endereço do seu backend

  constructor(private http: HttpClient) { }

  // Método de login para autenticar o usuário
  login(email: string, password: string, rememberMe: boolean): Observable<any> {

    return this.http.post(this.apiUrl, { email, password, rememberMe });
  }

  // Armazenar informações do usuário no localStorage ou sessionStorage
  storeUserInfo(user: any, rememberMe: boolean): void {
    const userData = JSON.stringify(user); // Stringificar os dados do usuário
    if (rememberMe) {
      // Armazenar no localStorage para uma sessão persistente
      localStorage.setItem('userInfo', userData);
    } else {
      // Armazenar no sessionStorage para uma sessão temporária
      sessionStorage.setItem('userInfo', userData);
    }
  }

  getUserInfo(): any {
    // Verificar o localStorage primeiro (caso o "Remember Me" tenha sido marcado)
    const userInfo = localStorage.getItem('userInfo') || sessionStorage.getItem('userInfo');
    return userInfo ? JSON.parse(userInfo) : null;
  }

  // Verificar se o usuário está logado
  isLoggedIn(): boolean {
    // Verifica se as informações do usuário estão armazenadas
    return this.getUserInfo() !== null;
  }

  // Método de logout para remover as informações do usuário
  logout(): void {
    localStorage.removeItem('userInfo');
    sessionStorage.removeItem('userInfo');
  }
}

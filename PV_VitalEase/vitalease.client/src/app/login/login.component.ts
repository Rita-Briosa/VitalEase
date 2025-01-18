import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  email: string = '';
  password: string = '';
  errorMessage: string = '';
  rememberMe: boolean = false; // Checkbox "Remember Me"

  constructor(private authService: AuthService, private router: Router) { }

  onSubmit() {
    this.authService.login(this.email, this.password, this.rememberMe).subscribe(
      (response: any) => {
        console.log('Login successful', response);

        // Armazena as informações do usuário no AuthService
        this.authService.storeUserInfo(response.user, this.rememberMe);

        // Redireciona com base no tipo de usuário
        this.redirectBasedOnUserType(response.user.type);
      },
      (error) => {
           this.errorMessage = error.error?.message || 'An unexpected error occurred';
      console.log('Login error', error);
      }
    );
  }

  // Método para redirecionar com base no tipo de usuário
  private redirectBasedOnUserType(userType: number): void {
    if (userType === 0) {
      // Tipo de usuário 0: Redireciona para a home page
      this.router.navigate(['/']);
    } else if (userType === 1) {
      // Tipo de usuário 1: Redireciona para a dashboard
      this.router.navigate(['/dashboard']);
    } else {
      // Redireciona para uma página padrão caso o tipo de usuário não seja reconhecido
      console.log('Unknown user type');
      this.router.navigate(['/']);
    }
  }
}


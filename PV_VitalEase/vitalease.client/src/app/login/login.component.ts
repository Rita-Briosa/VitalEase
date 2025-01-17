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
  rememberMe: boolean = false;

  constructor(private authService: AuthService, private router: Router) { }

  onSubmit() {
    this.authService.login(this.email, this.password, this.rememberMe).subscribe(
      (response: any) => {
        console.log('Login successful', response);

        // Armazenar as informações do usuário após o login
        // Se "Remember Me" estiver selecionado, usamos o localStorage
        if (this.rememberMe) {
          // Armazenar no localStorage para sessão duradoura
          localStorage.setItem('userInfo', JSON.stringify(response.user));
        } else {
          // Armazenar no sessionStorage para sessão temporária
          sessionStorage.setItem('userInfo', JSON.stringify(response.user));
        }

        // Verificar o tipo de usuário e redirecionar para a página apropriada
        if (response.user.type === 0) {
          // Tipo de usuário 0: Redireciona para a home page
          this.router.navigate(['/']);
        } else if (response.user.type === 1) {
          // Tipo de usuário 1: Redireciona para a dashboard
          this.router.navigate(['/dashboard']);
        } else {
          // Redireciona para uma página padrão ou erro, caso o tipo de usuário não seja reconhecido
          console.log('Unknown user type');
          this.router.navigate(['/']);
        }
      },
      (error) => {
        this.errorMessage = 'Email or password is incorrect';
        console.log('Login error', error);
      }
    );
  }
}


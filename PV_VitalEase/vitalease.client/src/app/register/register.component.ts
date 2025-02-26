import { Component } from '@angular/core';
import { RegisterService } from '../services/register.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  username: string = '';
  birthDate: Date = new Date();
  email: string = '';
  height: number = 90;
  weight: number = 30;
  gender: string = '';
  password: string = '';
  confirmPassword: string = '';
  errorMessage: string = '';
  heartProblems: boolean = false;
  successMessage: string = ''; // Adicionado para exibir sucesso
  passwordStrength: number = 0; // Valor numérico da força da senha
  passwordFeedback: string = '';

  constructor(private registerService: RegisterService, private router: Router) { }

  onSubmit() {
    this.errorMessage = ''; // Limpa mensagens antigas
    this.successMessage = '';

 

    // Passando os dados formatados para o serviço
    this.registerService.register(
      this.username,
     this.birthDate,  // A data formatada
      this.email,
      this.height,
      this.weight,
      this.gender,
      this.password,
      this.heartProblems
    ).subscribe(
      (response: any) => {
        this.clearForm();
        console.log('Register successful', response);
        this.successMessage = response.message;
        setTimeout(() => {
          this.router.navigate(['/login']); // Redireciona após 2 segundos
        }, 2000);
       
      },
      (error) => {
        this.errorMessage = error.error?.message || 'An unexpected error occurred';
        console.log('Register error', error);
      }
    );
  }

  clearForm() {
    this.username = '';
    this.birthDate = new Date();
    this.email = '';
    this.height = 90;
    this.weight = 30;
    this.gender = '';
    this.password = '';
    this.confirmPassword = '';
    this.heartProblems = false;
  }

  calculatePasswordStrength(password: string): void {
    this.passwordStrength = this.getStrengthScore(password);
    this.passwordFeedback = this.getStrengthFeedback(this.passwordStrength);
  }

  private getStrengthScore(password: string): number {
    let score = 0;

    if (!password) return score;

    if (password.length >= 12) score += 25; // Comprimento mínimo
    if (/[a-z]/.test(password)) score += 25; // Letras minúsculas
    if (/[A-Z]/.test(password)) score += 25; // Letras maiúsculas
    if (/[@$!%*?&]/.test(password)) score += 25; // Caracteres especiais

    return score;
  }

  // Retorna feedback textual baseado na força
  private getStrengthFeedback(score: number): string {
    if (score < 50) return 'Weak';
    if (score < 75) return 'Moderate';
    return 'Strong'
  }

}

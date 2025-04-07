import { Component } from '@angular/core';
import { RegisterService } from '../services/register.service';
import { Router } from '@angular/router';

/**
 * @component RegisterComponent
 * @description
 * The RegisterComponent handles user registration for the application.
 * It presents a registration form that collects user details such as username, birth date, email, height, weight, gender,
 * password, and heart health status. The component performs basic form operations including clearing the form after successful
 * registration, validating password strength, and providing user feedback. On successful registration, the user is redirected
 * to the login page after a short delay.
 *
 * @dependencies
 * - RegisterService: Handles communication with the backend to register a new user.
 * - Router: Manages navigation between application routes.
 *
 * @usage
 * This component is not standalone and relies on external templates and styles:
 *   - Template: './register.component.html'
 *   - Styles: ['./register.component.css']
 */
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

  /**
   * @method onSubmit
   * @description
   * Called when the registration form is submitted. This method clears any previous feedback messages,
   * sends the registration data to the backend via RegisterService, and handles the response.
   * On successful registration, the form is cleared, a success message is shown, and the user is redirected to the login page
   * after a brief delay. On error, an appropriate error message is displayed.
   */
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

  /**
   * @method clearForm
   * @description
   * Resets the registration form fields to their default values.
   */
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

  /**
   * @method calculatePasswordStrength
   * @description
   * Evaluates the strength of the provided password by calculating a numeric strength score and
   * setting corresponding textual feedback.
   *
   * @param password - The password to evaluate.
   */
  calculatePasswordStrength(password: string): void {
    this.passwordStrength = this.getStrengthScore(password);
    this.passwordFeedback = this.getStrengthFeedback(this.passwordStrength);
  }

  /**
   * @method getStrengthScore
   * @description
   * Calculates a strength score for a password based on length and the presence of lowercase, uppercase, and special characters.
   *
   * @param password - The password to evaluate.
   * @returns A numeric score representing the strength of the password.
   */
  private getStrengthScore(password: string): number {
    let score = 0;

    if (!password) return score;

    if (password.length >= 12) score += 25; // Comprimento mínimo
    if (/[a-z]/.test(password)) score += 25; // Letras minúsculas
    if (/[A-Z]/.test(password)) score += 25; // Letras maiúsculas
    if (/[@$!%*?&]/.test(password)) score += 25; // Caracteres especiais

    return score;
  }

  /**
   * @method getStrengthFeedback
   * @description
   * Provides textual feedback based on the numeric strength score of a password.
   *
   * @param score - The strength score of the password.
   * @returns A string indicating whether the password is Weak, Moderate, or Strong.
   */
  private getStrengthFeedback(score: number): string {
    if (score < 50) return 'Weak';
    if (score < 75) return 'Moderate';
    return 'Strong'
  }

}

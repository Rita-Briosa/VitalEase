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

  /**
 * @method limitNumberOfDigitsWeight
 * @description
 * Input validator that restricts user input for the "newWeight" field to valid numeric values between 30 and 400.
 * It permits navigation keys (ArrowLeft, ArrowRight) and editing keys (Delete, Backspace),
 * prevents leading zeros, and restricts input to a maximum of three digits.
 *
 * @param {KeyboardEvent} event - The keyboard event triggered on key press in the input field.
 *
 * @remarks
 * - Prevents input if the character is not a digit or if the resulting value is outside the valid range.
 * - Ensures that inputs like "000" are blocked.
 * - Maintains data integrity by validating on every keystroke.
 */
  limitNumberOfDigitsWeight(event: KeyboardEvent) {
    const input = event.target as HTMLInputElement;
    const key = event.key;
    const value = input.value;

    if (key === "ArrowLeft" || key === "ArrowRight" || key === "Delete" || key === "Backspace") {
      return;
    }

    // Impede que o primeiro número seja "0"
    if (value === "" && (key === "0")) {
      event.preventDefault();
    }

    if (value.length < 3 && /^[0-9]$/.test(key)) {
      const newValue = value + key;
      const numeric = parseInt(newValue, 10);
      if (numeric <= 0) {
        event.preventDefault();
      }
    }

    // Limita o comprimento a 3 dígitos para weight
    if (value.length >= 3 || !/^[0-9]$/.test(key)) {
      event.preventDefault();
    }

  }

  /**
* @method limitNumberOfDigitsHeight
* @description
* Input validator that restricts user input for the "newHeight" field to valid numeric values between 90 and 251.
* It permits navigation keys (ArrowLeft, ArrowRight) and editing keys (Delete, Backspace),
* prevents leading zeros, and restricts input to a maximum of three digits.
*
* @param {KeyboardEvent} event - The keyboard event triggered on key press in the input field.
*
* @remarks
* - Prevents input if the character is not a digit or if the resulting value is outside the valid range.
* - Ensures that inputs like "000" are blocked.
* - Maintains data integrity by validating on every keystroke.
*/
  limitNumberOfDigitsHeight(event: KeyboardEvent) {
    const input = event.target as HTMLInputElement;
    const key = event.key;
    const value = input.value;

    if (key === "ArrowLeft" || key === "ArrowRight" || key === "Delete" || key === "Backspace") {
      return;
    }

    // Impede que o primeiro número seja "0"
    if (value === "" && (key === "0")) {
      event.preventDefault();
    }

    if (value.length < 3 && /^[0-9]$/.test(key)) {
      const newValue = value + key;
      const numeric = parseInt(newValue, 10);
      if (numeric <= 0) {
        event.preventDefault();
      }
    }

    // Limita o comprimento a 3 dígitos para weight
    if (value.length >= 3 || !/^[0-9]$/.test(key)) {
      event.preventDefault();
    }

  }

  /**
* @method profileAge
* @description
* Calculates the user's age based on the date entered in the input field with ID "birthDateInput".
* It compares the birth date to the current date, adjusting the result if the user hasn't had their birthday yet this year.
*
* @returns {number} The calculated age based on the input birth date. Returns 0 if the input is invalid or not provided.
*
* @remarks
* - Relies on an HTML input element with the ID "birthDateInput".
* - Validates that the input exists and contains a value before attempting to parse.
* - Uses standard JavaScript `Date` operations to determine if the birthday has occurred this year.
* - Returns 0 if the input is missing or malformed.
*/
  get profileAge(): number {
    const birthDateInput = <HTMLInputElement>document.getElementById("birthDateInput");
    if (!birthDateInput || !birthDateInput.value) return 0;

    const birthDate = new Date(birthDateInput.value);
    const today = new Date();

    let age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();
    const dayDiff = today.getDate() - birthDate.getDate();

    // Corrige se ainda não fez anos este ano
    if (monthDiff < 0 || (monthDiff === 0 && dayDiff < 0)) {
      age--;
    }

    return age;
  }

}

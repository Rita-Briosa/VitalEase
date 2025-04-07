import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ResetService } from '../services/reset-pass.service';

/**
 * @component ResetPassComponent
 * @description
 * The ResetPassComponent handles the password reset process for users who have forgotten their password.
 * It retrieves a JWT token from the URL query parameters, validates the token via the ResetService,
 * and then allows the user to set a new password if the token is valid.
 * The component also calculates and displays password strength feedback.
 *
 * On form submission, it verifies that the new password and confirm password fields match,
 * and then sends the new password along with the token to the backend. Upon success, a success message
 * is displayed and the user is redirected to the login page after a brief delay.
 *
 * If errors occur (e.g. token expiration, mismatched passwords, or server errors), appropriate error
 * messages are displayed and a modal is used to provide further feedback.
 *
 * @dependencies
 * - ActivatedRoute: To extract query parameters (i.e., the token) from the URL.
 * - Router: To navigate between routes (e.g., redirecting to the login page).
 * - ResetService: Provides methods for validating the reset token and resetting the user's password.
 *
 * @usage
 * This component is not standalone and relies on external templates and styles:
 *   - Template: './reset-pass.component.html'
 *   - Styles: ['./reset-pass.component.css']
 */
@Component({
  selector: 'app-reset-pass',
  templateUrl: './reset-pass.component.html',
  standalone: false,
  styleUrls: ['./reset-pass.component.css'],
})
export class ResetPassComponent implements OnInit {
  token: string | null = null; // Token JWT recebido
  newPassword: string = ''; // Nova senha
  confirmPassword: string = ''; // Confirmação da senha
  errorMessage: string = ''; // Mensagem de erro
  successMessage: string = ''; // Mensagem de sucesso
  showModal: boolean = false; // Controla a exibição do modal
  modalMessage: string = ''; // Mensagem do modal

  passwordStrength: number = 0; // Valor numérico da força da senha
  passwordFeedback: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ResetService: ResetService
  ) { }

  /**
   * @method ngOnInit
   * @description
   * Lifecycle hook that is called after the component has been initialized.
   * It subscribes to the route query parameters to capture the reset token.
   * If a token is found, it validates the token. If no token is provided, an error message is shown
   * and the user is redirected to the home page.
   */
  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.token = params['token']; // Captura o token JWT enviado via URL
      console.log('Captured token:', this.token); // Verifica o valor do token

      if (!this.token) {
        this.errorMessage = 'Invalid token in URL.';
        this.router.navigate(['/']);
      } else {
        this.validateToken(this.token);
      }
    });
  }

  /**
   * @method onSubmit
   * @description
   * Called when the user submits the new password form. It first clears any previous error or success messages,
   * then checks if the new password and confirmation password match.
   * If they match, it calls the ResetService to reset the password using the token and new password.
   * On success, a success message is displayed and the user is redirected to the login page after a delay.
   * If an error occurs, an appropriate error message is displayed.
   */
  onSubmit(): void {
    // Limpa mensagens anteriores
    this.errorMessage = '';
    this.successMessage = '';

    // Verifica se as senhas coincidem
    if (this.newPassword === this.confirmPassword) {
      if (this.token) {
        // Envia a requisição para o serviço de redefinição de senha
        this.ResetService.resetPassword(this.token, this.newPassword).subscribe(
          (response) => {
            // Se a senha for redefinida com sucesso
            this.successMessage = response.message;
            console.log('Your password has been reset successfully.', response);
            setTimeout(() => {
              this.router.navigate(['/login']); // Redireciona após 2 segundos
            }, 2000);
          },
          (error) => {
            // Se ocorrer erro ao redefinir a senha
            // Aqui estamos tratando a mensagem de erro com base na resposta do servidor
            if (error.status === 400 || error.status === 401) {
              // Caso o servidor retorne uma mensagem de erro, usamos ela
              const serverErrorMessage = error.error?.message || 'Invalid or expired token. Try again.';
              this.errorMessage = serverErrorMessage;
            } else {
              // Em caso de outro erro, utilizamos a mensagem de erro do servidor, se houver
              this.errorMessage =
                error.error?.message || 'An error occurred. Please try again.';
            }
          }
        );
      } else {
        this.errorMessage = 'Token is missing.';
      }
    } else {
      this.errorMessage = 'Passwords do not match.';
    }
  }

  /**
   * @method calculatePasswordStrength
   * @description
   * Calculates the strength of the given password and sets the numeric strength score and corresponding textual feedback.
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
   * Computes a numeric score for the given password based on length and the presence of lowercase, uppercase, and special characters.
   *
   * @param password - The password to evaluate.
   * @returns A numeric score representing the password's strength.
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
   * Returns textual feedback based on the computed password strength score.
   *
   * @param score - The numeric strength score.
   * @returns A string indicating if the password is Weak, Moderate, or Strong.
   */
  private getStrengthFeedback(score: number): string {
    if (score < 50) return 'Weak';
    if (score < 75) return 'Moderate';
   return 'Strong'
  }

  /**
   * @method validateToken
   * @description
   * Validates the reset token by calling the ResetService. If the token is invalid or expired,
   * an error modal is displayed with an appropriate message.
   *
   * @param token - The JWT token to validate.
   */
  validateToken(token: string): void {
    this.ResetService.validateToken(token).subscribe(
      (response) => {
        console.log(response.message); // Token válido
      },
      (error) => {
        if (error.error?.message === 'Token expired.') {
          this.showErrorModal('Token has expired. Please, request new reset password link.');
        } else {
          this.showErrorModal('Invalid token or an error happen. Please, try again.');
        }
      }
    );
  }

  /**
   * @method showErrorModal
   * @description
   * Displays a modal dialog with the provided error message.
   *
   * @param message - The message to display in the modal.
   */
  showErrorModal(message: string): void {
    this.modalMessage = message;
    this.showModal = true;
  }

  /**
   * @method closeModal
   * @description Closes the modal dialog and navigates to the home page.
   */
  closeModal(): void {
    this.showModal = false;
    this.router.navigate(['/']);
  }

}

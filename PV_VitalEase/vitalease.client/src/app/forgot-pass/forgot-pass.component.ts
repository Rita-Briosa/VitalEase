import { Component } from '@angular/core';
import { ForgotService } from '../services/forgot.service'; // Ajuste o caminho, se necessário

/**
 * @component ForgotPassComponent
 * @description
 * The ForgotPassComponent handles the "forgot password" functionality for the application.
 * It allows a user to enter their email address and submit a password reset request.
 * The component uses the ForgotService to send the reset request to the backend.
 * Upon success, a success message is displayed; if an error occurs, an error message is shown.
 *
 * @dependencies
 * - ForgotService: Provides the method for sending the forgot password request.
 *
 * @usage
 * This component is not standalone and relies on external templates and styles:
 * - Template: './forgot-pass.component.html'
 * - Styles: ['./forgot-pass.component.css']
 */
@Component({
  selector: 'app-forgot-pass',
  standalone: false,
  templateUrl: './forgot-pass.component.html',
  styleUrls: ['./forgot-pass.component.css']
})
export class ForgotPassComponent {
  email: string = '';
  errorMessage: string = '';  // Variável para mensagens de erro
  successMessage: string = ''; // Variável para mensagens de sucesso

  constructor(private forgotService: ForgotService) { }

  /**
 * @method forgotPassword
 * @description
 * Initiates the forgot password process by sending a reset request using the ForgotService.
 * It clears previous messages, then sends the email address to the backend.
 * Depending on the response, it sets a success or error message.
 */
  forgotPassword() {
    this.errorMessage = '';
    this.successMessage = '';

    this.forgotService.ForgotPassword(this.email).subscribe({
      next: (response) => {
        console.log('Sucesso:', response); 
        this.successMessage = response.message;
      },
      error: (err) => {
        console.log('Erro:', err);
        this.errorMessage = err.error.message || 'E-mail of reset password failed.';
      }
    });
  }
}

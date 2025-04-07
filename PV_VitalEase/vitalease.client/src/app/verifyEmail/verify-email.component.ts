import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { VerifyEmailService } from '../services/verify-email.service';

/**
 * @component VerifyEmailComponent
 * @description
 * The VerifyEmailComponent is responsible for handling the email verification process.
 * It extracts a JWT token from the URL query parameters and validates it using the VerifyEmailService.
 * Depending on the outcome, the component either confirms that the token is valid or displays an error message via a modal dialog.
 * The user can then be redirected accordingly (e.g., to the login page).
 *
 * @dependencies
 * - ActivatedRoute: Used to extract the JWT token from the URL.
 * - Router: Handles navigation between routes.
 * - VerifyEmailService: Provides methods to validate the email verification token.
 *
 * @usage
 * This component is not standalone and uses external templates and styles:
 *   - Template: './verify-email.component.html'
 *   - Styles: ['./verify-email.component.css']
 */
@Component({
  selector: 'app-verify-email',
  templateUrl: './verify-email.component.html',
  standalone: false,
  styleUrls: ['./verify-email.component.css'],
})
export class VerifyEmailComponent implements OnInit {
  token: string | null = null; // Token JWT recebido
  errorMessage: string = ''; // Mensagem de erro
  successMessage: string = ''; // Mensagem de sucesso
  showModal: boolean = false; // Controla a exibição do modal
  modalMessage: string = ''; // Mensagem do modal

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private VerifyEmailService: VerifyEmailService
  ) { }

  /**
   * @method ngOnInit
   * @description
   * Lifecycle hook that is called after the component is initialized.
   * It subscribes to the query parameters to capture the JWT token.
   * If the token is missing, it sets an error message and navigates to the home page;
   * otherwise, it calls validateToken() to verify the token.
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
   * @method validateToken
   * @description
   * Validates the provided JWT token by calling the VerifyEmailService.
   * If the token is invalid or expired, an error modal is displayed with an appropriate message.
   *
   * @param token - The JWT token to validate.
   */
  validateToken(token: string): void {
    this.VerifyEmailService.validateToken(token).subscribe(
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
   * @param message - The error message to display.
   */
  showErrorModal(message: string): void {
    this.modalMessage = message;
    this.showModal = true;
  }

  /**
   * @method closeModal
   * @description
   * Closes the modal dialog and navigates the user to the home page.
   */
  closeModal(): void {
    this.showModal = false;
    this.router.navigate(['/']);
  }

  /**
   * @method goToLogin
   * @description Navigates the user to the login page.
   */
  goToLogin() {
    this.router.navigate(['/login']);
  }

}

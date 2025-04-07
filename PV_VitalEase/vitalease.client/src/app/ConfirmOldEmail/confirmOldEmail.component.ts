import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ConfirmOldEmailService } from '../services/confirmOldEmail.service';

/**
 * @component ConfirmOldEmailComponent
 * @description
 * The ConfirmOldEmailComponent handles the process of confirming a user's old email address during an email change.
 * It retrieves a JWT token from the URL query parameters, validates the token, and then either confirms or cancels
 * the email change request based on user action. Modal dialogs are used to display success or error messages.
 *
 * @dependencies
 * - ActivatedRoute: To extract query parameters (the JWT token) from the URL.
 * - Router: To navigate the user to different routes based on the result.
 * - ConfirmOldEmailService: Provides methods for validating, confirming, and canceling the old email change.
 */
@Component({
  selector: 'app-confirm-old-email',
  templateUrl: './confirmOldEmail.component.html',
  standalone: false,
  styleUrls: ['./confirmOldEmail.component.css'],
})
export class ConfirmOldEmailComponent implements OnInit {
  token: string | null = null; // Token JWT recebido
  errorMessage: string = ''; // Mensagem de erro
  successMessage: string = ''; // Mensagem de sucesso
  showModal: boolean = false; // Controla a exibição do modal
  modalMessage: string = ''; // Mensagem do modal
  modalInfo: string = ''; // Mensagem do modal

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ConfirmOldEmailService: ConfirmOldEmailService
  ) { }

  /**
  * @method ngOnInit
  * @description
  * Lifecycle hook that is called after the component is initialized.
  * It subscribes to the URL query parameters to capture the JWT token.
  * If the token is present, it calls validateToken() to verify its validity;
  * if not, the user is redirected to the homepage.
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
 * Validates the provided JWT token by calling the ConfirmOldEmailService.
 * If the token is invalid or expired, an error modal is displayed with an appropriate message.
 *
 * @param token - The JWT token to be validated.
 */
  validateToken(token: string): void {
    this.ConfirmOldEmailService.validateToken(token).subscribe(
      (response) => {
        console.log(response.message); // Token válido
      },
      (error) => {
        this.modalInfo = 'Error';
        if (error.error?.message === 'Token expired.') {
          this.showErrorModal('Token has expired. Please, request new change email link.');
        } else {
          this.showErrorModal('Invalid token or an error happen. Please, try again.');
        }
      }
    );
  }

  /**
 * @method ConfirmOldEmailChange
 * @description
 * Calls the service to confirm the old email change using the token.
 * Based on the response message, it displays a modal with either success or error information.
 * After a short delay, it closes the modal and navigates the user accordingly.
 */
  ConfirmOldEmailChange(): void {
    if (this.token != null) {
      this.ConfirmOldEmailService.confirmOldEmailChange(this.token).subscribe(
        (response) => {
          this.modalInfo = 'Success';
          if (response.message === 'Token is valid.') {
            this.showErrorModal(response.message);

            setTimeout(() => {
              this.closeModal();
            }, 1500);

            setTimeout(() => {
              this.router.navigate(['/']);
            }, 2500);
          }
          else {
            this.showErrorModal(response.message);

            setTimeout(() => {
              this.closeModal();
            }, 1500);

            setTimeout(() => {
              this.router.navigate(['/changeEmailConfirmation']);
            }, 2500);

          }
         

        }, (error) => {
          this.modalInfo = 'Error';
          if (error.error?.message === 'Token expired.') {
            this.showErrorModal('Token has expired. Please, request new change email link.');
          } else {
            this.showErrorModal('Invalid token or an error happen. Please, try again.');
          }
        });
    }
  }

  /**
 * @method CancelOldEmailChange
 * @description
 * Calls the service to cancel the old email change using the token.
 * Displays a modal with the response message, then after a short delay,
 * closes the modal and navigates to the email cancellation page.
 */
  CancelOldEmailChange(): void {
    if (this.token != null) {
      this.ConfirmOldEmailService.cancelOldEmailChange(this.token).subscribe(
        (response) => {
          this.modalInfo = 'Success';
          this.showErrorModal(response.message);
          setTimeout(() => {
            this.closeModal();
          }, 1500);

          setTimeout(() => {
            this.router.navigate(['/changeEmailCancellation']);
          }, 2000);
        }, (error) => {
          this.modalInfo = 'Error';
          if (error.error?.message === 'Token expired.') {
            this.showErrorModal('Token has expired. Please, request new change email link.');
          } else {
            this.showErrorModal('Invalid token or an error happen. Please, try again.');
          }
        });
    }
  }

  /**
 * @method showErrorModal
 * @description
 * Displays a modal dialog with the provided message.
 *
 * @param message - The message to be displayed in the modal.
 */
  showErrorModal(message: string): void {
    this.modalMessage = message;
    this.showModal = true;
  }

  /**
 * @method closeModal
 * @description
 * Closes the modal dialog.
 */
  closeModal(): void {
    this.showModal = false;
  }

}

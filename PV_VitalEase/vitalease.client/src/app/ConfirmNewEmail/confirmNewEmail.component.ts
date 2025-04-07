import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ConfirmNewEmailService } from '../services/confirmNewEmail.service';

/**
 * @component ConfirmNewEmailComponent
 * @description
 * This component handles the confirmation process for a user's new email address. It extracts a JWT token from the URL query parameters,
 * validates the token, and then either confirms or cancels the email change based on user actions.
 *
 * The component interacts with the ConfirmNewEmailService to perform token validation and to execute the confirmation or cancellation of the email change.
 * It also displays modal messages to provide feedback to the user regarding the status of the operation.
 *
 * @dependencies
 * - ActivatedRoute: To access query parameters from the URL.
 * - Router: To perform navigation based on the outcome of operations.
 * - ConfirmNewEmailService: A service that handles token validation, email change confirmation, and cancellation.
 */
@Component({
  selector: 'app-confirm-new-email',
  templateUrl: './confirmNewEmail.component.html',
  standalone: false,
  styleUrls: ['./confirmNewEmail.component.css'],
})
export class ConfirmNewEmailComponent implements OnInit {
  token: string | null = null; // Token JWT recebido
  errorMessage: string = ''; // Mensagem de erro
  successMessage: string = ''; // Mensagem de sucesso
  showModal: boolean = false; // Controla a exibição do modal
  modalMessage: string = ''; // Mensagem do modal
  modalInfo: string = ''; // Mensagem do modal

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ConfirmNewEmailService: ConfirmNewEmailService
  ) { }

  /**
 * @method ngOnInit
 * @description
 * Lifecycle hook that is executed after the component is initialized.
 * It subscribes to the route query parameters to capture the JWT token from the URL.
 * If a token is present, it proceeds to validate the token; otherwise, it redirects the user to the homepage.
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
 * Calls the ConfirmNewEmailService to validate the provided token.
 * If the token is invalid or expired, an error modal is displayed with an appropriate message.
 *
 * @param token - The JWT token to be validated.
 */
  validateToken(token: string): void {
    this.ConfirmNewEmailService.validateToken(token).subscribe(
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
 * @method ConfirmNewEmailChange
 * @description
 * Executes the confirmation of the email change by calling the confirmNewEmailChange method in ConfirmNewEmailService.
 * Displays a modal with the outcome message, and after a brief delay, closes the modal and navigates accordingly.
 */
  ConfirmNewEmailChange(): void {
    if (this.token != null) {
      this.ConfirmNewEmailService.confirmNewEmailChange(this.token).subscribe(
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
        });}
  }

  /**
 * @method CancelNewEmailChange
 * @description
 * Cancels the email change process by calling the cancelNewEmailChange method in ConfirmNewEmailService.
 * Displays a modal with the outcome message, then closes the modal and navigates to the change email cancellation page.
 */
  CancelNewEmailChange(): void {
    this.modalInfo = 'Success';
    if (this.token != null) {
      this.ConfirmNewEmailService.cancelNewEmailChange(this.token).subscribe(
        (response) => {
          this.showErrorModal(response.message);
          setTimeout(() => {
            this.closeModal();
          }, 1500);

          setTimeout(() => {
            this.router.navigate(['/changeEmailCancellation']);
          }, 2000);
        }, (error) => {
          if (error.error?.message === 'Token expired.') {
            this.modalInfo = 'Error';
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

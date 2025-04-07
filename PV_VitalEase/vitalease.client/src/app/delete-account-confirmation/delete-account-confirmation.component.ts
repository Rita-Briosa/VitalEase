import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { MyProfileService } from '../services/myProfile.service';

/**
 * @component DeleteAccountConfirmationComponent
 * @description
 * The DeleteAccountConfirmationComponent manages the confirmation process for a user's account deletion request.
 * It retrieves a token from the URL query parameters, validates the token using the MyProfileService, and then
 * either confirms or cancels the account deletion based on the user's actions. Modal dialogs are used to display
 * error messages, and the user is redirected accordingly.
 *
 * @dependencies
 * - ActivatedRoute: Extracts query parameters from the URL.
 * - Router: Navigates between routes based on the outcome.
 * - MyProfileService: Provides methods for token validation, account deletion, and deletion cancellation.
 *
 * @usage
 * This component is not standalone and relies on external templates and styles:
 * - Template: './delete-account-confirmation.component.html'
 * - Styles: ['./delete-account-confirmation.component.css']
 */
@Component({
  selector: 'app-delete-account-confirmation',
  standalone: false,
  templateUrl: './delete-account-confirmation.component.html',
  styleUrls: ['./delete-account-confirmation.component.css']
})
export class DeleteAccountConfirmationComponent implements OnInit {

  token: string = '';
  errorMessage: string = '';
  successMessage: string = '';
  showModal: boolean = false;
  modalMessage: string = '';
  modalInfo: string = '';
  email: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private myProfileService: MyProfileService
  ) { }

  /**
 * @method ngOnInit
 * @description
 * Lifecycle hook that initializes the component. It subscribes to query parameters in order to extract the token.
 * If a token is present, it validates it; if not, it sets an error message and redirects the user to the homepage.
 */
  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.token = params['token'];

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
 * Validates the provided token by calling the MyProfileService. If the token is valid,
 * the associated email is stored. If the token is expired or invalid, an error modal is displayed,
 * and the user is redirected to the homepage after a brief delay.
 *
 * @param token - The JWT token to validate.
 */
  validateToken(token: string): void {
    this.myProfileService.validateToken(token).subscribe(
      (response) => {
        this.email = response.email;


      },
      (error) => {
        this.modalInfo = 'Error';
        if (error.error?.message === 'Token expired.') {
          this.showErrorModal('Token has expired. Please, request a new delete account link.');
          setTimeout(() => {
            this.router.navigate(['/']);
          }, 1500);
        } else {
          this.showErrorModal('Invalid token or an error occurred. Please, try again.');
          setTimeout(() => {
            this.router.navigate(['/']);
          }, 1500);
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
 * @description Closes the modal dialog.
 */
  closeModal(): void {
    this.showModal = false;
  }

  /**
 * @method ConfirmDeleteAccount
 * @description
 * Confirms the deletion of the user's account by calling the MyProfileService.
 * Upon successful deletion, a success message is displayed and the user is redirected to the homepage.
 * If an error occurs during deletion, an error modal is shown.
 */
  ConfirmDeleteAccount(): void {

      this.myProfileService.deleteUserAcc(this.email, this.token).subscribe({
        next: (response) => {
          this.successMessage = "Your account has been deleted.";
          setTimeout(() => {
            this.router.navigate(['/']);
          }, 1500);
        },
        error: (error) => {
          console.error('Error deleting account:', error);
          this.showErrorModal('Error deleting account.');
        }
      });
  }

  /**
   * @method CancelAccountDeletion
   * @description
   * Cancels the account deletion process by calling the MyProfileService.
   * Upon successful cancellation, a success message is displayed and the user is redirected to the homepage.
   * If an error occurs, an error modal is shown.
   */
  CancelAccountDeletion(): void {
    console.log(this.token);
    this.myProfileService.deleteAccountCancellation(this.token).subscribe({
      next: (response) => {
        console.log('Delete account cancellation successful:', response);
        this.successMessage = "Delete account cancellation successful.";
        setTimeout(() => {
          this.router.navigate(['/']);
        }, 1500);
      },
      error: (error) => {
        console.error('Error cancelling delete account:', error);
        this.showErrorModal('Error cancelling delete account.');
      }
    });
  }
}

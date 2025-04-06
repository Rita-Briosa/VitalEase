import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { MyProfileService } from '../services/myProfile.service';

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

  showErrorModal(message: string): void {
    this.modalMessage = message;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
  }

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

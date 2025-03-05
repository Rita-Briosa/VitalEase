import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ConfirmOldEmailService } from '../services/confirmOldEmail.service';

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

  showErrorModal(message: string): void {
    this.modalMessage = message;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
  }

}

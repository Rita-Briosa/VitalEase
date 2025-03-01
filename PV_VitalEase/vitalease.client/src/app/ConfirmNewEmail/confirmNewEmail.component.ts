import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ConfirmNewEmailService } from '../services/confirmNewEmail.service';

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

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ConfirmNewEmailService: ConfirmNewEmailService
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
    this.ConfirmNewEmailService.validateToken(token).subscribe(
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

  showErrorModal(message: string): void {
    this.modalMessage = message;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.router.navigate(['/']);
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }

}

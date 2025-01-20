import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ResetService } from '../services/reset-pass.service';

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

  calculatePasswordStrength(password: string): void {
    this.passwordStrength = this.getStrengthScore(password);
    this.passwordFeedback = this.getStrengthFeedback(this.passwordStrength);
  }

  private getStrengthScore(password: string): number {
    let score = 0;

    if (!password) return score;

    if (password.length >= 12) score += 25; // Comprimento mínimo
    if (/[a-z]/.test(password)) score += 25; // Letras minúsculas
    if (/[A-Z]/.test(password)) score += 25; // Letras maiúsculas
    if (/[@$!%*?&]/.test(password)) score += 25; // Caracteres especiais

    return score;
  }

  // Retorna feedback textual baseado na força
  private getStrengthFeedback(score: number): string {
    if (score < 50) return 'Weak';
    if (score < 75) return 'Moderate';
   return 'Strong'
  }

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

  showErrorModal(message: string): void {
    this.modalMessage = message;
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.router.navigate(['/']);
  }

}

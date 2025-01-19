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
            console.log('Password reset successful', response);
            setTimeout(() => {
              this.router.navigate(['/login']); // Redireciona após 2 segundos
            }, 2000);
          },
          (error) => {
            // Se ocorrer erro ao redefinir a senha
            // Aqui estamos tratando a mensagem de erro com base na resposta do servidor
            if (error.status === 400 || error.status === 401) {
              // Caso o servidor retorne uma mensagem de erro, usamos ela
              const serverErrorMessage = error.error?.message || 'Token inválido ou expirado. Tente novamente.';
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
    if (score < 25) return 'Muito Fraca';
    if (score < 50) return 'Fraca';
    if (score < 75) return 'Boa';
    return 'Forte';
  }
}

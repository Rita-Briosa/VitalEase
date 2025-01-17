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
  email: string | null = null; // Email do usuário
  newPassword: string = ''; // Nova senha
  confirmPassword: string = ''; // Confirmação da senha
  errorMessage: string = ''; // Mensagem de erro
  successMessage: string = ''; // Mensagem de sucesso

  constructor(private route: ActivatedRoute, private router: Router, private ResetService: ResetService) { }

  ngOnInit(): void {

    this.route.queryParams.subscribe(params => {
      this.email = params['email'].toString(); // Atribui o email

      // Verifique o valor do email
      console.log('Captured email:', this.email); // Verifica o valor do email

      // Verifica se os parâmetros estão corretos
      if (!this.email) {
        this.errorMessage = 'Invalid parameters in URL.';
      }
    });
  }

  onSubmit(): void {
    // Limpa mensagens anteriores
    this.errorMessage = '';
    this.successMessage = '';

    // Verifica se as senhas coincidem
    if (this.newPassword === this.confirmPassword) {
      if (this.email) {
        // Envia a requisição para o serviço de reset de senha
        this.ResetService.resetPassword(this.email, this.newPassword).subscribe(
          (response) => {
            // Se a senha for redefinida com sucesso
            this.successMessage = response.message;
            console.log('Password reset successful', response);
            setTimeout(() => {
              this.router.navigate(['/login']);  // Redireciona após 2 segundos
            }, 2000);
          },
          (error) => {
            // Se ocorrer erro ao redefinir a senha
            this.errorMessage = error.error.message || 'An error occurred. Please try again.';
          }
        );
      } else {
        this.errorMessage = 'Email is missing.';
      }
    } else {
      this.errorMessage = 'Passwords do not match.';
    }
  }
}

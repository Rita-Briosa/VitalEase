import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { ForgotService } from '../services/forgot.service';

@Component({
  selector: 'app-reset-pass',
  templateUrl: './reset-pass.component.html',
  standalone: false,
  styleUrls: ['./reset-pass.component.css'],
})
export class ResetPassComponent implements OnInit {
  id: number | null = null; // ID do usuário
  email: string | null = null; // Email do usuário
  newPassword: string = ''; // Nova senha
  confirmPassword: string = ''; // Confirmação da senha
  errorMessage: string = ''; // Mensagem de erro
  successMessage: string = ''; // Mensagem de sucesso

  constructor(private route: ActivatedRoute, private router: Router, private forgotService: ForgotService) { }

  ngOnInit(): void {
    // Captura os parâmetros da URL (ID e email)
    this.route.queryParams.subscribe(params => {
      this.id = +params['id']; // Converte o ID para número
      this.email = params['email']; // Atribui o email

      // Verifique o valor do email
      console.log('Captured email:', this.email); // Verifica o valor do email

      // Verifica se os parâmetros estão corretos
      if (!this.id || !this.email) {
        this.errorMessage = 'Invalid parameters in URL.';
      }
    });
  }

  onSubmit(): void {
    this.errorMessage = '';  // Limpa a mensagem de erro ao tentar submeter
    this.successMessage = '';  // Limpa a mensagem de sucesso

    if (this.newPassword === this.confirmPassword) {
      // Verifique se o email está disponível antes de chamar o serviço
      if (this.email) {
        console.log('Email being sent to resetPassword:', this.email);  // Verifica se o email está sendo enviado

        // Chama o serviço de redefinir senha e passa o email e a nova senha
        this.forgotService.resetPassword(this.email, this.newPassword).subscribe(
          response => {
            // Se a senha for redefinida com sucesso, exibe a mensagem e redireciona
            this.successMessage = response.message;  // Exibe a mensagem de sucesso
            setTimeout(() => {
              this.router.navigate(['/']);  // Redireciona após 2 segundos
            }, 2000);
          },
          error => {
            // Se ocorrer erro, exibe a mensagem de erro
            if (error.error && error.error.message) {
              this.errorMessage = error.error.message;  // Exibe a mensagem de erro do backend
            } else {
              this.errorMessage = 'An unexpected error occurred. Please try again later.';
            }
          }
        );
      } else {
        this.errorMessage = 'Email not found in URL.';  // Caso o email não tenha sido capturado da URL
      }
    } else {
      this.errorMessage = 'Passwords do not match.';  // Se as senhas não coincidirem
    }
  }
}

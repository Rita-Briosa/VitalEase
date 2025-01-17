import { Component } from '@angular/core';
import { ForgotService } from '../services/forgot.service'; // Ajuste o caminho, se necessário

@Component({
  selector: 'app-forgot-pass',
  standalone: false,
  templateUrl: './forgot-pass.component.html',
  styleUrls: ['./forgot-pass.component.css']
})
export class ForgotPassComponent {
  email: string = '';
  errorMessage: string = '';  // Variável para mensagens de erro
  successMessage: string = ''; // Variável para mensagens de sucesso

  constructor(private forgotService: ForgotService) { }

  forgotPassword() {
    // Limpar mensagens anteriores
    this.errorMessage = '';
    this.successMessage = '';

    // Enviar a solicitação para o serviço
    this.forgotService.ForgotPassword(this.email).subscribe({
      next: (response) => {
        // Verifique a resposta e defina a mensagem de sucesso
        console.log('Sucesso:', response); // Verifique o que vem da resposta
        this.successMessage = response.message; // Exibe a mensagem de sucesso
      },
      error: (err) => {
        // Caso ocorra um erro, exiba a mensagem de erro
        console.log('Erro:', err); // Verifique o que vem do erro
        this.errorMessage = err.error.message || 'Falha ao enviar o e-mail de redefinição de senha.';
      }
    });
  }
}

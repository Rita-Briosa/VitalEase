import { Component } from '@angular/core';
import { RegisterService } from '../services/register.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  username: string = '';
  birthDate: Date = new Date();
  email: string = '';
  height: number = 90;
  weight: number = 30;
  gender: string = '';
  password: string = '';
  confirmPassword: string = '';
  errorMessage: string = '';
  heartProblems: boolean = false;
  successMessage: string = ''; // Adicionado para exibir sucesso

  constructor(private registerService: RegisterService, private router: Router) { }

  onSubmit() {
    this.errorMessage = ''; // Limpa mensagens antigas
    this.successMessage = '';

 

    // Passando os dados formatados para o serviço
    this.registerService.register(
      this.username,
     this.birthDate,  // A data formatada
      this.email,
      this.height,
      this.weight,
      this.gender,
      this.password,
      this.heartProblems
    ).subscribe(
      (response: any) => {
        this.clearForm();
        console.log('Register successful', response);
        this.successMessage = response.message;
        setTimeout(() => {
          this.router.navigate(['/login']); // Redireciona após 2 segundos
        }, 2000);
       
      },
      (error) => {
        this.errorMessage = error.error?.message || 'An unexpected error occurred';
        console.log('Register error', error);
      }
    );
  }

  clearForm() {
    this.username = '';
    this.birthDate = new Date();
    this.email = '';
    this.height = 90;
    this.weight = 30;
    this.gender = '';
    this.password = '';
    this.confirmPassword = '';
    this.heartProblems = false;
  }

}

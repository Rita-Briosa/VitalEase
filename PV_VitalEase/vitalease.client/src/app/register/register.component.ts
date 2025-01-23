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
  errorMessage: string = '';
  heartProblems: boolean = false;
  successMessage: string = ''; // Adicionado para exibir sucesso

  constructor(private registerService: RegisterService, private router: Router) { }

  onSubmit() {
    this.errorMessage = ''; // Limpa mensagens antigas
    this.successMessage = '';

    this.registerService.register(this.username,this.birthDate,this.email,this.height,this.weight,this.gender, this.password, this.heartProblems).subscribe(
      (response: any) => {
        console.log('Register successful', response);
        this.successMessage = response.message;
        setTimeout(() => {
          this.router.navigate(['/login']); // Redireciona após sucesso
        }, 3000);
      },
      (error) => {
        this.errorMessage = error.error?.message || 'An unexpected error occurred';
        console.log('Register error', error);
      }
    );
  }
}

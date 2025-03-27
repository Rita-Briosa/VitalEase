import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { TrainingRoutinesService } from '../services/training-routines.service';

@Component({
  selector: 'app-custom-training-routines',
  standalone: false,
  
  templateUrl: './custom-training-routines.component.html',
  styleUrl: './custom-training-routines.component.css'
})
export class CustomTrainingRoutinesComponent {
  errorMessage: string = '';

  userInfo: any = null;
  isLoggedIn: boolean = false;
  routines: any = [];

  constructor(private authService: AuthService, private routinesService: TrainingRoutinesService, private router: Router) { }

  ngOnInit() {
    // Check if user is logged in by fetching the user info
    /* this.isLoggedIn = this.authService.isLoggedIn();  // A verificação da sessão agora é feita com o método isLoggedIn()
     if (this.isLoggedIn) {
       this.userInfo = this.authService.getUserInfo();  // Se estiver logado, pega as informações do usuário
     }
   }*/

    const token = this.authService.getSessionToken();

    if (token) {
      this.authService.validateSessionToken().subscribe(
        (response: any) => {
          this.isLoggedIn = true;
          this.userInfo = response.user;

          this.getRoutines();
        },
        (error) => {
          this.authService.logout();
          this.router.navigate(['/login']);
        }
      );
    } else {
      //No token found, redirect to login
      this.router.navigate(['/login']);
    }

  }

  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }

  getRoutines() {
    this.routinesService.getCustomTrainingRoutines(this.userInfo.id).subscribe(
      (response: any) => {
        this.routines = response;
        console.log("Custom Routines loaded successfully!");
      },
      (error: any) => {
        this.errorMessage = error.error?.message; // Define a mensagem de erro se a requisição falhar
        console.log('Error loading Custom Routines:', error);
      }
    );
  }


}

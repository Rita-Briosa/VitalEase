import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-custom-training-routines',
  standalone: false,
  
  templateUrl: './custom-training-routines.component.html',
  styleUrl: './custom-training-routines.component.css'
})
export class CustomTrainingRoutinesComponent {
  userInfo: any = null;
  isLoggedIn: boolean = false;

  constructor(private authService: AuthService, private router: Router) { }

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
        },
        (error) => {
          this.authService.logout();
          this.router.navigate(['/login']);
        }
      );
    } else {
      // No token found, redirect to login
      //this.router.navigate(['/login']);
    }

  }
  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }
}

import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-about-us',
  standalone: false,
  
  templateUrl: './about-us.component.html',
  styleUrl: './about-us.component.css'
})
export class AboutUsComponent {
  userInfo: any = null;
  isLoggedIn: boolean = false;

  constructor(private authService: AuthService, private router: Router) { }

  ngOnInit() {
    // Check if user is logged in by fetching the user info
    this.isLoggedIn = this.authService.isLoggedIn();  // A verificação da sessão agora é feita com o método isLoggedIn()
    if (this.isLoggedIn) {
      this.userInfo = this.authService.getUserInfo();  // Se estiver logado, pega as informações do usuário
    }
  }

  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }
}

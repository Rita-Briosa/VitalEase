import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-homePage',
  templateUrl: './homePage.component.html',
  standalone: false,
  styleUrls: ['./homePage.component.css']
})
export class HomePageComponent implements OnInit {

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

  // Optional: You can implement a logout function to clear localStorage and update the view
  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }

  title = 'vitalease.client';
}

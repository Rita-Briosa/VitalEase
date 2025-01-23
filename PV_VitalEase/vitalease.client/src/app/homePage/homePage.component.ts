import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-homePage',
  templateUrl: './homePage.component.html',
  standalone: false,
  styleUrls: ['./homePage.component.css']
})
export class HomePageComponent implements OnInit {


  userInfo: any = null;
  isLoggedIn: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private http: HttpClient)
  { }

  ngOnInit() {

    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/']);
      return;
    }

    // Check if user is logged in by fetching the user info
    this.isLoggedIn = this.authService.isLoggedIn();  // A verificação da sessão agora é feita com o método isLoggedIn()
    if (this.isLoggedIn) {
      this.userInfo = this.authService.getUserInfo();  // Se estiver logado, pega as informações do usuário
    }
  }

  title = 'vitalease.client';
}

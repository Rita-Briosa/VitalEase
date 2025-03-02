import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-change-email-cancellation',
  templateUrl: './changeEmailCancellation.component.html',
  standalone: false,
  styleUrls: ['./changeEmailCancellation.component.css']
})
export class ChangeEmailCancellationComponent implements OnInit {


  userInfo: any = null;
  isLoggedIn: boolean = false;
  isAdmin: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private http: HttpClient) { }

  ngOnInit() {

    const token = this.authService.getSessionToken();

    if (token) {
      this.authService.validateSessionToken().subscribe(
        (response: any) => {
          this.isLoggedIn = true;
          this.userInfo = response.user;
          if (this.userInfo.type === 1) {
            this.isAdmin = true;
          } else {
            this.isAdmin = false;
          }

        },
        (error) => {
          this.authService.logout();
          this.router.navigate(['/login']);
        }
      );
    } else {
      // No token found, redirect to login
      this.router.navigate(['/login']);
    }
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/']);
      return;
    }
  }

}

import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

/**
 * @component HomePageComponent
 * @description
 * The HomePageComponent serves as the landing page of the application. It validates the user's session by checking for a token
 * and then retrieves the user's information. Based on the user's type, it determines if the user has administrative privileges.
 * If no valid session is found, the user is redirected to the login page.
 *
 * @dependencies
 * - AuthService: Provides methods for session token retrieval, validation, and user logout.
 * - Router: Handles navigation between different routes in the application.
 * - HttpClient: Used for making HTTP requests as needed.
 *
 * @usage
 * This component is not standalone and relies on external templates and styles:
 * - Template: './homePage.component.html'
 * - Styles: ['./homePage.component.css']
 */
@Component({
  selector: 'app-homePage',
  templateUrl: './homePage.component.html',
  standalone: false,
  styleUrls: ['./homePage.component.css']
})
export class HomePageComponent implements OnInit {


  userInfo: any = null;
  isLoggedIn: boolean = false;
  isAdmin: boolean = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private http: HttpClient)
  { }

  /**
 * @method ngOnInit
 * @description
 * Lifecycle hook that is called after the component is initialized.
 * It validates the session token. If valid, it retrieves the user's information and sets the admin flag.
 * If the token is invalid or missing, the user is redirected to the login page.
 */
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

  /**
 * @method goToDashboard
 * @description Navigates the user to the dashboard.
 */
  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  title = 'vitalease.client';
}

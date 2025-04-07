import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

/**
 * @component AboutUsComponent
 * @description
 * The AboutUsComponent manages the "About Us" section of the application.
 * It verifies the user's session on initialization, retrieves user information,
 * and determines whether the user is an admin.
 *
 * If a valid session token is found, it updates the component state with the user information.
 * If the session token is invalid or expired, the user is logged out and redirected to the login page.
 *
 * @property userInfo - Stores user details retrieved after successful session validation.
 * @property isLoggedIn - Boolean flag indicating whether the user is logged in.
 * @property isAdmin - Boolean flag indicating if the logged-in user has administrative privileges.
 *
 * @method ngOnInit
 * The lifecycle hook that executes when the component is initialized.
 * It checks for a session token, validates it, and updates user details and admin status accordingly.
 *
 * @method logout
 * Logs the user out by clearing the session and navigates to the homepage.
 *
 * @method goToDashboard
 * Navigates the user to the dashboard page.
 *
 * @dependencies
 * - AuthService: Provides methods for session token retrieval, validation, and logout operations.
 * - Router: Used for navigation between different application routes.
 */
@Component({
  selector: 'app-about-us',
  standalone: false,
  
  templateUrl: './about-us.component.html',
  styleUrl: './about-us.component.css'
})
export class AboutUsComponent {
  userInfo: any = null;
  isLoggedIn: boolean = false;
  isAdmin: boolean = false;
  constructor(private authService: AuthService, private router: Router) { }

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
      //this.router.navigate(['/login']);
    }

  }
  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }
  
}

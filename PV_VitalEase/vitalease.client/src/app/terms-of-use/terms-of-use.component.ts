import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

/**
 * @component TermsOfUseComponent
 * @description
 * The TermsOfUseComponent is responsible for displaying the application's Terms of Use.
 * It validates the user's session token upon initialization to determine if the user is logged in and
 * whether they have administrative privileges. The component also provides functions for logging out
 * and navigating to the dashboard.
 *
 * @dependencies
 * - AuthService: Handles session token management, validation, and user logout.
 * - Router: Facilitates navigation between different application routes.
 *
 * @usage
 * This component is not standalone and uses external templates and styles:
 *   - Template: './terms-of-use.component.html'
 *   - Styles: './terms-of-use.component.css'
 */
@Component({
  selector: 'app-terms-of-use',
  standalone: false,
  
  templateUrl: './terms-of-use.component.html',
  styleUrl: './terms-of-use.component.css'
})
export class TermsOfUseComponent {

  userInfo: any = null;
  isLoggedIn: boolean = false;
  isAdmin: boolean = false;
  constructor(private authService: AuthService, private router: Router) { }

  /**
   * @method ngOnInit
   * @description
   * Lifecycle hook that is called after the component is initialized.
   * It retrieves and validates the session token. If a valid token is found, it updates the component's state
   * with the user's information and determines the user's administrative status. If the token is invalid,
   * the user is logged out and redirected to the login page.
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
      //this.router.navigate(['/login']);
    }

  }

  /**
   * @method logout
   * @description
   * Logs out the user by calling the AuthService logout method and navigates to the home page.
   */
  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }

  /**
   * @method goToDashboard
   * @description Navigates the user to the dashboard page.
   */
  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }
}

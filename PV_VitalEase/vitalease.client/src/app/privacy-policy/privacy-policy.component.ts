import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

/**
 * @component PrivacyPolicyComponent
 * @description
 * The PrivacyPolicyComponent is responsible for displaying the application's privacy policy.
 * It validates the user's session token upon initialization to determine the user's authentication
 * status and administrative privileges. This component also provides functionality for logging out
 * and navigating to the dashboard.
 *
 * @dependencies
 * - AuthService: Provides methods for retrieving and validating the session token as well as handling logout operations.
 * - Router: Facilitates navigation between different routes within the application.
 *
 * @usage
 * This component is not standalone and uses external templates and styles:
 *   - Template: './privacy-policy.component.html'
 *   - Styles: './privacy-policy.component.css'
 */
@Component({
  selector: 'app-privacy-policy',
  standalone: false,
  
  templateUrl: './privacy-policy.component.html',
  styleUrl: './privacy-policy.component.css'
})
export class PrivacyPolicyComponent {

  userInfo: any = null;
  isLoggedIn: boolean = false;
  isAdmin: boolean = false;
  constructor(private authService: AuthService, private router: Router) { }

  /**
   * @method ngOnInit
   * @description
   * Lifecycle hook that is called after the component has been initialized.
   * It retrieves and validates the session token. If the token is valid, it updates the
   * component state with the user's information and sets the administrative flag based on the user's type.
   * If the token is invalid, the user is logged out and redirected to the login page.
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
   * Logs out the user by calling the AuthService and navigates the user to the home page.
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

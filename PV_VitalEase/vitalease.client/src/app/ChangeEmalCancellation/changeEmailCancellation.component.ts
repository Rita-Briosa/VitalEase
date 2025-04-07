import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

/**
 * @component ChangeEmailCancellationComponent
 * @description
 * The ChangeEmailCancellationComponent handles the cancellation of a user's email change process.
 * It validates the current user session on initialization and ensures that the user is authenticated.
 * If a valid session token is present, the user's details are retrieved and the component state is updated;
 * otherwise, the user is redirected to the login page.
 *
 * @dependencies
 * - AuthService: Provides methods for retrieving and validating session tokens, as well as logging out.
 * - Router: Handles navigation between routes.
 * - HttpClient: Facilitates HTTP requests when necessary.
 *
 * @usage
 * This component is not standalone and uses the template and styles provided in:
 * - './changeEmailCancellation.component.html'
 * - './changeEmailCancellation.component.css'
 */
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

  /**
* @method ngOnInit
* @description
* Lifecycle hook that is called after the component is initialized.
* It retrieves and validates the session token. If the token is valid, it updates the component
* state with the user's information and administrative status. If no valid token is found, the user is redirected to the login page.
* Additionally, if the user is not authenticated, they are redirected to the homepage.
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

}

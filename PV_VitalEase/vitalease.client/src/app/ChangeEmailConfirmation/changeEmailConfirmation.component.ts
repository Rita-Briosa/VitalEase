import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

/**
 * @component ChangeEmailConfirmationComponent
 * @description
 * The ChangeEmailConfirmationComponent is responsible for handling the confirmation of a user's email change.
 * It validates the user's session token on initialization and updates the component's state with the user's information,
 * including whether the user has administrative privileges.
 *
 * If a valid session token is found, the component retrieves the user's details; if not, the user is redirected to the login page.
 *
 * @dependencies
 * - AuthService: Provides methods for session token retrieval and validation.
 * - Router: Handles navigation between different application routes.
 * - HttpClient: Facilitates HTTP requests as needed.
 *
 * @example
 * This component is not standalone and uses the template and styles provided in:
 * - './changeEmailConfirmation.component.html'
 * - './changeEmailConfirmation.component.css'
 */
@Component({
  selector: 'app-change-email-confirmation',
  templateUrl: './changeEmailConfirmation.component.html',
  standalone: false,
  styleUrls: ['./changeEmailConfirmation.component.css']
})
export class ChangeEmailConfirmationComponent implements OnInit {


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
* Lifecycle hook that is called after the component has been initialized.
* It retrieves the current session token, validates it, and updates the component state with the user's details.
* If the token is valid, the user's information is stored and admin status is determined.
* If the token is invalid or missing, the user is logged out and redirected to the login page.
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
  }

}

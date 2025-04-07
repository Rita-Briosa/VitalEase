import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

/**
 * @component LoginComponent
 * @description
 * The LoginComponent handles user authentication by providing a login form where users can enter their email,
 * password, and choose to be remembered. Upon form submission, it calls the AuthService to authenticate the user.
 * If the login is successful, a session token is stored and the user is redirected based on their user type.
 * If an error occurs, an appropriate error message is displayed.
 *
 * @dependencies
 * - AuthService: Handles user authentication, token management, and session storage.
 * - Router: Manages navigation to different routes based on user type and authentication status.
 *
 * @usage
 * This component is not standalone and uses external templates and styles:
 * - Template: './login.component.html'
 * - Styles: ['./login.component.css']
 */
@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  email: string = '';
  password: string = '';
  errorMessage: string = '';
  rememberMe: boolean = false; // Checkbox "Remember Me"

  constructor(private authService: AuthService, private router: Router) { }

  /**
  * @method onSubmit
  * @description
  * Called when the login form is submitted. It uses the AuthService to attempt to log in with the provided email,
  * password, and rememberMe flag. On a successful login, it stores the session token and redirects the user based on their user type.
  * If the login fails, an error message is displayed.
  */
  onSubmit() {
    this.authService.login(this.email, this.password, this.rememberMe).subscribe(
      (response: any) => {
        console.log('Login successful', response);

        this.authService.setSessionToken(response.token);
        this.redirectBasedOnUserType(response.user.type);
      },
      (error) => {
           this.errorMessage = error.error?.message || 'An unexpected error occurred';
      console.log('Login error', error);
      }
    );
  }

  /**
   * @method redirectBasedOnUserType
   * @description
   * Redirects the user to an appropriate page based on their user type.
   *
   * @param userType - The type of the user (e.g., 0 for standard users, 1 for administrators).
   * If the user type is 0, the user is redirected to the home page.
   * If the user type is 1, the user is redirected to the dashboard.
   * If the user type is not recognized, the user is redirected to the home page.
   */
  private redirectBasedOnUserType(userType: number): void {
    if (userType === 0) {
      // Tipo de usuário 0: Redireciona para a home page
      this.router.navigate(['/']);
    } else if (userType === 1) {
      // Tipo de usuário 1: Redireciona para a dashboard
      this.router.navigate(['/dashboard']);
    } else {
      // Redireciona para uma página padrão caso o tipo de usuário não seja reconhecido
      console.log('Unknown user type');
      this.router.navigate(['/']);
    }
  }
}


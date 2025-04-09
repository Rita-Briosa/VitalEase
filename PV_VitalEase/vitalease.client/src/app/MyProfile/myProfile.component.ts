import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { MyProfileService } from '../services/myProfile.service'
import { Router } from '@angular/router';
import { AbstractControl, FormControl, FormGroup, Validators } from '@angular/forms';

/**
 * @component MyProfileComponent
 * @description
 * The MyProfileComponent is responsible for displaying and managing a user's profile information.
 * It allows users to view their current profile details (username, birth date, height, weight, gender, email, heart health status)
 * and update these details through various operations, such as changing the username, birth date, weight, height, gender,
 * and password. The component also provides functionality to initiate account deletion and change the email address.
 *
 * It includes form validation using Angular Reactive Forms with custom validators (e.g. for email).
 * Additionally, it offers password strength calculation and toggles for showing/hiding password fields.
 *
 * @dependencies
 * - AuthService: Provides session token management and authentication validation.
 * - MyProfileService: Handles API requests for retrieving and updating user profile information.
 * - Router: Manages navigation between routes.
 *
 * @usage
 * This component is not standalone and uses external templates and styles:
 *   - Template: './myProfile.component.html'
 *   - Styles: ['./myProfile.component.css']
 */
@Component({
  selector: 'app-myProfile',
  standalone: false,
  templateUrl: './myProfile.component.html',
  styleUrls: ['./myProfile.component.css']
})
export class MyProfileComponent {
  username: string = '';
  birthDate: string = '';
  weight: number = 30;
  height: number = 90;
  gender: string = '';
  email: string = '';
  hasHeartProblems: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';
  isLoggedIn: boolean = false;
  userInfo: any = null;
  activeModal: string | null = null;
  newUsername: string = '';
  newBirthDate: string = '';
  newWeight: number = 30;
  newHeight: number = 90;
  newGender: string = '';
  oldPassword: string = '';
  newPassword: string = '';
  newEmail: string = '';
  newHasHeartProblems: boolean = false;
  showPassword: boolean = false;
  showOldPassword: boolean = false;
  showNewPassword: boolean = false;
  showConfirmPassword: boolean = false;
  confirmPassword: string = '';
  passwordStrength: number = 0; // Valor numérico da força da senha
  passwordFeedback: string = '';
  password: string = '';
  isConfirmed: boolean = false;

  constructor(private authService: AuthService, private router: Router, private profileService: MyProfileService) { }

  form: FormGroup = new FormGroup({
    email: new FormControl("", [Validators.required, this.customEmailValidator]),
    password: new FormControl("")
  })

  /**
 * @method getError
 * @description
 * Returns the appropriate error message for a given form control.
 *
 * @param control - The form control to check for errors.
 * @returns A string containing the error message.
 */
  getError(control: any): string {
    if (control.errors?.required && control.touched)
      return 'This field is required!';
    else if (control.errors?.emailError && control.touched)
      return 'Please enter valid email address!';
    else return '';
  }

  /**
   * @method customEmailValidator
   * @description
   * Custom validator for validating an email address against a specific regex pattern.
   *
   * @param control - The form control containing the email value.
   * @returns An object with the error key if invalid, or null if valid.
   */
  customEmailValidator(control: AbstractControl) {
    const pattern = /^[\w.-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,20}$/;
    const value = control.value;
    if (!pattern.test(value) && control.touched)
      return {
        emailError: true
      }
    else return null;
  }

  /**
   * @method ngOnInit
   * @description
   * Lifecycle hook called after component initialization.
   * It validates the user's session token and retrieves the user's profile information.
   * If the token is invalid or missing, the user is redirected to the login page.
   */
  ngOnInit() {

    const token = this.authService.getSessionToken();

    if (token) {
      this.authService.validateSessionToken().subscribe(
        (response: any) => {
          this.isLoggedIn = true;
          this.userInfo = response.user;
          this.email = this.userInfo.email;
          this.getProfile();
        },
        (error) => {
          this.authService.logout();
          this.router.navigate(['/login']);
        }
      );
    } else {
      this.router.navigate(['/login']);
    }

    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/']);
      return;
    }
  }

  /**
   * @method getProfile
   * @description
   * Retrieves the profile information of the user based on their email by calling the MyProfileService.
   * The received data is then used to update the component's profile properties.
   */
  getProfile(): void {
    this.profileService.getProfileInfo(this.email).subscribe(
      (response: any) => {
        this.username = response.username;
        this.newUsername = this.username;
        this.birthDate = response.birthDate;
        this.newBirthDate = this.birthDate;
        this.height = response.height;
        this.newHeight = this.height;
        this.weight = response.weight;
        this.newWeight = this.weight;
        this.gender = response.gender;
        this.newGender = this.gender;
        this.hasHeartProblems = response.hasHeartProblems;
        this.newHasHeartProblems = this.hasHeartProblems;
      },
      (error: any) => {
        this.errorMessage = error.error?.message;
        this.openModal('error');
      }
    );
  }

  ////////////////////////////////////////
  /**
   * @method confirmDeleteAccount
   * @description
   * Prompts the user for confirmation before proceeding with account deletion.
   * If confirmed, calls the accDeleteConfirmation method.
   */
  confirmDeleteAccount() {
    if (confirm('Are you sure you want to delete your account? This action is irreversible!')) {
      this.accDeleteConfirmation(this.password);
    }
  }

  /**
   * @method accDeleteConfirmation
   * @description
   * Validates the user's password by calling the MyProfileService. If the password is correct,
   * it proceeds to request account deletion.
   *
   * @param password - The password to validate.
   */
  accDeleteConfirmation(password: string): void {
    // Pass both email and password to the validatePassword method
    this.profileService.validatePassword(this.email, password).subscribe( // Pass email and password
      (response: any) => {
        this.successMessage = response.message;
        this.errorMessage = '';

        // If password validation is successful, delete the account
        this.deleteAccountRequest();
      },
      (error: any) => {
        this.errorMessage = error.error?.message;
        this.successMessage = '';
      }
    );
  }

  /**
   * @method deleteAccountRequest
   * @description
   * Initiates an account deletion request by calling the MyProfileService.
   * On success, notifies the user that an email has been sent to complete the deletion process.
   */
  deleteAccountRequest(): void {

    this.profileService.deleteAccountRequest(this.email).subscribe(
      (response: any) => {
        this.successMessage = response.message;
        this.errorMessage = '';
        setTimeout(() => {
          this.closeModal(); // Redireciona após 2 segundos
        }, 2000);
        alert("Has been sent an email to complete the deletion of your account");
      }, (error: any) => {
        this.errorMessage = error.error?.message;
        this.successMessage = '';
      });
  }

  ////////////////////////////////////////////////
  /**
   * @method changeUsername
   * @description
   * Updates the user's username by calling the MyProfileService.
   *
   * @param username - The new username.
   */
  changeUsername(username: string): void {
    this.profileService.changeUsername(username, this.email).subscribe(
      (response: any) => {
        this.username = username;
        this.successMessage = response.message;
        this.errorMessage = '';
        setTimeout(() => {
          this.closeModal(); // Redireciona após 2 segundos
        }, 2000);
      }, (error: any) => {
        this.errorMessage = error.error?.message;
        this.successMessage = '';
      });
  }

  /**
   * @method changeBirthDate
   * @description
   * Updates the user's birth date by calling the MyProfileService.
   *
   * @param date - The new birth date.
   */
  changeBirthDate(date: string): void {
    this.profileService.changeBirthDate(date, this.email).subscribe(
      (response: any) => {
        this.birthDate = date;
        this.successMessage = response.message;
        this.errorMessage = '';
        setTimeout(() => {
          this.closeModal(); // Redireciona após 2 segundos
        }, 2000);
      }, (error: any) => {
        this.errorMessage = error.error?.message;
        this.successMessage = '';
    });
  }

  /**
   * @method changeWeight
   * @description
   * Updates the user's weight by calling the MyProfileService.
   *
   * @param weight - The new weight.
   */
  changeWeight(weight: number): void {
    this.profileService.changeWeight(weight, this.email).subscribe(
      (response: any) => {
        this.weight = weight;
        this.successMessage = response.message;
        this.errorMessage = '';
        setTimeout(() => {
          this.closeModal(); // Redireciona após 2 segundos
        }, 2000);
      }, (error: any) => {
        this.errorMessage = error.error?.message;
        this.successMessage = '';
      });
  }

  /**
   * @method changeHeight
   * @description
   * Updates the user's height by calling the MyProfileService.
   *
   * @param height - The new height.
   */
  changeHeight(height: number): void {
    this.profileService.changeHeight(height, this.email).subscribe(
      (response: any) => {
        this.height = height;
        this.successMessage = response.message;
        this.errorMessage = '';
        setTimeout(() => {
          this.closeModal(); // Redireciona após 2 segundos
        }, 2000);
      }, (error: any) => {
        this.errorMessage = error.error?.message;
        this.successMessage = '';
      });
  }

  /**
   * @method changeGender
   * @description
   * Updates the user's gender by calling the MyProfileService.
   *
   * @param gender - The new gender value.
   */
  changeGender(gender: string): void {
    if (gender.toString() === "male" || gender.toString() === "male") {
      gender = "Male";

} else
      if (gender.toString() === "female" || gender.toString() === "Female") {
        gender = "Female";
    }
    this.profileService.changeGender(gender, this.email).subscribe(
      (response: any) => {
        this.gender = gender;
        this.successMessage = response.message;
        this.errorMessage = '';
        setTimeout(() => {
          this.closeModal(); // Redireciona após 2 segundos
        }, 2000);
      }, (error: any) => {
        this.errorMessage = error.error?.message;
        this.successMessage = '';
      });
  }

  /**
   * @method changeHasHeartProblems
   * @description
   * Updates the user's heart health status by calling the MyProfileService.
   *
   * @param hasHeartProblems - The new heart health status.
   */
  changeHasHeartProblems(hasHeartProblems: boolean): void {

    if (hasHeartProblems.toString() === "true" || hasHeartProblems.toString() === "True") {
      hasHeartProblems = true;

    } else
      if (hasHeartProblems.toString() === "false" || hasHeartProblems.toString() === "False") {
        hasHeartProblems = false;
      }
    this.profileService.changeHasHeartProblems(hasHeartProblems, this.email).subscribe(
      (response: any) => {
        this.hasHeartProblems = hasHeartProblems;
        this.successMessage = response.message;
        this.errorMessage = '';
        setTimeout(() => {
          this.closeModal(); // Redireciona após 2 segundos
        }, 2000);
      }, (error: any) => {
        this.errorMessage = error.error?.message;
        this.successMessage = '';
      });
  }

  /**
   * @method changePassword
   * @description
   * Updates the user's password by calling the MyProfileService.
   * It first verifies that the new password matches the confirm password field.
   *
   * @param oldPassword - The current password.
   * @param newPassword - The new password.
   */
  changePassword(oldPassword: string, newPassword: string): void {
    if (this.newPassword === this.confirmPassword) {
      this.profileService.changePassword(oldPassword,newPassword, this.email).subscribe(
        (response: any) => {
          this.successMessage = response.message;
          this.errorMessage = '';
          setTimeout(() => {
            this.closeModal();
            this.logout();
          }, 2000);
        }, (error: any) => {
          this.errorMessage = error.error?.message;
          this.successMessage = '';
        });
    } else {
       this.errorMessage = 'Passwords do not match.';
    }
  }

  /**
   * @method changeEmail
   * @description
   * Initiates the email change process by calling the MyProfileService.
   *
   * @param password - The current password for verification.
   * @param newEmail - The new email address.
   */
  changeEmail(password: string, newEmail: string): void {

      this.profileService.changeEmail(password, this.email, newEmail).subscribe(
        (response: any) => {
          this.successMessage = response.message;
          this.errorMessage = '';
          setTimeout(() => {
            this.closeModal(); // Redireciona após 2 segundos
          }, 2000);
        }, (error: any) => {
          this.errorMessage = error.error?.message;
          this.successMessage = '';
        });
  }

  /**
   * @method togglePasswordVisibility
   * @description Toggles the visibility of the general password field.
   */
  togglePasswordVisibility() {
    this.showPassword = !this.showPassword; // Alterna entre true e false
  }

  /**
   * @method togglePasswordVisibilityOld
   * @description Toggles the visibility of the old password field.
   */
  togglePasswordVisibilityOld() {
    this.showOldPassword = !this.showOldPassword; // Alterna entre true e false
  }

  /**
   * @method togglePasswordVisibilityNew
   * @description Toggles the visibility of the new password field.
   */
  togglePasswordVisibilityNew() {
    this.showNewPassword = !this.showNewPassword; // Alterna entre true e false
  }

  /**
   * @method togglePasswordVisibilityConfirm
   * @description Toggles the visibility of the confirm password field.
   */
  togglePasswordVisibilityConfirm() {
    this.showConfirmPassword = !this.showConfirmPassword; // Alterna entre true e false
  }

  /**
   * @method calculatePasswordStrength
   * @description
   * Calculates the strength of a given password and sets the corresponding numeric score and textual feedback.
   *
   * @param password - The password to evaluate.
   */
  calculatePasswordStrength(password: string): void {
    this.passwordStrength = this.getStrengthScore(password);
    this.passwordFeedback = this.getStrengthFeedback(this.passwordStrength);
  }

  /**
  * @method getStrengthScore
  * @description
  * Calculates a numeric strength score for a password based on length, lowercase, uppercase, and special characters.
  *
  * @param password - The password to evaluate.
  * @returns A numeric score representing the strength of the password.
  */
  private getStrengthScore(password: string): number {
    let score = 0;

    if (!password) return score;

    if (password.length >= 12) score += 25; // Comprimento mínimo
    if (/[a-z]/.test(password)) score += 25; // Letras minúsculas
    if (/[A-Z]/.test(password)) score += 25; // Letras maiúsculas
    if (/[@$!%*?&]/.test(password)) score += 25; // Caracteres especiais

    return score;
  }

  /**
   * @method getStrengthFeedback
   * @description
   * Provides textual feedback based on the password strength score.
   *
   * @param score - The numeric strength score.
   * @returns A string indicating whether the password is Weak, Moderate, or Strong.
   */
  private getStrengthFeedback(score: number): string {
    if (score < 50) return 'Weak';
    if (score < 75) return 'Moderate';
    return 'Strong'
  }

  /**
   * @method openModal
   * @description
   * Opens a modal dialog for profile updates. It also initializes new profile values based on the current profile.
   *
   * @param field - The specific profile field/modal to open.
   */
  openModal(field: string) {
    this.activeModal = field; // Define qual modal será aberta
    this.newUsername = this.username;
    this.newBirthDate = this.birthDate;
    this.newHeight = this.height;
    this.newWeight = this.weight;
    this.newGender = this.gender;
    this.newHasHeartProblems = this.hasHeartProblems;
  }

  /**
   * @method closeModal
   * @description Closes any open modal dialog and resets feedback messages.
   */
  closeModal() {
    this.successMessage = '';
    this.errorMessage = '';
    this.activeModal = null; // Remove a modal ativa
  }

  /**
   * @method logout
   * @description Logs the user out by calling the AuthService and then navigates to the home page.
   */
  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }
}

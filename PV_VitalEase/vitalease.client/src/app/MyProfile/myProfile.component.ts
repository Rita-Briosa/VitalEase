import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { MyProfileService } from '../services/myProfile.service'
import { Router } from '@angular/router';

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

  constructor(private authService: AuthService, private router: Router, private profileService: MyProfileService) { }

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

  confirmDeleteAccount() {
    if (confirm('Are you sure you want to delete your account? This action is irreversible!')) {
      this.deleteAccount(this.email);
    }
  }
  deleteAccount(email: string) {
    this.profileService.deleteUserAcc(email).subscribe({
      next: (response) => {
        console.log('Account deleted:', response);
        alert('Conta eliminada com sucesso!');
      },
      error: (error) => {
        console.error('Error deleting account:', error);
        alert('Erro ao eliminar conta!');
      }
    });
  }

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
        this.errorMessage = error.message;
        this.successMessage = '';
    });
  }

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

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword; // Alterna entre true e false
  }

  togglePasswordVisibilityOld() {
    this.showOldPassword = !this.showOldPassword; // Alterna entre true e false
  }

  togglePasswordVisibilityNew() {
    this.showNewPassword = !this.showNewPassword; // Alterna entre true e false
  }

  togglePasswordVisibilityConfirm() {
    this.showConfirmPassword = !this.showConfirmPassword; // Alterna entre true e false
  }

  calculatePasswordStrength(password: string): void {
    this.passwordStrength = this.getStrengthScore(password);
    this.passwordFeedback = this.getStrengthFeedback(this.passwordStrength);
  }

  private getStrengthScore(password: string): number {
    let score = 0;

    if (!password) return score;

    if (password.length >= 12) score += 25; // Comprimento mínimo
    if (/[a-z]/.test(password)) score += 25; // Letras minúsculas
    if (/[A-Z]/.test(password)) score += 25; // Letras maiúsculas
    if (/[@$!%*?&]/.test(password)) score += 25; // Caracteres especiais

    return score;
  }

  // Retorna feedback textual baseado na força
  private getStrengthFeedback(score: number): string {
    if (score < 50) return 'Weak';
    if (score < 75) return 'Moderate';
    return 'Strong'
  }

  // Função para abrir a modal específica
  openModal(field: string) {
    this.activeModal = field; // Define qual modal será aberta
    this.newUsername = this.username;
    this.newBirthDate = this.birthDate;
    this.newHeight = this.height;
    this.newWeight = this.weight;
    this.newGender = this.gender;
    this.newHasHeartProblems = this.hasHeartProblems;
  }

  // Função para fechar a modal
  closeModal() {
    this.successMessage = '';
    this.errorMessage = '';
    this.activeModal = null; // Remove a modal ativa
  }


  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }
}

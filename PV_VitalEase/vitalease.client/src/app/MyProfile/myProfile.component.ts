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
  newPassword: string = '';
  newEmail: string = '';
  newHasHeartProblems: boolean = false;

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
        console.error('Erro ao procurar perfil:');

      }
    );
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

    if (hasHeartProblems.toString() === "yes" || hasHeartProblems.toString() === "Yes") {
      hasHeartProblems = true;

    } else
      if (hasHeartProblems.toString() === "No" || hasHeartProblems.toString() === "no") {
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

  // Função para abrir a modal específica
  openModal(field: string) {
    this.activeModal = field; // Define qual modal será aberta
  }

  // Função para fechar a modal
  closeModal() {
    this.activeModal = null; // Remove a modal ativa
    this.successMessage = '';
    this.errorMessage = '';
  }

  // Função para salvar as mudanças
  saveChanges(field: string) {
    console.log(`Saving ${field} changes`);
    this.closeModal(); // Fecha a modal após salvar
  }

  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }
}

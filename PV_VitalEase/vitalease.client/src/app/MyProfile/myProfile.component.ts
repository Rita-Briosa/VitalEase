import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
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
  hasHeartProblems: boolean = false;
  errorMessage: string = '';
  isLoggedIn: boolean = false;
  userInfo: any = null;
  activeModal: string | null = null; // A variável que armazena a modal ativa

  constructor(private authService: AuthService, private router: Router) { }

  ngOnInit() {
    const token = this.authService.getSessionToken();

    if (token) {
      this.authService.validateSessionToken().subscribe(
        (response: any) => {
          this.isLoggedIn = true;
          this.userInfo = response.user;
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

  // Função para abrir a modal específica
  openModal(field: string) {
    this.activeModal = field; // Define qual modal será aberta
  }

  // Função para fechar a modal
  closeModal() {
    this.activeModal = null; // Remove a modal ativa
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

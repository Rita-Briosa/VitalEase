import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { TrainingRoutinesService } from '../services/training-routines.service';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-manage-training-routines',
  templateUrl: './manage-training-routines.component.html',
  standalone: false,
  styleUrls: ['./manage-training-routines.component.css']
})
export class ManageTrainingRoutinesComponent implements OnInit {
  filters: any = {
    type: '',
    difficultyLevel: null,
    muscleGroup: '',
    equipmentNeeded: '',
  };

  userInfo: any = null;
  isLoggedIn: boolean = false;
  isAdmin: boolean = false;
  routines: any[] = []; // Array para armazenar as routines
  errorMessage: string = '';


  constructor(
    private trainingRoutinesService: TrainingRoutinesService,
    private authService: AuthService,
    private router: Router,
    private http: HttpClient) { }

  ngOnInit() {

    const token = this.authService.getSessionToken();

    if (token) {
      this.authService.validateSessionToken().subscribe(
        (response: any) => {
          this.isLoggedIn = true;
          this.userInfo = response.user;
          this.getRoutines();
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

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  getRoutines(): void {
    this.trainingRoutinesService.getRoutines(this.userInfo.id).subscribe(
      (response: any) => {
        this.routines = response; // Armazena as routines na variável 'routines'
        console.log('Routines loaded successfully:', this.routines);
      },
      (error: any) => {
        this.errorMessage = 'Error loading routines'; // Define a mensagem de erro se a requisição falhar
        console.log('Error loading routines:', error);
      }
    );
  }

  getTypeClass(type: string): string {
    switch (type.toLowerCase()) {
      case 'warm-up':
        return 'warmup-text';
      case 'cool-down':
        return 'cooldown-text';
      case 'stretching':
        return 'stretching-text';
      case 'muscle-focused':
        return 'muscle-text';
      default:
        return 'default-text'; // Caso o tipo não esteja listado
    }
  }

  getTypeStyle(type: string): any {
    switch (type.toLowerCase()) {
      case 'warm-up':
        return { color: '#FFCC00' }; // Cor amarela para warmup
      case 'cool-down':
        return { color: '#00CCFF' }; // Cor azul para cooldown
      case 'stretching':
        return { color: '#FF66CC' }; // Cor rosa para stretching
      case 'muscle-focused':
        return { color: '#FF4500' }; // Cor laranja para muscle-focused
      default:
        return { color: '#000000' }; // Cor preta por padrão
    }
  }

  /*// Check if user is logged in by fetching the user info
  this.isLoggedIn = this.authService.isLoggedIn();  // A verificação da sessão agora é feita com o método isLoggedIn()
  if (this.isLoggedIn) {
    this.userInfo = this.authService.getUserInfo();  // Se estiver logado, pega as informações do usuário
  }
}*/

  title = 'vitalease.client';
}



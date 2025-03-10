import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { ExercisesService } from '../services/exercises.service';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-exercises',
  templateUrl: './exercises.component.html',
  standalone: false,
  styleUrls: ['./exercises.component.css']
})

export class ExercisesComponent implements OnInit {


  userInfo: any = null;
  isLoggedIn: boolean = false;
  isAdmin: boolean = false;
  exercises: any[] = []; // Array para armazenar os logs
  errorMessage: string = '';
  activeModal: string = '';
  modalExercise: any = null; // Armazena o exercício para a modal
  media: any[] = []; // Array para armazenar os logs


  constructor(
    private exercisesService: ExercisesService,
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
          this.getExercises();
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

  // Função para obter os logs
  getExercises(): void {
    this.exercisesService.getExercises().subscribe(
      (response: any) => {
        this.exercises = response; // Armazena os exercises na variável 'exercises'
        console.log('Exercicios carregados com sucesso:', this.exercises);
      },
      (error: any) => {
        this.errorMessage = 'Erro ao carregar os exercicios'; // Define a mensagem de erro se a requisição falhar
        console.log('Erro ao carregar os exercicios:', error);
      }
    );
  }

  getTypeClass(type: string): string {
    switch (type.toLowerCase()) {
      case 'warmup':
        return 'warmup-text';
      case 'cooldown':
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
      case 'warmup':
        return { color: '#FFCC00' }; // Cor amarela para warmup
      case 'cooldown':
        return { color: '#00CCFF' }; // Cor azul para cooldown
      case 'stretching':
        return { color: '#FF66CC' }; // Cor rosa para stretching
      case 'muscle-focused':
        return { color: '#FF4500' }; // Cor laranja para muscle-focused
      default:
        return { color: '#000000' }; // Cor preta por padrão
    }
  }

  // Função para abrir o modal e passar o exercício selecionado
  openModal(modalType: string, exercise: any): void {
    this.activeModal = modalType; // Define que a modal 'details' será exibida
    this.modalExercise = exercise; // Define o exercício selecionado para ser exibido na modal
  }

  closeModal() {
    this.activeModal = ''; // Fechar a modal
    this.modalExercise = null; // Limpa o exercício selecionado
  }

  /*// Check if user is logged in by fetching the user info
  this.isLoggedIn = this.authService.isLoggedIn();  // A verificação da sessão agora é feita com o método isLoggedIn()
  if (this.isLoggedIn) {
    this.userInfo = this.authService.getUserInfo();  // Se estiver logado, pega as informações do usuário
  }
}*/

  title = 'vitalease.client';
}

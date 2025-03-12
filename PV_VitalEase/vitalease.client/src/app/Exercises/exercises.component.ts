import { Component, OnInit } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
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
  exercises: any[] = []; // Array para armazenar os exercises
  errorMessage: string = '';
  activeModal: string = '';
  modalExercise: any = null; // Armazena o exercício para a modal
  media: any[] = []; // Array para armazenar os media
  activeMediaIndex: number = 0; // Índice para controlar qual mídia está sendo exibida


  constructor(
    private exercisesService: ExercisesService,
    private authService: AuthService,
    private router: Router,
    private sanitizer: DomSanitizer,
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
        console.log('Exercises loaded successfully:', this.exercises);
      },
      (error: any) => {
        this.errorMessage = 'Error loading exercises'; // Define a mensagem de erro se a requisição falhar
        console.log('Error loading exercises:', error);
      }
    );
  }

  getModalExerciseMedia(): void {
    this.exercisesService.getMedia(this.modalExercise.id).subscribe(
      (response: any) => {
        this.media = response; // Armazena os exercises na variável 'exercises'
        console.log('Exercise media loaded successfully:', this.media);
      },
      (error: any) => {
        this.errorMessage = 'Error loading exercise media'; // Define a mensagem de erro se a requisição falhar
        console.log('Error loading exercise media:', error);
      }
    );
  }

  convertToEmbedUrl(url: string): string {
    const youtubeRegex = /(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S+?\?v=))([a-zA-Z0-9_-]{11})/;
    const match = url.match(youtubeRegex);

    if (match && match[1]) {
      return `https://www.youtube.com/embed/${match[1]}`;
    }
    return url;  // Retorna a URL original se não for do YouTube
  }

  // Função para sanitizar a URL do vídeo
  sanitizeUrl(url: string): SafeResourceUrl {
    const embedUrl = this.convertToEmbedUrl(url);  // Converte a URL para embed, se necessário
    return this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl); // Sanitiza a URL
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
    this.getModalExerciseMedia();
  }

  closeModal() {
    this.activeModal = ''; // Fechar a modal
    this.modalExercise = null; // Limpa o exercício selecionado
    this.media = [];
    this.activeMediaIndex = 0;
  }

  previousMedia() {
    if (this.activeMediaIndex > 0) {
      this.activeMediaIndex--;
    }
  }

  nextMedia() {
    if (this.activeMediaIndex < this.media.length - 1) {
      this.activeMediaIndex++;
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

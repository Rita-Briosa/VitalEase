import { Component, OnInit } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { AuthService } from '../services/auth.service';
import { ExercisesService } from '../services/exercises.service';
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
  exercises: any[] = []; // Array para armazenar os exercises
  routines: any[] = []; // Array para armazenar as routines
  errorMessage: string = '';
  activeModal: string = '';
  modalExercise: any = null; // Armazena o exercício para a modal
  media: any[] = []; // Array para armazenar os media
  activeMediaIndex: number = 0; // Índice para controlar qual mídia está sendo exibida
  selectedSortedOption: string = '';
  selectedRoutine: number = 0;// Armazena a rotina selecionada
  successMessage: string = '';


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

  getRoutines(): void {
    this.exercisesService.getRoutines(this.userInfo.id).subscribe(
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

  getModalExerciseMedia(): void {
    this.exercisesService.getMedia(this.modalExercise.id).subscribe(
      (response: any) => {
        this.media = response; // Armazena os media na variável 'media'
        console.log('Exercise media loaded successfully:', this.media);
      },
      (error: any) => {
        this.errorMessage = 'Error loading exercise media'; // Define a mensagem de erro se a requisição falhar
        console.log('Error loading exercise media:', error);
      }
    );
  }

  addRoutine(): void {
    if (!this.selectedRoutine || !this.modalExercise) {
      this.errorMessage = 'Please select a routine before adding.';
      return;
    }

    const exerciseId = this.modalExercise.id;
    const routineId = this.selectedRoutine;

    console.log('Adding exercise:', exerciseId, 'to routine:', routineId);

    this.exercisesService.addRoutine(routineId, exerciseId).subscribe(
      (response: any) => {
        this.successMessage = response.message;
        this.errorMessage = '';

        setTimeout(() => {
          this.closeModal();
        }, 2000);
      },
      (error: any) => {
        this.errorMessage = error.error?.message || 'An error occurred';
        this.successMessage = '';
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
    this.errorMessage = '';
    this.successMessage = '';
  }

  openAddModal(exercise: any): void {
    this.activeModal = 'add';
    this.modalExercise = exercise;
    this.getRoutines();
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

  getFilteredExercises(): void {

    this.exercisesService.getFilteredExercises(this.filters).subscribe(
      (response: any) => {
        this.exercises = response;
        console.log('Exercises filtered Successfully:', this.exercises);
      },
      (error) => {
        this.errorMessage = 'Error filtering exercises'; // Define a mensagem de erro se a requisição falhar
        console.log('Error filtering exercises:', error);
      }
    )
  }

  sortExercises(): void {
    this.exercises = this.getSortedExercises(this.selectedSortedOption, this.exercises);
    console.log('Exercises sorted successfully:', this.exercises);
  }

  getSortedExercises(sortedOption: string, exercises: any[]): any[] {
    if (!sortedOption || !exercises || exercises.length === 0) {
      return exercises; // Se não houver opção de ordenação ou exercícios, retorna a lista original
    }

    // Mapeia os níveis de dificuldade para números
    const difficultyOrder: { [key: string]: number } = {
      'Beginner': 1,
      'Intermediate': 2,
      'Advanced': 3
    };

    return [...exercises].sort((a, b) => {
      switch (sortedOption) {
        case 'name-asc':
          return a.name.localeCompare(b.name);
        case 'name-desc':
          return b.name.localeCompare(a.name);
        case 'difficulty-asc':
          return difficultyOrder[a.difficultyLevel] - difficultyOrder[b.difficultyLevel]; // Ascendente
        case 'difficulty-desc':
          return difficultyOrder[b.difficultyLevel] - difficultyOrder[a.difficultyLevel]; // Descendente
        case 'muscle-group':
          return a.muscleGroup.localeCompare(b.muscleGroup);
        case 'equipment':
          return a.equipmentNecessary.localeCompare(b.equipmentNecessary);
        default:
          return 0;
      }
    });
  }

  /*// Check if user is logged in by fetching the user info
  this.isLoggedIn = this.authService.isLoggedIn();  // A verificação da sessão agora é feita com o método isLoggedIn()
  if (this.isLoggedIn) {
    this.userInfo = this.authService.getUserInfo();  // Se estiver logado, pega as informações do usuário
  }
}*/

  title = 'vitalease.client';
}



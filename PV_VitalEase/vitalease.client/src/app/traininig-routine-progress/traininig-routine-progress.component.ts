import { Component, OnInit } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { AuthService } from '../services/auth.service';
import { TrainingRoutinesService } from '../services/training-routines.service';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';


@Component({
  selector: 'app-traininig-routine-progress',
  standalone: false,
  
  templateUrl: './traininig-routine-progress.component.html',
  styleUrl: './traininig-routine-progress.component.css'
})
export class TraininigRoutineProgressComponent {

  userInfo: any = null;
  isLoggedIn: boolean = false;
  isAdmin: boolean = false;
  errorMessage: string = '';

  successMessage: string = '';
  exercises: any[] = []; // Lista de exercícios disponíveis
  media: any[] = []; // Lista de exercícios disponíveis
  routineId: string | null = null;
  activeMediaIndex: number = 0; // Índice para controlar qual mídia está sendo exibida
  activeExerciseIndex: number = 0; // Índice para controlar qual mídia está sendo exibida
  exerciseId: number = 0; // Índice para controlar qual mídia está sendo exibida
  constructor(
    private trainingRoutinesService: TrainingRoutinesService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private sanitizer: DomSanitizer,
    private http: HttpClient) { }

  ngOnInit() {

    this.routineId = this.route.snapshot.paramMap.get('id');

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

  goToTrainingRoutines() {
    this.router.navigate(['/manage-training-routines']);
  }

  getExercises(): void {
    if (!this.routineId) {
      this.errorMessage = "Routine ID is missing.";
      return;
    }

    this.trainingRoutinesService.getExercises(this.routineId).subscribe(
      (response: any) => {
        this.exercises = response;
        console.log('Exercises loaded successfully:', this.exercises);
        this.getExerciseMedia();
      },
      (error: any) => {
        this.errorMessage = 'Error fetching exercises from routine';
        console.error('Error loading exercises:', error);
      }
    );
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

  previousExercise() {
    if (this.activeExerciseIndex > 0) {
      this.activeExerciseIndex--;
      this.getExerciseMedia();
      this.activeMediaIndex = 0;
    }
  }

  nextExercise() {
    if (this.activeExerciseIndex < this.media.length - 1) {
      this.activeExerciseIndex++;
      this.getExerciseMedia();
      this.activeMediaIndex = 0;
    }
  }

  skipExercises() {
    this.activeExerciseIndex = this.exercises.length - 1; // Salta para o último exercício
    this.getExerciseMedia();
    this.activeMediaIndex = 0;
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

  getExerciseMedia(): void {

    var exercise = this.exercises[this.activeExerciseIndex];

    this.exerciseId = exercise.id;

    this.trainingRoutinesService.getMedia(this.exerciseId.toString()).subscribe(
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

}

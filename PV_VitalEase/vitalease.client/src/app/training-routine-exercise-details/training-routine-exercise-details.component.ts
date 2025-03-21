import { Component } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { AuthService } from '../services/auth.service';
import { TrainingRoutinesService } from '../services/training-routines.service';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';


@Component({
  selector: 'app-training-routine-exercise-details',
  standalone: false,
  
  templateUrl: './training-routine-exercise-details.component.html',
  styleUrl: './training-routine-exercise-details.component.css'
})
export class TrainingRoutineExerciseDetailsComponent {

  exerciseId: string | null = null;
  userInfo: any = null;
  isLoggedIn: boolean = false;
  isAdmin: boolean = false;
  media: any[] = []; // Array para armazenar os media
  activeMediaIndex: number = 0; // Índice para controlar qual mídia está sendo exibida
  exercise: any = null;
  errorMessage: string = '';

  constructor(
    private trainingRoutinesService: TrainingRoutinesService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private sanitizer: DomSanitizer,
    private http: HttpClient) { }

  ngOnInit() {

    this.exerciseId = this.route.snapshot.paramMap.get('id');

    const token = this.authService.getSessionToken();

    if (token) {
      this.authService.validateSessionToken().subscribe(
        (response: any) => {
          this.isLoggedIn = true;
          this.userInfo = response.user;
          this.getExercise();
          this.getExerciseMedia();
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

  getExercise(): void {

    if (this.exerciseId != null) {
      this.trainingRoutinesService.getExercise(this.exerciseId).subscribe(
        (response: any) => {
          this.exercise = response; // Armazena os exercises na variável 'exercises'
          console.log('Exercise loaded successfully:', this.exercise);
        },
        (error: any) => {
          this.errorMessage = 'Error loading exercises'; // Define a mensagem de erro se a requisição falhar
          console.log('Error loading exercises:', error);
        }
      );
    } else {
      this.errorMessage = 'exercise Id doesn t exists';
    }
   
  }

  getExerciseMedia(): void {

    if (this.exerciseId != null) {
      this.trainingRoutinesService.getMedia(this.exerciseId).subscribe(
        (response: any) => {
          this.media = response; // Armazena os media na variável 'media'
          console.log('Exercise media loaded successfully:', this.media);
        },
        (error: any) => {
          this.errorMessage = 'Error loading exercise media'; // Define a mensagem de erro se a requisição falhar
          console.log('Error loading exercise media:', error);
        }
      );
    } else {
      this.errorMessage = 'exercise Id doesn t exists';
    }
   
  }
}

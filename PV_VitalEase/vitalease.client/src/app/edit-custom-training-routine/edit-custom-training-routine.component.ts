import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { TrainingRoutinesService } from '../services/training-routines.service';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-edit-custom-training-routine',
  standalone: false,
  
  templateUrl: './edit-custom-training-routine.component.html',
  styleUrl: './edit-custom-training-routine.component.css'
})
export class EditCustomTrainingRoutineComponent {
  errorMessage: string = '';
  successMessage: string = '';
  activeModal: string = '';

  userInfo: any = null;
  isLoggedIn: boolean = false;

  routineId: string | null = null;
  exercises: any[] = []; // Lista de exercícios
  warmUpExercises: any[] = [];
  mainExercises: any[] = [];
  coolDownExercises: any[] = [];

  selectedExerciseId: number = 0;
  selectedExercise: any = null;
  activeMediaIndex: number = 0; // Índice para controlar qual mídia está sendo exibida
  media: any[] = []; // Array para armazenar os media

  constructor(private authService: AuthService, private routinesService: TrainingRoutinesService, private router: Router, private route: ActivatedRoute, private sanitizer: DomSanitizer,) { }

  ngOnInit() {
    // Check if user is logged in by fetching the user info
    /* this.isLoggedIn = this.authService.isLoggedIn();  // A verificação da sessão agora é feita com o método isLoggedIn()
     if (this.isLoggedIn) {
       this.userInfo = this.authService.getUserInfo();  // Se estiver logado, pega as informações do usuário
     }
   }*/

    this.routineId = this.route.snapshot.paramMap.get('id');

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
      //No token found, redirect to login
      this.router.navigate(['/login']);
    }

    this.getExercises();

  }

  getExercises(): void {
    if (!this.routineId) {
      return;
    }

    this.routinesService.getExercises(this.routineId).subscribe(
      (response: any) => {
        this.exercises = response;

        this.exercises.forEach(e => {
          if (e.type === "Warm-up") {
            this.warmUpExercises.push(e);
          }
          if (e.type === "Stretching" || e.type === "Muscle-focused") {
            this.mainExercises.push(e);
          }
          if (e.type === "Cool-Down") {
            this.coolDownExercises.push(e);
          }
        });

        console.log(this.warmUpExercises);
        console.log(this.mainExercises);
        console.log(this.coolDownExercises);
        console.log('Exercises loaded successfully:', this.exercises);
      },
      (error: any) => {
        console.error('Error loading exercises:', error);
      }
    );
  }

  deleteExercise(exerciseId: number) {
    if (!this.routineId) {
      return;
    }

    this.routinesService.deleteExerciseFromRoutine(parseInt(this.routineId), exerciseId).subscribe(
      (response: any) => {
        console.log(response);
        window.location.reload();
      },
      (error: any) => {
        console.error('Error removing exercice:', error);
      }
    )
  }

  selectExercise(exerciseId: number) {
    this.selectedExerciseId = exerciseId;
    this.selectedExercise = this.exercises.find(e => e.id === exerciseId)
    this.getExerciseMedia(exerciseId);
  }

  unselectExercise() {
    this.selectedExerciseId = 0;
    this.selectedExercise = null;
    this.media = [];
  }

  openDeleteExerciseModal(exerciseId: number): void {
    this.selectExercise(exerciseId);
    this.activeModal = 'deleteExercise';
  }

  closeModal() {
    this.unselectExercise();
    this.activeModal = ''; // Fechar a modal
    this.errorMessage = '';
    this.successMessage = '';
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

  convertToEmbedUrl(url: string): string {
    const youtubeRegex = /(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S+?\?v=))([a-zA-Z0-9_-]{11})/;
    const match = url.match(youtubeRegex);

    if (match && match[1]) {
      return `https://www.youtube.com/embed/${match[1]}`;
    }
    return url;  // Retorna a URL original se não for do YouTube
  }

  sanitizeUrl(url: string): SafeResourceUrl {
    const embedUrl = this.convertToEmbedUrl(url);  // Converte a URL para embed, se necessário
    return this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl); // Sanitiza a URL
  }

  getExerciseMedia(exerciseId: number): void {

    if (exerciseId != null) {
      this.routinesService.getMedia(exerciseId.toString()).subscribe(
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

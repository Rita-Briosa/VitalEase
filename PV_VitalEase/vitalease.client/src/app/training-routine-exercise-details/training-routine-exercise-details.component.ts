import { Component } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { AuthService } from '../services/auth.service';
import { TrainingRoutinesService } from '../services/training-routines.service';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

/**
 * @component TrainingRoutineExerciseDetailsComponent
 * @description
 * The TrainingRoutineExerciseDetailsComponent displays detailed information for a specific exercise within a training routine.
 * It retrieves the exercise ID from the route parameters and validates the user's session via the AuthService. If the session is valid,
 * the component fetches the exercise details and its associated media (e.g., videos) using the TrainingRoutinesService.
 * It also provides helper methods to convert and sanitize media URLs for safe embedding and to apply styling based on the exercise type.
 *
 * @dependencies
 * - TrainingRoutinesService: Provides methods to retrieve exercise details and associated media.
 * - AuthService: Validates the user's session token and provides user authentication data.
 * - ActivatedRoute: Extracts the exercise ID from the current route.
 * - Router: Enables navigation between different application routes.
 * - DomSanitizer: Sanitizes URLs for safe embedding in the template.
 * - HttpClient: Used for HTTP requests when necessary.
 *
 * @usage
 * This component is not standalone and uses external templates and styles:
 *   - Template: './training-routine-exercise-details.component.html'
 *   - Styles: './training-routine-exercise-details.component.css'
 */
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

  /**
 * @method ngOnInit
 * @description
 * Lifecycle hook that initializes the component. It extracts the exercise ID from the route parameters,
 * validates the user's session token, and then fetches the exercise details and associated media.
 * If the session token is invalid or missing, the user is redirected to the login page.
 */
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

  /**
   * @method goToDashboard
   * @description Navigates the user to the dashboard page.
   */
  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  /**
   * @method convertToEmbedUrl
   * @description
   * Converts a standard YouTube URL into an embeddable URL using a regex pattern. 
   * If the URL is not a YouTube URL, it returns the original URL.
   *
   * @param url - The original media URL.
   * @returns A string representing the embeddable URL.
   */
  convertToEmbedUrl(url: string): string {
    const youtubeRegex = /(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S+?\?v=))([a-zA-Z0-9_-]{11})/;
    const match = url.match(youtubeRegex);

    if (match && match[1]) {
      return `https://www.youtube.com/embed/${match[1]}`;
    }
    return url;  // Retorna a URL original se não for do YouTube
  }

  /**
   * @method sanitizeUrl
   * @description
   * Sanitizes a media URL for safe embedding in the component's template by converting it to an embeddable URL and bypassing Angular's security.
   *
   * @param url - The media URL to sanitize.
   * @returns A SafeResourceUrl that can be used in the template.
   */
  sanitizeUrl(url: string): SafeResourceUrl {
    const embedUrl = this.convertToEmbedUrl(url);  // Converte a URL para embed, se necessário
    return this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl); // Sanitiza a URL
  }

  /**
   * @method getTypeClass
   * @description
   * Returns a CSS class based on the exercise type for styling purposes.
   *
   * @param type - The exercise type.
   * @returns A string representing the corresponding CSS class.
   */
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

  /**
   * @method getTypeStyle
   * @description
   * Returns an inline style object based on the exercise type.
   *
   * @param type - The exercise type.
   * @returns An object containing CSS style properties.
   */
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

  /**
   * @method previousMedia
   * @description Navigates to the previous media item in the media array.
   */
  previousMedia() {
    if (this.activeMediaIndex > 0) {
      this.activeMediaIndex--;
    }
  }

  /**
   * @method nextMedia
   * @description Navigates to the next media item in the media array.
   */
  nextMedia() {
    if (this.activeMediaIndex < this.media.length - 1) {
      this.activeMediaIndex++;
    }
  }

  /**
   * @method getExercise
   * @description
   * Fetches the exercise details for the given exercise ID using the TrainingRoutinesService.
   * The retrieved exercise is stored in the component's exercise property.
   */
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

  /**
   * @method getExerciseMedia
   * @description
   * Fetches media associated with the exercise using the TrainingRoutinesService.
   * The retrieved media items are stored in the component's media array.
   */
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

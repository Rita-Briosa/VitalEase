import { Component, OnInit } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { AuthService } from '../services/auth.service';
import { TrainingRoutinesService } from '../services/training-routines.service';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

/**
 * @component TraininigRoutineProgressComponent
 * @description
 * The TraininigRoutineProgressComponent (note the misspelling in the component name) is responsible for managing and displaying the progress of a training routine.
 * It retrieves the routine ID from the route parameters and validates the user session via the AuthService. Once authenticated,
 * it loads the exercises associated with the routine using the TrainingRoutinesService, and it displays details such as the media (e.g. videos) 
 * for each exercise and the exercise routine relation (e.g. sets and reps).
 *
 * The component supports navigation between different exercises within the routine. It provides controls to navigate to the previous or next 
 * media item, as well as previous or next exercise. A "skip exercises" function allows the user to jump directly to the last exercise in the routine.
 *
 * Additionally, the component includes helper methods to convert standard YouTube URLs into embeddable URLs and to sanitize them for safe 
 * embedding in the template.
 *
 * @dependencies
 * - TrainingRoutinesService: Used to fetch the list of exercises for the routine, the media associated with each exercise, and the relation details
 *   (such as sets and reps) between an exercise and the routine.
 * - AuthService: Validates the user's session token and provides the logged-in user's information.
 * - ActivatedRoute: Extracts the routine ID from the route parameters.
 * - Router: Facilitates navigation between application routes.
 * - DomSanitizer: Sanitizes media URLs to ensure safe embedding in the Angular template.
 * - HttpClient: Supports HTTP operations as needed.
 *
 * @usage
 * This component is not standalone and uses external templates and styles:
 *   - Template: './traininig-routine-progress.component.html'
 *   - Styles: './traininig-routine-progress.component.css'
 */
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

  shownRelation: any = null;
; // Indice para os sets e reps
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
 * Lifecycle hook that initializes the component. It retrieves the routine ID from the route parameters and validates the user's session token.
 * If the token is valid, it calls the getExercises() method to load the exercises associated with the routine.
 * In case of an invalid token or missing routine ID, an error message is set or the user is redirected to the login page.
 */
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

  /**
   * @method goToDashboard
   * @description Navigates the user to the dashboard page.
   */
  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  /**
   * @method goToTrainingRoutines
   * @description Navigates the user to the manage training routines page.
   */
  goToTrainingRoutines() {
    this.router.navigate(['/manage-training-routines']);
  }

  /**
   * @method getExercises
   * @description
   * Retrieves the list of exercises associated with the training routine by calling the TrainingRoutinesService.
   * If the routine ID is missing, it sets an error message.
   */
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
        this.getExerciseRoutine(this.exerciseId);
      },
      (error: any) => {
        this.errorMessage = 'Error fetching exercises from routine';
        console.error('Error loading exercises:', error);
      }
    );
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
   * @method previousExercise
   * @description
   * Navigates to the previous exercise in the exercises list. It updates the active exercise index,
   * reloads the media for the new active exercise, and resets the media index to 0.
   */
  previousExercise() {
    if (this.activeExerciseIndex > 0) {
      this.activeExerciseIndex--;
      this.getExerciseMedia();
      this.getExerciseRoutine(this.exercises[this.activeExerciseIndex].id);
      this.activeMediaIndex = 0;
    }
  }

  /**
   * @method nextExercise
   * @description
   * Navigates to the next exercise in the exercises list. It updates the active exercise index,
   * reloads the media for the new active exercise, and resets the media index to 0.
   */
  nextExercise() {
    if (this.activeExerciseIndex < this.media.length ) {
      this.activeExerciseIndex++;
      this.getExerciseMedia();
      this.getExerciseRoutine(this.exercises[this.activeExerciseIndex].id);
      this.activeMediaIndex = 0;
    }
  }

  /**
   * @method skipExercises
   * @description Jumps to the last exercise in the exercises list. It reloads the media and exercise routine details for the last exercise.
   */
  skipExercises() {
    this.activeExerciseIndex = this.exercises.length - 1; // Salta para o último exercício
    this.getExerciseMedia();
    this.getExerciseRoutine(this.exercises[this.activeExerciseIndex].id);
    this.activeMediaIndex = 0;
  }

  /**
   * @method convertToEmbedUrl
   * @description
   * Converts a YouTube URL into an embeddable URL format using a regular expression. If the URL does not match the YouTube pattern,
   * it returns the original URL.
   *
   * @param url - The original video URL.
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
   * Sanitizes a video URL for safe embedding in the template by converting it to an embeddable URL and bypassing Angular's security.
   *
   * @param url - The original video URL.
   * @returns A SafeResourceUrl that can be bound to an iframe or similar element.
   */
  sanitizeUrl(url: string): SafeResourceUrl {
    const embedUrl = this.convertToEmbedUrl(url);  // Converte a URL para embed, se necessário
    return this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl); // Sanitiza a URL
  }

  /**
   * @method getExerciseMedia
   * @description
   * Retrieves media items associated with the current active exercise by calling the TrainingRoutinesService.
   * The exercise ID is determined from the exercises list based on the active exercise index.
   */
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

  /**
   * @method getExerciseRoutine
   * @description
   * Retrieves the relation details (e.g., sets, reps) for a given exercise in the training routine by calling the TrainingRoutinesService.
   *
   * @param exerciseId - The ID of the exercise for which to retrieve relation details.
   */
  getExerciseRoutine(exerciseId: number) {
    if (!this.routineId) {
      console.log("null");
      return;
    }

    console.log(parseInt(this.routineId));
    console.log(exerciseId);

    this.trainingRoutinesService.getExerciseRoutine(parseInt(this.routineId), exerciseId).subscribe(
      (response: any) => {
        this.shownRelation = response;
        console.log(response);
      },
      (error: any) => {
        console.error('Error getting relation between exercise and routine', error);
      }
    )
  }

}

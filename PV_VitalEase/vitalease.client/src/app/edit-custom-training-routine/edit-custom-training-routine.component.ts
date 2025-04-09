import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { TrainingRoutinesService } from '../services/training-routines.service';
import { ExercisesService } from '../services/exercises.service';

/**
 * @component EditCustomTrainingRoutineComponent
 * @description
 * The EditCustomTrainingRoutineComponent manages the editing of a custom training routine. This component allows the user to view, add, edit,
 * and delete exercises associated with a specific training routine. It also supports categorising exercises into different groups such as
 * Warm-up, Main, and Cool-Down. In addition, the component handles media associated with exercises, including converting and sanitising URLs
 * for embedding (e.g. YouTube videos).
 *
 * @dependencies
 * - AuthService: Provides methods for validating the user's session token.
 * - TrainingRoutinesService: Manages operations related to training routines (e.g. fetching exercises, deleting exercises, retrieving media).
 * - ExercisesService: Provides methods to add exercises to a routine.
 * - Router & ActivatedRoute: Handle navigation and parameter extraction from the route.
 * - DomSanitizer: Sanitises resource URLs for safe embedding in the template.
 *
 * @usage
 * This component is not standalone and uses external templates and styles:
 * - Template: './edit-custom-training-routine.component.html'
 * - Styles: './edit-custom-training-routine.component.css'
 */
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
  isAdmin: boolean = false;
  routineId: string | null = null;
  routineExercises: any[] = []; // Lista de exercícios
  warmUpExercises: any[] = [];
  mainExercises: any[] = [];
  coolDownExercises: any[] = [];

  selectedExerciseId: number = 0;
  selectedExercise: any = null;

  shownRelation: any = null;

  activeMediaIndex: number = 0; // Índice para controlar qual mídia está sendo exibida
  media: any[] = []; // Array para armazenar os media

  exercises: any[] = [];
  sets: number = 3;
  reps: number = 12;// Armazena a rotina selecionada
  duration: number = 60;// Armazena a rotina selecionada
  selectedModalExercise: any = null; // Armazena o exercício para a modal
  selectedOption: string = 'duration';

  routine: any = null;
  routineName: string = '';


  constructor(private authService: AuthService, private routinesService: TrainingRoutinesService, private exercisesService: ExercisesService, private router: Router, private route: ActivatedRoute, private sanitizer: DomSanitizer,) { }

  /**
 * @method ngOnInit
 * @description
 * Lifecycle hook that initialises the component. It retrieves the routine ID from the route parameters,
 * validates the user session via the AuthService, and then loads the exercises for the routine as well as the list of all exercises.
 */
  ngOnInit() {
    this.routineId = this.route.snapshot.paramMap.get('id');

    const token = this.authService.getSessionToken();

    if (token) {
      this.authService.validateSessionToken().subscribe(
        (response: any) => {
          this.isLoggedIn = true;
          this.userInfo = response.user;
          if (this.userInfo.type === 1) {
            this.isAdmin = true;
          } else {
            this.isAdmin = false;
          }

          if (!this.routineId) return;

          this.routinesService.getRoutine(parseInt(this.routineId)).subscribe(
            (response: any) => {
              this.routine = response;
              this.routineName = this.routine.name;
              console.log(`${this.routine.userId} + ${this.userInfo.id}`);
              console.log(`${this.routine.name} + ${this.userInfo.id}`);

              if (this.routine.userId !== this.userInfo.id && this.isAdmin === false) {
                this.router.navigate(['/']);
              }

            },
            (error: any) => {
              console.error('Error loading Routine', error);
            }
          );

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

    this.getRoutineExercises();
    this.getExercises();

  }

  /**
 * @method getExercises
 * @description
 * Retrieves the list of all exercises from the ExercisesService and stores them locally.
 */
  getExercises(): void {
    this.exercisesService.getExercises().subscribe(
      (response: any) => {
        this.exercises = response;
      },
      (error: any) => {
        console.error('Error loading Exercises', error);
      }
    );
  }

  /**
* @method getRoutine
* @description
* Retrieves the information of the routine and stores it locally.
*/
  getRoutine(): void {
    if (!this.routineId) return;

    this.routinesService.getRoutine(parseInt(this.routineId)).subscribe(
      (response: any) => {
        this.routine = response;
      },
      (error: any) => {
        console.error('Error loading Routine', error);
      }
    );
  }

  /**
  * @method goToDashboard
  * @description Navigates the user to the dashboard.
  */
  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  /**
 * @method getRoutineExercises
 * @description
 * Retrieves exercises associated with the current routine from the TrainingRoutinesService.
 * Categorises the exercises into Warm-up, Main, and Cool-Down groups based on their type.
 */
  getRoutineExercises(): void {
    if (!this.routineId) {
      return;
    }

    this.routinesService.getExercises(this.routineId).subscribe(
      (response: any) => {
        this.routineExercises = response.exerciseDtos;

        console.log(response);

        this.routineExercises.forEach(e => {
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
        console.log('Exercises loaded successfully:', this.routineExercises);
      },
      (error: any) => {
        console.error('Error loading exercises:', error);
      }
    );
  }

  /**
 * @method getExerciseRoutine
 * @description
 * Retrieves the relation details between a specific exercise and the routine.
 *
 * @param exerciseId - The identifier of the exercise for which to retrieve relation details.
 */
  getExerciseRoutine(exerciseId: number) {
    if (!this.routineId) {
      return;
    }

    console.log(parseInt(this.routineId));
    console.log(exerciseId);

    this.routinesService.getExerciseRoutine(parseInt(this.routineId), exerciseId).subscribe(
      (response: any) => {
        this.shownRelation = response;


        if (this.shownRelation.duration === null || this.shownRelation.duration === 0) {
          this.sets = this.shownRelation.sets;
          this.reps = this.shownRelation.reps;
          this.selectedOption = 'reps';
          this.duration = 60;
        } else if (this.shownRelation.reps === null || this.shownRelation.reps === 0) {
          this.sets = this.shownRelation.sets;
          this.duration = this.shownRelation.duration;
          this.selectedOption = 'duration';
          this.reps = 12;
        }

        console.log(response);
      },
      (error: any) => {
        console.error('Error getting relation between exercise and routine', error);
      }
    )
  }

  /**
 * @method deleteExercise
 * @description
 * Deletes an exercise from the current routine using the TrainingRoutinesService.
 *
 * @param exerciseId - The identifier of the exercise to be removed.
 */
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

  /**
 * @method selectExercise
 * @description
 * Selects an exercise from the routine, updating the component state to show its details.
 * Also retrieves the relation details and associated media for the selected exercise.
 *
 * @param exerciseId - The identifier of the exercise to select.
 */
  selectExercise(exerciseId: number) {
    this.selectedExerciseId = exerciseId;
    this.selectedExercise = this.routineExercises.find(e => e.id === exerciseId);
    this.getExerciseRoutine(exerciseId);
    this.getExerciseMedia(exerciseId);
  }

  /**
 * @method unselectExercise
 * @description
 * Clears the selection of an exercise, resetting related component state properties.
 */
  unselectExercise() {
    this.selectedExerciseId = 0;
    this.selectedExercise = null;
    this.shownRelation = null;
    this.selectedModalExercise = null;
    this.media = [];
  }

  /**
 * @method openDeleteExerciseModal
 * @description
 * Opens the modal dialog for deleting an exercise by selecting the exercise and setting the active modal.
 *
 * @param exerciseId - The identifier of the exercise to be deleted.
 */
  openDeleteExerciseModal(exerciseId: number): void {
    this.selectExercise(exerciseId);
    this.activeModal = 'deleteExercise';
  }

  /**
 * @method closeModal
 * @description Closes any open modal dialogs and resets error and success messages.
 */
  closeModal() {
    this.unselectExercise();
    this.activeModal = ''; // Fechar a modal
    this.errorMessage = '';
    this.successMessage = '';
  }

  /**
 * @method previousMedia
 * @description Moves to the previous media item in the media array.
 */
  previousMedia() {
    if (this.activeMediaIndex > 0) {
      this.activeMediaIndex--;
    }
  }

  /**
 * @method nextMedia
 * @description Moves to the next media item in the media array.
 */
  nextMedia() {
    if (this.activeMediaIndex < this.media.length - 1) {
      this.activeMediaIndex++;
    }
  }

  /**
 * @method convertToEmbedUrl
 * @description
 * Converts a standard YouTube URL into an embeddable URL. If the URL does not match YouTube's pattern,
 * the original URL is returned.
 *
 * @param url - The original media URL.
 * @returns A URL formatted for embedding.
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
 * Sanitises a media URL for safe embedding in the component template.
 *
 * @param url - The media URL to sanitise.
 * @returns A SafeResourceUrl that can be used in the template.
 */
  sanitizeUrl(url: string): SafeResourceUrl {
    const embedUrl = this.convertToEmbedUrl(url);  // Converte a URL para embed, se necessário
    return this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl); // Sanitiza a URL
  }

  /**
 * @method getExerciseMedia
 * @description
 * Retrieves media associated with a specific exercise by calling the TrainingRoutinesService.
 * The retrieved media is stored in the media array.
 *
 * @param exerciseId - The identifier of the exercise.
 */
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

  /**
 * @method addExercise
 * @description
 * Adds an exercise to the routine by calling the ExercisesService.
 * The exercise is added along with parameters such as reps, duration, and sets.
 * On success, a success message is displayed and the page is reloaded.
 */
  addExercise(): void {
    if (!this.routineId || !this.selectedModalExercise) {
      this.errorMessage = 'Please select an exercise before adding.';
      return;
    }

    const modalExerciseId = this.selectedModalExercise.id;

    console.log(`${this.routineId}/ ${modalExerciseId} / ${this.reps} / ${this.duration} / ${this.sets}`);

    if (this.selectedOption === 'duration') {
      this.reps = 0;
    } else if (this.selectedOption === 'reps') {
      this.duration = 0;
    }

    console.log(`${this.routineId}/ ${modalExerciseId} / ${this.reps} / ${this.duration} / ${this.sets}`);


    this.exercisesService.addRoutine(parseInt(this.routineId), modalExerciseId, this.reps, this.duration, this.sets).subscribe(
      (response: any) => {
        this.successMessage = response.message;
        this.errorMessage = '';
        this.closeModal();
        window.location.reload();
      },
      (error: any) => {
        this.errorMessage = error.error?.message || 'An error occurred';
        this.successMessage = '';
      }
    );
  }

  /**
 * @method openAddModal
 * @description Opens the modal dialog for adding an exercise to the routine.
 */
  openAddModal(): void {
    this.unselectExercise();
    console.log("add");
    this.activeModal = 'add';
  }

  /**
 * @method editExercise
 * @description
 * Edits the details of an exercise within the routine (such as sets, duration, and reps) by calling the TrainingRoutinesService.
 * On success, the modal is closed and the page is reloaded.
 */
  editExercise(): void {
    if (!this.routineId || !this.selectedExercise) {
      this.errorMessage = 'Please select an exercise before adding.';
      return;
    }

    const exerciseId = this.selectedExercise.id;

    console.log(`${this.routineId}/ ${exerciseId} / ${this.reps} / ${this.duration} / ${this.sets}`);

    if (this.selectedOption === 'duration') {
      this.reps = 0;
    } else if (this.selectedOption === 'reps') {
      this.duration = 0;
    }

    console.log(`${this.routineId}/ ${exerciseId} / ${this.reps} / ${this.duration} / ${this.sets}`);

    this.routinesService.editExerciseRoutine(parseInt(this.routineId), exerciseId, this.reps, this.duration, this.sets).subscribe(
      (response: any) => {
        this.closeModal();
        window.location.reload();
      },
      (error: any) => {
        this.errorMessage = error.error?.message || 'An error occurred';
        this.successMessage = '';
      }
    );
  }

  /**
 * @method openEditExerciseModal
 * @description Opens the modal dialog for editing an exercise by selecting the exercise and setting the active modal.
 *
 * @param exerciseId - The identifier of the exercise to be edited.
 */
  openEditExerciseModal(exerciseId: number): void {
    this.unselectExercise();
    this.selectExercise(exerciseId);

    
    this.activeModal = 'edit';
    
   
  }

  /**
 * @method onOptionChange
 * @description
 * Handler for option changes that resets values conditionally. If the selected option is 'duration', the reps are reset to 0,
 * and if it is 'reps', the duration is reset to 0.
 */
  onOptionChange() {
    if (this.selectedOption === 'duration') {
      if (this.shownRelation.reps === 0 || this.shownRelation.reps === null) {
        this.reps = 12;
        this.duration = this.shownRelation.duration;
      } 
    
    } else if (this.selectedOption === 'reps') {
      if (this.shownRelation.duration === 0 || this.shownRelation.duration === null) {
        this.duration = 60;
        this.reps = this.shownRelation.reps;
      } 
    }
  }


  /**
 * @method limitNumberOfDigitsSets
 * @description
 * Input validator that restricts user input for the "sets" field to valid numeric values between 1 and 12.
 * It allows navigation keys (ArrowLeft, ArrowRight) and editing keys (Delete, Backspace),
 * prevents leading zeros, enforces a numeric range (1–12), and limits input to a maximum of two digits.
 *
 * @param {KeyboardEvent} event - The keyboard event triggered on key press in the input field.
 *
 * @remarks
 * - Prevents input if the key is not a digit or exceeds the 2-digit limit.
 * - Ensures values like "00", "013", or "99" are blocked if they are outside the accepted range.
 * - Only allows valid numbers to be formed with each keystroke.
 */
  limitNumberOfDigitsSets(event: KeyboardEvent) {
    const input = event.target as HTMLInputElement;
    const key = event.key;
    const value = input.value;

    if (key === "ArrowLeft" || key === "ArrowRight" || key === "Delete" || key === "Backspace") {
      return;
    }

    // Impede que o primeiro número seja "0"
    if (value === "" && key === "0") {
      event.preventDefault();
    }

    // Impede que o valor ultrapasse 200 para reps (3 dígitos)
    if (value.length < 3 && /^[0-9]$/.test(key)) {
      const newValue = value + key;
      const numeric = parseInt(newValue, 10);
      if (numeric > 12 || numeric <= 0) {
        event.preventDefault();
      }
    }

    // Limita o comprimento a 3 dígitos para reps
    if (value.length >= 2 || !/^[0-9]$/.test(key)) {
      event.preventDefault();
    }
  }

  /**
 * @method limitNumberOfDigitsDuration
 * @description
 * Input validator that restricts user input for the "duration" field to valid numeric values between 1 and 600 seconds.
 * It allows navigation (ArrowLeft, ArrowRight) and editing keys (Delete, Backspace), prevents leading zeros,
 * enforces the numeric range (1–600), and limits input to a maximum of three digits.
 *
 * @param {KeyboardEvent} event - The keyboard event triggered when the user types in the input field.
 *
 * @remarks
 * - Blocks input if the key is not a digit or would result in an invalid number.
 * - Ensures values like "000", "601", or "999" are prevented.
 * - Enforces value integrity on each keystroke to avoid invalid durations.
 */
  limitNumberOfDigitsDuration(event: KeyboardEvent) {
    const input = event.target as HTMLInputElement;
    const key = event.key;
    const value = input.value;

    if (key === "ArrowLeft" || key === "ArrowRight" || key === "Delete" || key === "Backspace") {
      return;
    }

    // Impede que o primeiro número seja "0"
    if (value === "" && key === "0") {
      event.preventDefault();
    }

    // Impede que o valor ultrapasse 600 para duration (3 dígitos)
    if (value.length < 3 && /^[0-9]$/.test(key)) {
      const newValue = value + key;
      const numeric = parseInt(newValue, 10);
      if (numeric > 600 || numeric <= 0) {
        event.preventDefault();
      }
    }

    // Limita o comprimento a 3 dígitos para reps
    if (value.length >= 3 || !/^[0-9]$/.test(key)) {
      event.preventDefault();
    }
  }

  /**
   * @method limitNumberOfDigitsReps
   * @description
   * Input validator that restricts user input for the "reps" field to valid numeric values between 1 and 200.
   * It permits navigation keys (ArrowLeft, ArrowRight) and editing keys (Delete, Backspace),
   * prevents leading zeros, enforces the numeric range (1–200), and restricts input to a maximum of three digits.
   *
   * @param {KeyboardEvent} event - The keyboard event triggered on key press in the input field.
   *
   * @remarks
   * - Prevents input if the character is not a digit or if the resulting value is outside the valid range.
   * - Ensures that inputs like "000", "201", or "999" are blocked.
   * - Maintains data integrity by validating on every keystroke.
   */
  limitNumberOfDigitsReps(event: KeyboardEvent) {
    const input = event.target as HTMLInputElement;
    const key = event.key;
    const value = input.value;

    if (key === "ArrowLeft" || key === "ArrowRight" || key === "Delete" || key === "Backspace") {
      return;
    }

    // Impede que o primeiro número seja "0"
    if (value === "" && key === "0") {
      event.preventDefault();
    }

    // Impede que o valor ultrapasse 200 para reps (3 dígitos)
    if (value.length < 3 && /^[0-9]$/.test(key)) {
      const newValue = value + key;
      const numeric = parseInt(newValue, 10);
      if (numeric > 200 || numeric <= 0) {
        event.preventDefault();
      }
    }

    // Limita o comprimento a 3 dígitos para reps
    if (value.length >= 3 || !/^[0-9]$/.test(key)) {
      event.preventDefault();
    }

  }


}

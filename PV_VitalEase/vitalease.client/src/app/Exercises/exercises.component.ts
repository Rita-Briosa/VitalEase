import { Component, OnInit } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { AuthService } from '../services/auth.service';
import { ExercisesService } from '../services/exercises.service';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

/**
 * @component ExercisesComponent
 * @description
 * The ExercisesComponent is responsible for managing and displaying the list of exercises and routines available in the application.
 * It allows users to filter and sort exercises based on various criteria, view exercise media, and add exercises to selected routines.
 * Additionally, it supports administrative functionalities such as adding new exercises.
 *
 * The component interacts with the following services:
 * - ExercisesService: Handles API requests for fetching exercises, routines, media, and applying filters.
 * - AuthService: Validates user authentication by verifying session tokens.
 * - Router: Enables navigation between routes.
 * - DomSanitizer: Sanitizes media URLs for safe embedding in the template.
 * - HttpClient: Facilitates HTTP requests.
 *
 * @usage
 * This component is not standalone and uses external templates and styles:
 * - Template: './exercises.component.html'
 * - Styles: './exercises.component.css'
 */
@Component({
  selector: 'app-exercises',
  templateUrl: './exercises.component.html',
  standalone: false,
  styleUrls: ['./exercises.component.css']
})

export class ExercisesComponent implements OnInit {

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
  addErrorMessage: string = '';
  addRoutineErrorMessage: string = '';
  activeModal: string = '';
  modalExercise: any = null; // Armazena o exercício para a modal
  media: any[] = []; // Array para armazenar os media
  activeMediaIndex: number = 0; // Índice para controlar qual mídia está sendo exibida
  selectedSortedOption: string = '';
  selectedOption: string = 'duration';
  selectedRoutine: number = 0;// Armazena a rotina selecionada
  successMessage: string = '';
  newName: string = '';
  newDescription: string = '';
  newType: string = '';
  newDifficultyLevel: string = '';
  newMuscleGroup: string = '';
  newEquipmentNecessary: string = '';
  newMediaName: string = '';
  newMediaType: string = '';
  newMediaUrl: string = '';
  newMediaName1: string = '';
  newMediaType1: string = '';
  newMediaUrl1: string = '';
  newMediaName2: string = '';
  newMediaType2: string = '';
  newMediaUrl2: string = '';
  sets: number = 3;
  reps: number = 12;// Armazena a rotina selecionada
  duration: number = 60;// Armazena a rotina selecionada

  constructor(
    private exercisesService: ExercisesService,
    private authService: AuthService,
    private router: Router,
    private sanitizer: DomSanitizer,
    private http: HttpClient) { }

  /**
* @method ngOnInit
* @description
* Lifecycle hook that initializes the component. It validates the user's session token via AuthService.
* If validation is successful, it loads the exercises list. Otherwise, the user is redirected to the login page.
*/
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

  /**
 * @method goToDashboard
 * @description Navigates the user to the dashboard.
 */
  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  /**
 * @method getExercises
 * @description
 * Retrieves the list of exercises from the ExercisesService and stores them in the 'exercises' array.
 */
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

  /**
 * @method getRoutines
 * @description
 * Retrieves the list of routines for the current user from the ExercisesService.
 */
  getRoutines(): void {
    this.exercisesService.getRoutines(this.userInfo.id).subscribe(
      (response: any) => {
        this.routines = response; // Armazena as routines na variável 'routines'
        console.log('Routines loaded successfully:', this.routines);
      },
      (error: any) => {
        this.addRoutineErrorMessage = error.error?.message || 'An unexpected error occurred'; // Define a mensagem de erro se a requisição falhar
        console.log('Error loading routines:', error);
      }
    );
  }

  /**
 * @method getModalExerciseMedia
 * @description
 * Retrieves media associated with the exercise currently selected in the modal.
 */
  getModalExerciseMedia(): void {
    this.exercisesService.getMedia(this.modalExercise.id).subscribe(
      (response: any) => {
        this.media = response; // Armazena os media na variável 'media'
        console.log('Exercise media loaded successfully:', this.media);
      },
      (error: any) => {
        this.errorMessage = error.error?.message || 'An unexpected error occurred'; // Define a mensagem de erro se a requisição falhar
        console.log('Error loading exercise media:', error);
      }
    );
  }

  /**
 * @method addRoutine
 * @description
 * Adds the selected exercise to a routine using the ExercisesService.
 * Requires a selected routine and a modal exercise. On success, a success message is shown.
 */
  addRoutine(): void {
    if (!this.selectedRoutine || !this.modalExercise) {
      this.addErrorMessage = 'Please select a routine before adding.';
      return;
    }

    const exerciseId = this.modalExercise.id;
    const routineId = this.selectedRoutine;

    if (this.selectedOption === 'duration') {
      this.reps = 0;
    } else if (this.selectedOption === 'reps') {
      this.duration = 0;
    }

    this.exercisesService.addRoutine(routineId, exerciseId, this.reps, this.duration, this.sets).subscribe(
      (response: any) => {
        this.successMessage = response.message;
        this.addErrorMessage = '';

        setTimeout(() => {
          this.closeModal();
        }, 2000);
      },
      (error: any) => {
        this.addErrorMessage = error.error?.message || 'An error occurred';
        this.successMessage = '';
      }
    );
  }

  /**
 * @method addExercise
 * @description
 * Adds a new exercise by calling the ExercisesService with the new exercise details.
 * On success, the modal is closed, the page is reloaded, and a success message is displayed.
 */
  addExercise(): void {


    this.exercisesService.addExercise(this.newName, this.newDescription, this.newType, this.newDifficultyLevel,
      this.newMuscleGroup, this.newEquipmentNecessary, this.newMediaName, this.newMediaType, this.newMediaUrl,
      this.newMediaName1, this.newMediaType1, this.newMediaUrl1, this.newMediaName2, this.newMediaType2, this.newMediaUrl2).subscribe(
      (response: any) => {
        this.successMessage = response.message;
        this.errorMessage = '';

        setTimeout(() => {
          this.closeModal();
          window.location.reload();
        }, 2000);
      },
      (error: any) => {
        this.errorMessage = error.error?.message || 'An unexpected error occurred';
        this.successMessage = '';
      }
    );
  }

  /**
 * @method convertToEmbedUrl
 * @description
 * Converts a given YouTube URL into an embeddable URL. If the URL does not match the YouTube pattern,
 * the original URL is returned.
 *
 * @param url - The original URL.
 * @returns The embeddable URL.
 */
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

  /**
 * @method getTypeClass
 * @description
 * Returns a CSS class based on the exercise type.
 *
 * @param type - The exercise type.
 * @returns The corresponding CSS class.
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
 * @returns An object with CSS style properties.
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
 * @method openModal
 * @description
 * Opens a modal dialog for the specified modal type and sets the modalExercise.
 * Also retrieves media for the selected exercise.
 *
 * @param modalType - The type of modal to open.
 * @param exercise - The exercise to display in the modal.
 */
  openModal(modalType: string, exercise: any): void {
    this.activeModal = modalType; // Define que a modal 'details' será exibida
    this.modalExercise = exercise; // Define o exercício selecionado para ser exibido na modal
    this.getModalExerciseMedia();
  }

  /**
 * @method closeModal
 * @description
 * Closes the active modal and resets related state variables.
 */
  closeModal() {
    this.activeModal = ''; // Fechar a modal
    this.modalExercise = null; // Limpa o exercício selecionado
    this.media = [];
    this.activeMediaIndex = 0;
    this.errorMessage = '';
    this.successMessage = '';
    this.addErrorMessage = '';
    this.addRoutineErrorMessage = '';
  }

  /**
 * @method openAddModal
 * @description Opens the modal for adding an exercise to a routine.
 *
 * @param exercise - The exercise to add.
 */
  openAddModal(exercise: any): void {
    this.activeModal = 'add';
    this.modalExercise = exercise;
    this.getRoutines();
  }

  /**
 * @method openAddExerciseModal
 * @description Opens the modal for adding a new exercise.
 */
  openAddExerciseModal(): void {
    this.activeModal = 'addExercise';
  }

  /**
 * @method previousMedia
 * @description
 * Navigates to the previous media item in the media array.
 */
  previousMedia() {
    if (this.activeMediaIndex > 0) {
      this.activeMediaIndex--;
    }
  }

  /**
 * @method nextMedia
 * @description
 * Navigates to the next media item in the media array.
 */
  nextMedia() {
    if (this.activeMediaIndex < this.media.length - 1) {
      this.activeMediaIndex++;
    }
  }

  /**
 * @method getFilteredExercises
 * @description
 * Retrieves exercises that match the filter criteria using the ExercisesService.
 */
  getFilteredExercises(): void {

    this.exercisesService.getFilteredExercises(this.filters).subscribe(
      (response: any) => {
        this.exercises = response;
        console.log('Exercises filtered Successfully:', this.exercises);
      },
      (error) => {
        this.errorMessage = error.error?.message || 'An unexpected error occurred'; // Define a mensagem de erro se a requisição falhar
        console.log('Error filtering exercises:', error);
      }
    )
  }

  /**
 * @method sortExercises
 * @description
 * Sorts the current list of exercises based on the selected sorted option.
 */
  sortExercises(): void {
    this.exercises = this.getSortedExercises(this.selectedSortedOption, this.exercises);
    console.log('Exercises sorted successfully:', this.exercises);
  }

  /**
 * @method getSortedExercises
 * @description
 * Returns a sorted array of exercises based on the provided sorted option.
 *
 * @param sortedOption - The option to sort by (e.g., 'name-asc', 'difficulty-asc').
 * @param exercises - The array of exercises to sort.
 * @returns A sorted array of exercises.
 */
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

  /**
 * @method onOptionChange
 * @description
 * Handler that resets certain exercise parameters when the selected option changes.
 * If the selected option is 'duration', reps are reset to 0, and vice versa.
 */
  onOptionChange() {
    if (this.selectedOption === 'duration') {
      this.reps = 12;
    } else if (this.selectedOption === 'reps') {
      this.duration = 60;
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


  title = 'vitalease.client';
}

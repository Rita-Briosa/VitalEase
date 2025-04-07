import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { TrainingRoutinesService } from '../services/training-routines.service';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

/**
 * @component ManageTrainingRoutinesComponent
 * @description
 * The ManageTrainingRoutinesComponent is used for managing training routines within the application.
 * It allows administrators to view, filter, sort, add, and navigate routines. The component also enables
 * the selection of exercises to be added to a routine, and provides user feedback through error and success messages.
 *
 * @dependencies
 * - TrainingRoutinesService: Provides methods for fetching, adding, and filtering routines, as well as retrieving available exercises.
 * - AuthService: Validates the user's session token and provides user information.
 * - Router: Facilitates navigation to different routes.
 * - HttpClient: Used for making HTTP requests when needed.
 *
 * @usage
 * This component is not standalone and uses external templates and styles:
 *   - Template: './manage-training-routines.component.html'
 *   - Styles: ['./manage-training-routines.component.css']
 */
@Component({
  selector: 'app-manage-training-routines',
  templateUrl: './manage-training-routines.component.html',
  standalone: false,
  styleUrls: ['./manage-training-routines.component.css']
})
export class ManageTrainingRoutinesComponent implements OnInit {
  filters: any = {
    name: '',
    type: '',
    difficultyLevel: null,
    numberOfExercises: null,
    equipmentNeeded: '',
  };

  userInfo: any = null;
  isLoggedIn: boolean = false;
  isAdmin: boolean = false;
  routines: any[] = []; // Array para armazenar as routines
  errorMessage: string = '';
  addRoutineErrorMessage: string = '';
  activeModal: string = '';
  successMessage: string = '';
  newName: string = '';
  newDescription: string = '';
  newType: string = '';
  newRoutineLevel: string = '';
  newNeeds: string = '';
  exercises: any[] = []; // Lista de exercícios disponíveis
  selectedExercises: number[] = []; // IDs dos exercícios selecionados
  selectedSortOption: string = '';

  constructor(
    private trainingRoutinesService: TrainingRoutinesService,
    private authService: AuthService,
    private router: Router,
    private http: HttpClient) { }

  /**
 * @method ngOnInit
 * @description
 * Lifecycle hook that is executed upon component initialization.
 * It validates the user's session token, retrieves user information, and loads training routines.
 * If the token is invalid or missing, the user is redirected to the login page.
 */
  ngOnInit() {

    const token = this.authService.getSessionToken();

    if (token) {
      this.authService.validateSessionToken().subscribe(
        (response: any) => {
          this.isLoggedIn = true;
          this.userInfo = response.user;
          this.getRoutines();
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
 * @method toggleExerciseSelection
 * @description
 * Adds or removes an exercise from the selectedExercises array based on whether it is already selected.
 *
 * @param exerciseId - The ID of the exercise to toggle.
 */
  toggleExerciseSelection(exerciseId: number) {
    const index = this.selectedExercises.indexOf(exerciseId);
    if (index === -1) {
      this.selectedExercises.push(exerciseId);
    } else {
      this.selectedExercises.splice(index, 1);
    }
  }

  /**
   * @method exerciseSelected
   * @description
   * Checks if an exercise is selected based on its ID.
   *
   * @param exerciseId - The ID of the exercise.
   * @returns A boolean indicating whether the exercise is selected.
   */
  exerciseSelected(exerciseId: number): boolean {
    const exercise = this.exercises.find(ex => ex.id === exerciseId);
    return exercise ? exercise.selected : false;
  }

  /**
   * @method getExercises
   * @description
   * Retrieves the list of exercises available to add to a routine by calling the TrainingRoutinesService.
   */
  getExercises(): void {
    this.trainingRoutinesService.getExercisesToAddToRoutine().subscribe(
      (response: any) => {
        this.exercises = response; // Armazena as routines na variável 'routines'
        console.log('Exercises loaded successfully:', this.exercises);
      },
      (error: any) => {
        this.addRoutineErrorMessage = error.error?.message || 'An unexpected error occurred'; // Define a mensagem de erro se a requisição falhar
        console.log('Error loading exercises:', error);
      }
    );
  }

  /**
   * @method getRoutines
   * @description
   * Retrieves the list of training routines by calling the TrainingRoutinesService.
   */
  getRoutines(): void {
    this.trainingRoutinesService.getRoutines().subscribe(
      (response: any) => {
        this.routines = response; // Armazena as routines na variável 'routines'
        console.log('Routines loaded successfully:', this.routines);
      },
      (error: any) => {
        this.errorMessage = error.error?.message; // Define a mensagem de erro se a requisição falhar
        console.log('Error loading routines:', error);
      }
    );

  }

  /**
   * @method getTypeClass
   * @description
   * Returns a CSS class based on the routine type.
   *
   * @param type - The routine type.
   * @returns A string containing the corresponding CSS class.
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
   * Returns an inline style object based on the routine type.
   *
   * @param type - The routine type.
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
   * @method navigateToRoutine
   * @description Navigates to the routine details page.
   *
   * @param routineId - The ID of the routine.
   */
  navigateToRoutine(routineId: number) {
    this.router.navigate(['/training-routine-details', routineId]);
  }

  /**
   * @method navigateToRoutineProgress
   * @description Navigates to the routine progress page.
   *
   * @param routineId - The ID of the routine.
   */
  navigateToRoutineProgress(routineId: number) {
    this.router.navigate(['/training-routine-progress', routineId]);
  }

  /**
   * @method openAddRoutineModal
   * @description Opens the modal dialog for adding a new routine and loads available exercises.
   */
  openAddRoutineModal(): void {
    this.activeModal = 'addRoutine';
    this.getExercises();
  }

  /**
   * @method closeModal
   * @description Closes the active modal and resets error and success messages.
   */
  closeModal() {
    this.activeModal = ''; // Fechar a modal
    this.errorMessage = '';
    this.successMessage = '';
    this.addRoutineErrorMessage = '';
  }

  /**
   * @method addRoutine
   * @description
   * Adds a new training routine by calling the TrainingRoutinesService with the provided details and selected exercises.
   * On success, a success message is displayed and the page is reloaded.
   */
  addRoutine(): void {

    this.trainingRoutinesService.addRoutine(this.newName, this.newDescription, this.newType, this.newRoutineLevel, this.newNeeds, this.selectedExercises)
      .subscribe(
        (response: any) => {
          this.successMessage = response.message;
          this.errorMessage = '';

          setTimeout(() => {
            this.closeModal();
            window.location.reload();
          }, 2000);
        },
        (error: any) => {
          this.addRoutineErrorMessage = error.error?.message || 'An error occurred';
          this.successMessage = '';
        }
      );
  }

  /**
   * @method getFilteredRoutines
   * @description
   * Retrieves routines that match the filter criteria by calling the TrainingRoutinesService.
   */
  getFilteredRoutines(): void {

    this.trainingRoutinesService.getFilteredRoutines(this.filters).subscribe(
      (response: any) => {
        this.routines = response;
        console.log('Exercises filtered Successfully:', this.routines);
      },
      (error) => {
        this.errorMessage = error.error?.message || 'An unexpected error occurred'; // Define a mensagem de erro se a requisição falhar
        console.log('Error filtering exercises:', error);
      }
    )
  }

  /**
   * @method sortRoutines
   * @description
   * Sorts the current list of routines based on the selected sort option.
   */
  sortRoutines(): void {
    this.routines = this.getSortedRoutines(this.selectedSortOption, this.routines);
    console.log('Exercises sorted successfully:', this.routines);
  }

  /**
   * @method getSortedRoutines
   * @description
   * Returns a sorted array of routines based on the provided sort option.
   *
   * @param sortOption - The sorting option (e.g., 'name-asc', 'difficulty-asc').
   * @param routines - The array of routines to sort.
   * @returns A sorted array of routines.
   */
  getSortedRoutines(sortOption: string, routines: any[]): any[] {
    if (!sortOption || !routines || routines.length === 0) {
      return routines; // Se não houver opção de ordenação ou exercícios, retorna a lista original
    }

    // Mapeia os níveis de dificuldade para números
    const difficultyOrder: { [key: string]: number } = {
      'Beginner': 0,
      'Intermediate': 1,
      'Advanced': 2
    };

    return [...routines].sort((a, b) => {
      switch (sortOption) {
        case 'name-asc':
          return a.name.localeCompare(b.name);
        case 'name-desc':
          return b.name.localeCompare(a.name);
        case 'difficulty-asc':
          return a.level - b.level; // Ascendente
        case 'difficulty-desc':
          return b.level - a.level; // Descendente
        case 'number-of-exercises-asc':
          return a.exerciseCount - b.exerciseCount;
        case 'number-of-exercises-desc':
          return b.exerciseCount - a .exerciseCount;
      }
    });
  }

  title = 'vitalease.client';
}



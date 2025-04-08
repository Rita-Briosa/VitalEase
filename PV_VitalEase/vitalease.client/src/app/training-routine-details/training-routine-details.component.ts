import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { TrainingRoutinesService } from '../services/training-routines.service';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

/**
 * @component TrainingRoutineDetailsComponent
 * @description
 * The TrainingRoutineDetailsComponent displays the details of a specific training routine, including the list of exercises 
 * associated with the routine. Upon initialization, it retrieves the routine ID from the route parameters, validates the user’s 
 * session token via the AuthService, and loads the exercises for the routine using the TrainingRoutinesService.
 *
 * If the routine ID is missing or the user is not authenticated, appropriate error messages are set and the user is redirected 
 * to the login page.
 *
 * @dependencies
 * - TrainingRoutinesService: Provides methods to retrieve exercises associated with a specific training routine.
 * - AuthService: Handles session token retrieval, validation, and user authentication.
 * - ActivatedRoute: Extracts parameters from the current route (used here to get the routine ID).
 * - Router: Facilitates navigation between routes.
 * - HttpClient: Used for HTTP operations if needed.
 *
 * @usage
 * This component is not standalone and uses external templates and styles:
 *   - Template: './training-routine-details.component.html'
 *   - Styles: ['./training-routine-details.component.css']
 */
@Component({
  selector: 'app-training-routine-details',
  templateUrl: './training-routine-details.component.html',
  standalone: false,
  styleUrls: ['./training-routine-details.component.css'] // Correção do nome
})
export class TrainingRoutineDetailsComponent {

  routineId: string | null = null;
  exercises: any[] = []; // Lista de exercícios
  userInfo: any = null;
  isLoggedIn: boolean = false;
  isAdmin: boolean = false;
  errorMessage: string = '';

  constructor(
    private trainingRoutinesService: TrainingRoutinesService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router, // Injeção correta do Router
    private http: HttpClient
  ) { }

  /**
   * @method ngOnInit
   * @description
   * Lifecycle hook that initializes the component. It extracts the routine ID from the route parameters, validates the 
   * user's session token, and retrieves the list of exercises for the routine. If the routine ID is not provided or the token 
   * is invalid, the component sets an error message and redirects the user as necessary.
   */
  ngOnInit() {
    this.routineId = this.route.snapshot.paramMap.get('id');

  
    if (!this.routineId) {
      this.errorMessage = "Routine ID not provided";
      return;
    }

    const token = this.authService.getSessionToken();

    if (token) {
      this.authService.validateSessionToken().subscribe(
        (response: any) => {
          this.isLoggedIn = true;
          this.userInfo = response.user;
          this.isAdmin = this.userInfo.type === 1; // Define se o usuário é admin
          this.getExercises();
        },
        (error) => {
          this.authService.logout();
          this.router.navigate(['/login']); // Correção na navegação
        }
      );
    } else {
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
   * @method getExercises
   * @description
   * Retrieves the list of exercises associated with the training routine by calling the TrainingRoutinesService.
   * If the routine ID is missing, it sets an appropriate error message.
   */
  getExercises(): void {
    if (!this.routineId) {
      this.errorMessage = "Routine ID is missing.";
      return;
    }

    this.trainingRoutinesService.getExercises(this.routineId).subscribe(
      (response: any) => {
        this.exercises = response.exerciseDtos;
        console.log('Exercises loaded successfully:', this.exercises);
      },
      (error: any) => {
        this.errorMessage = 'Error fetching exercises from routine';
        console.error('Error loading exercises:', error);
      }
    );
  }
}

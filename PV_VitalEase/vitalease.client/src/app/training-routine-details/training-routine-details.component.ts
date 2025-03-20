import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { TrainingRoutinesService } from '../services/training-routines.service';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

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

  goToDashboard() {
    this.router.navigate(['/dashboard']);
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
      },
      (error: any) => {
        this.errorMessage = 'Error fetching exercises from routine';
        console.error('Error loading exercises:', error);
      }
    );
  }
}

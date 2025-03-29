import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { TrainingRoutinesService } from '../services/training-routines.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-edit-custom-training-routine',
  standalone: false,
  
  templateUrl: './edit-custom-training-routine.component.html',
  styleUrl: './edit-custom-training-routine.component.css'
})
export class EditCustomTrainingRoutineComponent {
  userInfo: any = null;
  isLoggedIn: boolean = false;

  routineId: string | null = null;
  exercises: any[] = []; // Lista de exercícios
  warmUpExercises: any[] = [];
  mainExercises: any[] = [];
  coolDownExercises: any[] = [];

  constructor(private authService: AuthService, private routinesService: TrainingRoutinesService, private router: Router, private route: ActivatedRoute,) { }

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

}

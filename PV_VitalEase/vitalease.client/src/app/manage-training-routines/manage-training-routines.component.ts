import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { TrainingRoutinesService } from '../services/training-routines.service';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

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

  ngOnInit() {

    const token = this.authService.getSessionToken();

    if (token) {
      this.authService.validateSessionToken().subscribe(
        (response: any) => {
          this.isLoggedIn = true;
          this.userInfo = response.user;
          this.getRoutines();
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

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  toggleExerciseSelection(exerciseId: number) {
    const index = this.selectedExercises.indexOf(exerciseId);
    if (index === -1) {
      this.selectedExercises.push(exerciseId);
    } else {
      this.selectedExercises.splice(index, 1);
    }
  }

  exerciseSelected(exerciseId: number): boolean {
    const exercise = this.exercises.find(ex => ex.id === exerciseId);
    return exercise ? exercise.selected : false;
  }

  getExercises(): void {
    this.trainingRoutinesService.getExercisesToAddToRoutine().subscribe(
      (response: any) => {
        this.exercises = response; // Armazena as routines na variável 'routines'
        console.log('Exercises loaded successfully:', this.exercises);
      },
      (error: any) => {
        this.errorMessage = error.error?.message || 'An unexpected error occurred'; // Define a mensagem de erro se a requisição falhar
        console.log('Error loading exercises:', error);
      }
    );
  }

  getRoutines(): void {
    this.trainingRoutinesService.getRoutines(this.userInfo.id).subscribe(
      (response: any) => {
        this.routines = response; // Armazena as routines na variável 'routines'
        console.log('Routines loaded successfully:', this.routines);
      },
      (error: any) => {
        this.errorMessage = error.error?.message; // Define a mensagem de erro se a requisição falhar
        console.log('Error loading routines:', error);
      }
    );

    this.getFilteredRoutines();
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

  navigateToRoutine(routineId: number) {
    this.router.navigate(['/training-routine-details', routineId]);
  }

  navigateToRoutineProgress(routineId: number) {
    this.router.navigate(['/training-routine-progress', routineId]);
  }

  openAddRoutineModal(): void {
    this.activeModal = 'addRoutine';
  }

  closeModal() {
    this.activeModal = ''; // Fechar a modal
    this.errorMessage = '';
    this.successMessage = '';
  }

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
          this.errorMessage = error.error?.message || 'An error occurred';
          this.successMessage = '';
        }
      );
  }

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

  sortRoutines(): void {
    this.routines = this.getSortedRoutines(this.selectedSortOption, this.routines);
    console.log('Exercises sorted successfully:', this.routines);
  }

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

  /*// Check if user is logged in by fetching the user info
  this.isLoggedIn = this.authService.isLoggedIn();  // A verificação da sessão agora é feita com o método isLoggedIn()
  if (this.isLoggedIn) {
    this.userInfo = this.authService.getUserInfo();  // Se estiver logado, pega as informações do usuário
  }
}*/

  title = 'vitalease.client';
}



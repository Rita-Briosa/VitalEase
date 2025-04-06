import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { TrainingRoutinesService } from '../services/training-routines.service';

@Component({
  selector: 'app-custom-training-routines',
  standalone: false,
  
  templateUrl: './custom-training-routines.component.html',
  styleUrl: './custom-training-routines.component.css'
})
export class CustomTrainingRoutinesComponent {
  errorMessage: string = '';
  successMessage: string = '';
  activeModal: string = '';

  userInfo: any = null;
  isLoggedIn: boolean = false;
  routines: any = [];
  isAdmin: boolean = false;

  newName: string = '';
  newDescription: string = '';
  newType: string = '';
  newRoutineLevel: string = '';
  newNeeds: string = '';

  selectedRoutineId: number = 0;


  constructor(private authService: AuthService, private routinesService: TrainingRoutinesService, private router: Router) { }

  ngOnInit() {
    // Check if user is logged in by fetching the user info
    /* this.isLoggedIn = this.authService.isLoggedIn();  // A verificação da sessão agora é feita com o método isLoggedIn()
     if (this.isLoggedIn) {
       this.userInfo = this.authService.getUserInfo();  // Se estiver logado, pega as informações do usuário
     }
   }*/

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
      //No token found, redirect to login
      this.router.navigate(['/login']);
    }

  }

  getRoutines() {
    this.routinesService.getCustomTrainingRoutines(this.userInfo.id).subscribe(
      (response: any) => {
        this.routines = response;
        console.log("Custom Routines loaded successfully!");
      },
      (error: any) => {
        this.errorMessage = error.error?.message; // Define a mensagem de erro se a requisição falhar
        console.log('Error loading Custom Routines:', error);
      }
    );
  }
  
  addCustomRoutine() {
    console.log(this.userInfo.id);

    this.routinesService.addCustomRoutine(this.userInfo.id, this.newName, this.newDescription, this.newType, this.newRoutineLevel, this.newNeeds).subscribe(
      (response: any) => {
        console.log(response.message);
        console.log(response);
        this.closeModal();
        this.router.navigate([`/edit-custom-training-routine/${response.routineId}`]);
      },
      (error: any) => {
        console.log(error.error?.message);
      }
    );
  }

  deleteCustomRoutine(routineId: number) {
    this.routinesService.deleteRoutine(routineId).subscribe(
      (response: any) => {
        this.unselectRoutine();
        window.location.reload();
        console.log(response);
      },
      (error: any) => {
        this.unselectRoutine();
        console.log(error.error?.message);
      }
    )
  }

  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  selectRoutine(routineId: number) {
    this.selectedRoutineId = routineId;
  }

  unselectRoutine() {
    this.selectedRoutineId = 0;
  }

  openAddRoutineModal(): void {
    this.activeModal = 'addRoutine';
  }

  openDeleteRoutineModal(routineId: number): void {
    this.selectRoutine(routineId);
    this.activeModal = 'deleteRoutine';
  }

  closeModal() {
    this.unselectRoutine();
    this.activeModal = ''; // Fechar a modal
    this.errorMessage = '';
    this.successMessage = '';
  }



  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }

}

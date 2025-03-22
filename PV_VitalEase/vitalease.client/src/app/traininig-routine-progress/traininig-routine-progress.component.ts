import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { TrainingRoutinesService } from '../services/training-routines.service';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';


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

}

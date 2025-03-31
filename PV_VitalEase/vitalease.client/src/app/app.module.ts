
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms'; // Importando FormsModule
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HomePageComponent } from './homePage/homePage.component';
import { LoginComponent } from './login/login.component';
import { DashboardComponent } from './Dashboard/dashboard.component';
import { ForgotPassComponent } from './forgot-pass/forgot-pass.component';
import { ResetPassComponent } from './reset-pass/reset-pass.component';
import { AboutUsComponent } from './about-us/about-us.component';
import { RegisterComponent } from './register/register.component';
import { VerifyEmailComponent } from './verifyEmail/verify-email.component';



import { MyProfileComponent } from './MyProfile/myProfile.component';
import { ConfirmNewEmailComponent } from './ConfirmNewEmail/confirmNewEmail.component';
import { ConfirmOldEmailComponent } from './ConfirmOldEmail/confirmOldEmail.component';
import { DeleteAccountComponent } from './delete-account/delete-account.component';
import { ChangeEmailConfirmationComponent } from './ChangeEmailConfirmation/changeEmailConfirmation.component';
import { ChangeEmailCancellationComponent } from './ChangeEmalCancellation/changeEmailCancellation.component';
import { ExercisesComponent } from './Exercises/exercises.component';
import { ManageTrainingRoutinesComponent } from './manage-training-routines/manage-training-routines.component';
import { TrainingRoutineDetailsComponent } from './training-routine-details/training-routine-details.component';
import { TrainingRoutineExerciseDetailsComponent } from './training-routine-exercise-details/training-routine-exercise-details.component';
import { TraininigRoutineProgressComponent } from './traininig-routine-progress/traininig-routine-progress.component';
import { MapComponent } from './map/map.component';
import { CustomTrainingRoutinesComponent } from './custom-training-routines/custom-training-routines.component';
import { EditCustomTrainingRoutineComponent } from './edit-custom-training-routine/edit-custom-training-routine.component';




@NgModule({
  declarations: [
    AppComponent, HomePageComponent, LoginComponent, DashboardComponent, ForgotPassComponent, ResetPassComponent, AboutUsComponent, 
    RegisterComponent, VerifyEmailComponent, MyProfileComponent, DeleteAccountComponent, ConfirmNewEmailComponent, ConfirmOldEmailComponent,
    ChangeEmailConfirmationComponent, ChangeEmailCancellationComponent, ExercisesComponent, ManageTrainingRoutinesComponent, TrainingRoutineDetailsComponent, TrainingRoutineExerciseDetailsComponent, TraininigRoutineProgressComponent, CustomTrainingRoutinesComponent, EditCustomTrainingRoutineComponent,
  ],
  imports: [
    BrowserModule, HttpClientModule, CommonModule, RouterModule, CommonModule, MapComponent,
    AppRoutingModule, FormsModule, ReactiveFormsModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }



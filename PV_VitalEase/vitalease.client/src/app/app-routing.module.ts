import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { HomePageComponent } from './homePage/homePage.component';
import { DashboardComponent } from './Dashboard/dashboard.component';
import { ForgotPassComponent } from './forgot-pass/forgot-pass.component';
import { ResetPassComponent } from './reset-pass/reset-pass.component';
import { AboutUsComponent } from './about-us/about-us.component';
import { RegisterComponent } from './register/register.component';
import { VerifyEmailComponent } from './verifyEmail/verify-email.component';
import { MyProfileComponent } from './MyProfile/myProfile.component';
import { ConfirmNewEmailComponent } from './ConfirmNewEmail/confirmNewEmail.component';
import { ConfirmOldEmailComponent } from './ConfirmOldEmail/confirmOldEmail.component';
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
const routes: Routes = [
  { path: '', component: HomePageComponent},
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'forgotPassword', component: ForgotPassComponent },
  { path: 'resetPassword', component: ResetPassComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'aboutUs', component: AboutUsComponent },
  { path: 'emailValidation', component: VerifyEmailComponent },
  { path: 'myProfile', component: MyProfileComponent },
  { path: 'confirmNewEmail', component: ConfirmNewEmailComponent },
  { path: 'confirmOldEmail', component: ConfirmOldEmailComponent },
  { path: 'changeEmailConfirmation', component: ChangeEmailConfirmationComponent },
  { path: 'changeEmailCancellation', component: ChangeEmailCancellationComponent },
  { path: 'exercises', component: ExercisesComponent },
  { path: 'manage-training-routines', component: ManageTrainingRoutinesComponent },
  { path: 'training-routine-details/:id', component: TrainingRoutineDetailsComponent },
  { path: 'training-routine-exercise-details/:id', component: TrainingRoutineExerciseDetailsComponent },
  { path: 'training-routine-progress/:id', component: TraininigRoutineProgressComponent },
  { path: 'edit-custom-training-routine/:id', component: EditCustomTrainingRoutineComponent },
  { path: 'map', component: MapComponent },
  { path: 'manage-training-routines/custom-training-routines', component: CustomTrainingRoutinesComponent },
  { path: '**', redirectTo: '' }]


@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }


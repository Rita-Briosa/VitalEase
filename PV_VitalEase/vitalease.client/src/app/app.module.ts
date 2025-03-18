
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms'; // Importando FormsModule
import { CommonModule } from '@angular/common';

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



@NgModule({
  declarations: [
    AppComponent, HomePageComponent, LoginComponent, DashboardComponent, ForgotPassComponent, ResetPassComponent, AboutUsComponent,
    RegisterComponent, VerifyEmailComponent, MyProfileComponent, DeleteAccountComponent, ConfirmNewEmailComponent, ConfirmOldEmailComponent,
    ChangeEmailConfirmationComponent, ChangeEmailCancellationComponent,ExercisesComponent, ManageTrainingRoutinesComponent
  ],
  imports: [
    BrowserModule, HttpClientModule, CommonModule,
    AppRoutingModule, FormsModule, ReactiveFormsModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }



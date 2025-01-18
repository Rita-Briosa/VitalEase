import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { HomePageComponent } from './homePage/homePage.component';
import { DashboardComponent } from './Dashboard/dashboard.component';
import { ForgotPassComponent } from './forgot-pass/forgot-pass.component';
import { ResetPassComponent } from './reset-pass/reset-pass.component';
import { AboutUsComponent } from './about-us/about-us.component';
//import { RegisterComponent } from './register/register.component';

const routes: Routes = [
  { path: '', component: HomePageComponent },
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'forgotPassword', component: ForgotPassComponent },
  { path: 'resetPassword', component: ResetPassComponent },
  { path: 'aboutUs', component: AboutUsComponent },
  { path: '**', redirectTo: '' }]
//{ path: 'register', component: RegisterComponent }];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }


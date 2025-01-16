import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { homePageComponent } from './homePage/homePage.component';
//import { RegisterComponent } from './register/register.component';

const routes: Routes = [
  { path: '', component: homePageComponent},
  { path: 'login', component: LoginComponent },
  { path: '**', redirectTo: '' }]
  //{ path: 'register', component: RegisterComponent }];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }

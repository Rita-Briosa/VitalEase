import { Component } from '@angular/core';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent {
  onSubmit() {
    // Logic for submitting login credentials
    console.log('Login form submitted');
  }
}

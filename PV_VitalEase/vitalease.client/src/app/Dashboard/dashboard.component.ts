import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router'; // Para redirecionamento
import { LogsService } from '../services/logs.service'; // Importe o serviço de logs
import { AuthService } from '../services/auth.service';

/**
 * @component DashboardComponent
 * @description
 * The DashboardComponent is responsible for displaying and filtering application logs.
 * It validates the user's session token on initialization, retrieves the user's email, and then loads all logs.
 * Administrators can filter logs based on various criteria, including user ID, email, date range, action type, and status.
 *
 * @dependencies
 * - LogsService: Provides methods to retrieve and filter logs.
 * - Router: Handles navigation between routes.
 * - AuthService: Handles user authentication and session token validation.
 *
 * @usage
 * This component is not standalone and uses external templates and styles:
 * - Template: './dashboard.component.html'
 * - Styles: './dashboard.component.css'
 */
@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  standalone: false,
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  filters: any = {
    userId: '',
    userEmail: '',
    dateFrom: '',
    dateTo: '',
    actionType: '',
    status: ''
  };

  logs: any[] = []; // Array para armazenar os logs
  errorMessage: string = '';
  email = '';// Mensagem de erro caso algo dê errado
  user: any = null;
  isLoggedIn: boolean = false;
  constructor(private logsService: LogsService, private router: Router, private authService: AuthService) { }

  /**
  * @method ngOnInit
  * @description
  * Lifecycle hook that is executed when the component is initialized.
  * It checks for the session token and validates it. If the token is valid, the user's email is retrieved
  * and the logs are loaded. Otherwise, the user is redirected to the login page.
  */
  ngOnInit(): void {

    const token = this.authService.getSessionToken();
 
    if (token) {
      this.authService.validateSessionToken().subscribe(
        (response: any) => {
          this.isLoggedIn = true;
          this.email = response.user.email;
          this.getLogs();// Store user data
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

  /**
 * @method getLogs
 * @description
 * Retrieves all logs by calling the LogsService.
 * On success, the logs are stored in the component's logs array.
 * If an error occurs, an error message is set.
 */
  getLogs(): void {
    this.logsService.getLogs().subscribe(
      (response: any) => {
        this.logs = response; // Armazena os logs na variável 'logs'
        console.log('Logs carregados com sucesso:', this.logs);
      },
      (error) => {
        this.errorMessage = 'Erro ao carregar os logs'; // Define a mensagem de erro se a requisição falhar
        console.log('Erro ao carregar os logs:', error);
      }
    );
  }

  /**
 * @method getFilterLogs
 * @description
 * Retrieves logs that match the filter criteria specified in the filters object.
 * Converts date strings to the required date-time format and then calls the LogsService to fetch the filtered logs.
 */
  getFilterLogs(): void {

    // Converte as datas para Date, se existirem
    if (this.filters.dateFrom) {
      this.filters.dateFrom = this.convertToDateTime(this.filters.dateFrom);
    }

    if (this.filters.dateTo) {
      this.filters.dateTo = this.convertToDateTime(this.filters.dateTo);
    }

    this.logsService.getLogsFilter(this.filters).subscribe(
      (response: any) => {
        this.logs = response;
        console.log('Logs carregados com sucesso:', this.logs);
      },
      (error) => {
        this.errorMessage = 'Erro ao carregar os logs'; // Define a mensagem de erro se a requisição falhar
        console.log('Erro ao carregar os logs:', error);
      }
    )
  }
  /**
 * @method logout
 * @description
 * Logs out the current user by calling the AuthService logout method and then navigates to the homepage.
 */
    logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }

  /**
 * @method goToHome
 * @description Navigates the user to the homepage.
 */
  goToHome() {
    this.router.navigate(['/']);
  }

  /**
 * @method convertToDateTime
 * @description
 * Converts a date string into a standardized date-time string in the format 'yyyy-MM-ddTHH:mm:ss'.
 *
 * @param dateString - The input date string.
 * @returns The formatted date-time string or null if the input is empty.
 */
  convertToDateTime(dateString: string): string | null {
    if (dateString) {
      const date = new Date(dateString);
      const year = date.getFullYear();
      const month = (date.getMonth() + 1).toString().padStart(2, '0'); // Mês começa em 0, por isso somamos 1
      const day = date.getDate().toString().padStart(2, '0');
      const hours = date.getHours().toString().padStart(2, '0');
      const minutes = date.getMinutes().toString().padStart(2, '0');
      const seconds = date.getSeconds().toString().padStart(2, '0');

      return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`; // Formato yyyy-MM-ddTHH:mm:ss
    }
    return null;
  }
}

import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router'; // Para redirecionamento
import { LogsService } from '../services/logs.service'; // Importe o serviço de logs
import { AuthService } from '../services/auth.service';

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

  /*ngOnInit(): void {
    // Verifica se o usuário está logado e se o userType é 1
    this.isLoggedIn = this.authService.isAuthenticated();
    if (this.isLoggedIn) {
      this.userInfo = this.authService.getUserInfo();
      this.email = this.userInfo.email// Obtém as informações do usuário
    }

    if (!this.userInfo || this.userInfo.type !== 1) {
      // Se o usuário não estiver logado ou não for do tipo 1, redireciona para outra página
      this.router.navigate(['/']); // Redireciona para a página inicial ou página de erro
    } else {
      // Caso o usuário seja do tipo 1, carrega os logs
      this.getLogs();
    }
  }*/

  // Função para obter os logs
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

  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }

  goToHome() {
    this.router.navigate(['/']);
  }

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

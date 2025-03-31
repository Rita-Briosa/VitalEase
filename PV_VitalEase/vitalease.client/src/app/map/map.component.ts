import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import * as L from 'leaflet'; // Importa a biblioteca Leaflet para manipulação de mapas
import { CommonModule } from '@angular/common';  // Adiciona o CommonModule, necessário para usar diretivas como *ngIf
import { ChangeDetectorRef } from '@angular/core';
import { RouterModule } from '@angular/router';  // Importa o RouterModule para usar rotas no Angular

// Personalização do ícone padrão do marcador do Leaflet
L.Marker.prototype.options.icon = L.icon({
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',  // URL do ícone do marcador
  iconSize: [25, 41],  // Tamanho do ícone
  iconAnchor: [12, 41],  // Posição da âncora do ícone, para centralizar a imagem no marcador
  popupAnchor: [1, -34],  // Posição do pop-up em relação ao marcador
  shadowUrl: ''  // Não irá carregar a sombra do marcador
});

// Define o componente do mapa
@Component({
  selector: 'app-map',
  standalone: true,  // Marca o componente como independente, sem depender de um módulo externo
  imports: [CommonModule, RouterModule],  // Importa CommonModule e RouterModule
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.css']
})
export class MapComponent implements OnInit {
  // Variáveis para armazenar o estado do utilizador e do mapa
  userInfo: any = null;
  isLoggedIn: boolean = false;
  @ViewChild('map', { static: true }) mapElement!: ElementRef; // Referência ao elemento do mapa no HTML

  // Variáveis para o mapa, marcadores, rota e resumo da rota
  map!: L.Map;
  markers: L.Marker[] = [];  // Array para armazenar os marcadores
  routeLayer!: L.GeoJSON;  // Camada de rota do tipo GeoJSON
  routeSummary: { distance: string, duration: string } | null = null;  // Resumo da rota
  selectedDestination: L.LatLng | null = null;  // Destino selecionado

  constructor(private authService: AuthService, private router: Router, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    // Inicializa o mapa e verifica a sessão do utilizador
    this.initMap();
    this.checkUserSession();
  }

  // Verifica se o utilizador está autenticado
  checkUserSession() {
    const token = this.authService.getSessionToken();  // Obtém o token da sessão
    if (token) {
      // Se o token existir, valida a sessão
      this.authService.validateSessionToken().subscribe(
        (response: any) => {
          this.isLoggedIn = true;
          this.userInfo = response.user;  // Armazena as informações do utilizador
        },
        () => {
          // Caso a validação falhe, faz logout e redireciona para login
          this.authService.logout();
          this.router.navigate(['/login']);
        }
      );
    }
  }

  // Inicializa o mapa
  initMap() {
    this.map = L.map(this.mapElement.nativeElement).setView([38.521877, -8.839083], 15);  // Define a vista inicial do mapa

    // Carrega o mapa base com os tiles do OpenStreetMap
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors'
    }).addTo(this.map);

    // Adiciona um evento de clique no mapa
    this.map.on('click', (event: L.LeafletMouseEvent) => {
      this.selectedDestination = event.latlng;  // Salva a localização clicada
      this.addMarker(event.latlng);  // Adiciona um marcador no local clicado
      this.calculateRoute();  // Calcula a rota até o destino
    });
  }

  // Adiciona um marcador no mapa
  addMarker(position: L.LatLng) {
    this.markers.forEach(marker => this.map.removeLayer(marker));  // Remove qualquer marcador anterior
    this.markers = [];  // Limpa a lista de marcadores

    // Adiciona um novo marcador com o ícone padrão
    const marker = L.marker(position).addTo(this.map);

    // Adiciona um pop-up ao marcador, mostrando a distância e duração da rota
    marker.bindPopup(`
      <b>Route summary</b><br>
      Distance: ${this.routeSummary?.distance}<br>
      Time: ${this.routeSummary?.duration}
    `).openPopup();  // Abre o pop-up automaticamente

    this.markers.push(marker);  // Adiciona o marcador à lista
  }

  // Função para fechar o pop-up no mapa
  closePopup() {
    if (this.map) {
      this.map.closePopup();  // Fecha o pop-up atual do mapa
    }
  }

  // Calcula a rota entre o ponto de origem e o destino
  calculateRoute() {
    if (!this.selectedDestination) return;  // Verifica se o destino foi selecionado

    const start = '-8.839083,38.521877'; // Coordenadas fixas de origem
    const end = `${this.selectedDestination.lng},${this.selectedDestination.lat}`;  // Coordenadas do destino
    const osrmUrl = `https://router.project-osrm.org/route/v1/driving/${start};${end}?overview=full&geometries=geojson`;  // URL da API para calcular a rota

    // Realiza a requisição para obter a rota
    fetch(osrmUrl)
      .then(response => response.json())  // Converte a resposta para JSON
      .then(data => {
        if (data.routes.length > 0) {
          const route = data.routes[0];  // Obtém a primeira rota

          // Remove a camada anterior de rota, se existir
          if (this.routeLayer) {
            this.map.removeLayer(this.routeLayer);
          }

          // Adiciona a nova rota ao mapa como um GeoJSON
          this.routeLayer = L.geoJSON(route.geometry, {
            style: { color: 'blue', weight: 5 }  // Estilo da linha da rota
          }).addTo(this.map);

          // Atualiza o resumo da rota (distância e duração)
          this.routeSummary = {
            distance: (route.distance / 1000).toFixed(2) + ' km',
            duration: (route.duration / 60).toFixed(2) + ' min'
          };

          this.cdr.detectChanges(); // Força a detecção de mudanças para garantir que a UI seja atualizada
        } else {
          alert('It was not possible to calculate the route.');  // Exibe um alerta se não houver rota encontrada
        }
      })
      .catch(error => console.error('Error obtaining the route:', error));  // Trata erros na requisição
  }

  // Função para logout do utilizador
  logout() {
    this.authService.logout();  // Chama o serviço de logout
    this.isLoggedIn = false;  // Atualiza o estado de login
    this.router.navigate(['/']);  // Redireciona para a página inicial
  }
}

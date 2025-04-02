import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import * as L from 'leaflet';
import { CommonModule } from '@angular/common';
import { ChangeDetectorRef } from '@angular/core';
import { RouterModule } from '@angular/router';

// Personalização do ícone padrão do marcador do Leaflet
L.Marker.prototype.options.icon = L.icon({
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowUrl: ''
});

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: 'map.component.html',
  styleUrls: ['map.component.css']
})
export class MapComponent implements OnInit, AfterViewInit {
  userInfo: any = null;
  isLoggedIn: boolean = false;

  @ViewChild('map', { static: true }) mapElement!: ElementRef;
  @ViewChild('searchInput', { static: true }) searchInput!: ElementRef;

  map!: L.Map;
  markers: L.Marker[] = [];
  routeLayer!: L.GeoJSON;
  routeSummary: { distance: string, duration: string } | null = null;
  selectedDestination: L.LatLng | null = null;

  // Google Maps Autocomplete instance
  googleAutocomplete!: google.maps.places.Autocomplete;

  constructor(private authService: AuthService, private router: Router, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.initMap();
    this.checkUserSession();
  }

  ngAfterViewInit() {
    this.loadGoogleMapsAPI()
      .then(() => {
        this.initializeGoogleAutocomplete();
      })
      .catch(err => console.error('Erro ao carregar a Google Maps API:', err));
  }

  // Carrega a API do Google Maps dinamicamente e insere a chave diretamente
  loadGoogleMapsAPI(): Promise<void> {
    return new Promise((resolve, reject) => {
      if ((window as any).google && (window as any).google.maps) {
        resolve();
        return;
      }
      const script = document.createElement('script');
      // Chave do Google Maps inserida diretamente na URL
      script.src = `https://maps.googleapis.com/maps/api/js?key=AIzaSyCacOSrxFC3yjrFfIwqW1Y571gxtqrXEwk&libraries=places`;
      script.async = true;
      script.defer = true;
      script.onload = () => resolve();
      script.onerror = (error: any) => reject(error);
      document.head.appendChild(script);
    });
  }

  // Inicializa o Autocomplete do Google Places no input de pesquisa
  initializeGoogleAutocomplete() {
    if (!this.searchInput) {
      console.error('Elemento de pesquisa não encontrado!');
      return;
    }
    this.googleAutocomplete = new google.maps.places.Autocomplete(this.searchInput.nativeElement, {
      types: ['geocode']  // Pode ajustar os tipos conforme necessário
    });
    this.googleAutocomplete.addListener('place_changed', () => {
      const place = this.googleAutocomplete.getPlace();
      if (!place.geometry || !place.geometry.location) {
        alert('Nenhum resultado foi encontrado!');
        return;
      }
      const lat = place.geometry.location.lat();
      const lng = place.geometry.location.lng();
      const leafletLatLng = L.latLng(lat, lng);

      this.selectedDestination = leafletLatLng;
      this.addMarker(leafletLatLng);
      this.map.setView([lat, lng], 15);
      this.calculateRoute();
    });
  }

  // Verifica se o utilizador está autenticado
  checkUserSession() {
    const token = this.authService.getSessionToken();
    if (token) {
      this.authService.validateSessionToken().subscribe(
        (response: any) => {
          this.isLoggedIn = true;
          this.userInfo = response.user;
        },
        () => {
          this.authService.logout();
          this.router.navigate(['/login']);
        }
      );
    }
  }

  // Inicializa o mapa Leaflet
  initMap() {
    this.map = L.map(this.mapElement.nativeElement).setView([38.521877, -8.839083], 15);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors'
    }).addTo(this.map);

    // Mantém o evento de clique no mapa
    this.map.on('click', (event: L.LeafletMouseEvent) => {
      this.selectedDestination = event.latlng;
      this.addMarker(event.latlng);
      this.calculateRoute();
    });
  }

  // Adiciona um marcador no mapa
  addMarker(position: L.LatLng) {
    this.markers.forEach(marker => this.map.removeLayer(marker));
    this.markers = [];

    const marker = L.marker(position).addTo(this.map);
    marker.bindPopup(`
      <b>Resumo da rota</b><br>
      Distância: ${this.routeSummary?.distance ?? 'N/D'}<br>
      Tempo: ${this.routeSummary?.duration ?? 'N/D'}
    `).openPopup();
    this.markers.push(marker);
  }

  // Calcula a rota entre um ponto fixo de origem e o destino selecionado
  calculateRoute() {
    if (!this.selectedDestination) return;

    const start = '-8.839083,38.521877'; // Coordenadas de origem fixas
    const end = `${this.selectedDestination.lng},${this.selectedDestination.lat}`;
    const osrmUrl = `https://router.project-osrm.org/route/v1/driving/${start};${end}?overview=full&geometries=geojson`;

    fetch(osrmUrl)
      .then(response => response.json())
      .then(data => {
        if (data.routes.length > 0) {
          const route = data.routes[0];

          if (this.routeLayer) {
            this.map.removeLayer(this.routeLayer);
          }

          this.routeLayer = L.geoJSON(route.geometry, {
            style: { color: 'blue', weight: 5 }
          }).addTo(this.map);

          this.routeSummary = {
            distance: (route.distance / 1000).toFixed(2) + ' km',
            duration: (route.duration / 60).toFixed(2) + ' min'
          };

          this.cdr.detectChanges();
        } else {
          alert('Não foi possível calcular a rota.');
        }
      })
      .catch(error => console.error('Erro ao obter a rota:', error));
  }

  // Função para logout do utilizador
  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }
}

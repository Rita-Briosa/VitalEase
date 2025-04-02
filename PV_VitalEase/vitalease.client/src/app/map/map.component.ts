import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import * as L from 'leaflet';
import { CommonModule } from '@angular/common';
import { ChangeDetectorRef } from '@angular/core';
import { RouterModule } from '@angular/router';

// Personalização do ícone padrão do marcador do Leaflet (usado para cliques no mapa)
L.Marker.prototype.options.icon = L.icon({
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowUrl: ''
});

const redIcon = L.icon({
  iconUrl: 'http://maps.google.com/mapfiles/ms/icons/red-dot.png',
  iconSize: [32, 32],
  iconAnchor: [16, 32],
  popupAnchor: [0, -32]
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

  // Google Maps Autocomplete instance (se continuar a usar para obter um único resultado)
  googleAutocomplete!: google.maps.places.Autocomplete;

  constructor(private authService: AuthService, private router: Router, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.initMap();
    this.checkUserSession();
  }

  ngAfterViewInit() {
    this.loadGoogleMapsAPI()
      .then(() => {
        // Optionally, you may initialise autocomplete if you wish to get a single result.
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

  // (Opcional) Inicializa o Autocomplete do Google Places no input de pesquisa
  initializeGoogleAutocomplete() {
    if (!this.searchInput) {
      console.error('Elemento de pesquisa não encontrado!');
      return;
    }
    this.googleAutocomplete = new google.maps.places.Autocomplete(this.searchInput.nativeElement, {
      types: ['geocode']
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

  // Pesquisa locais (ex: "spa") e adiciona marcadores vermelhos para cada resultado
  // Pesquisa locais (ex: "spa") e adiciona múltiplos marcadores vermelhos
  searchPlaces() {
    const query = this.searchInput.nativeElement.value;
    if (!query) {
      alert("Insira um termo de pesquisa.");
      return;
    }

    // Obter os limites atuais do mapa (Leaflet) e converter para google.maps.LatLngBounds
    const bounds = this.map.getBounds();
    const southWest = bounds.getSouthWest();
    const northEast = bounds.getNorthEast();
    const googleBounds = new google.maps.LatLngBounds(
      new google.maps.LatLng(southWest.lat, southWest.lng),
      new google.maps.LatLng(northEast.lat, northEast.lng)
    );

    // Cria um serviço do Google Places
    const service = new google.maps.places.PlacesService(document.createElement('div'));
    const request = {
      query: query,
      bounds: googleBounds
    };

    service.textSearch(request, (results, status) => {
      if (status === google.maps.places.PlacesServiceStatus.OK && results) {
        // Remover marcadores de pesquisa anteriores
        this.markers.forEach(marker => this.map.removeLayer(marker));
        this.markers = [];

        results.forEach(place => {
          if (!place.geometry || !place.geometry.location) return;
          const lat = place.geometry.location.lat();
          const lng = place.geometry.location.lng();
          const leafletLatLng = L.latLng(lat, lng);

          const marker = L.marker(leafletLatLng, { icon: redIcon }).addTo(this.map);
          marker.bindPopup(`
          <b>${place.name}</b><br>
          ${place.formatted_address || ''}<br>
          ${place.rating ? 'Avaliação: ' + place.rating : ''}
        `);
          this.markers.push(marker);
        });

        // Opcional: Ajusta a vista do mapa para mostrar todos os marcadores
        const group = new L.FeatureGroup(this.markers);
        this.map.fitBounds(group.getBounds());
      } else {
        alert("Nenhum resultado foi encontrado!");
      }
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

  // Adiciona um marcador no mapa (usando o ícone vermelho)
  addMarker(position: L.LatLng) {
    // Se pretender limpar marcadores anteriores (opcional):
    this.markers.forEach(marker => this.map.removeLayer(marker));
    this.markers = [];

    const marker = L.marker(position, { icon: redIcon }).addTo(this.map);
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

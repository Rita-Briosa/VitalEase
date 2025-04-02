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

  // New property to store user's current location
  userLocation: L.LatLng | null = null;

  @ViewChild('map', { static: true }) mapElement!: ElementRef;
  @ViewChild('searchInput', { static: true }) searchInput!: ElementRef;

  map!: L.Map;
  markers: L.Marker[] = [];
  routeLayer: L.GeoJSON | null = null;
  routeSummary: { distance: string, duration: string } | null = null;
  selectedDestination: L.LatLng | null = null;

  // Google Maps Autocomplete instance (for single result use)
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

  loadGoogleMapsAPI(): Promise<void> {
    return new Promise((resolve, reject) => {
      if ((window as any).google && (window as any).google.maps) {
        resolve();
        return;
      }
      const script = document.createElement('script');
      script.src = `https://maps.googleapis.com/maps/api/js?key=AIzaSyCacOSrxFC3yjrFfIwqW1Y571gxtqrXEwk&libraries=places`;
      script.async = true;
      script.defer = true;
      script.onload = () => resolve();
      script.onerror = (error: any) => reject(error);
      document.head.appendChild(script);
    });
  }

  initializeGoogleAutocomplete() {
    if (!this.searchInput) {
      console.error('No results found. Please try again.');
      return;
    }
    this.googleAutocomplete = new google.maps.places.Autocomplete(this.searchInput.nativeElement, {
      types: ['geocode']
    });
    this.googleAutocomplete.addListener('place_changed', () => {
      const place = this.googleAutocomplete.getPlace();
      if (!place.geometry || !place.geometry.location) {
     
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

  searchPlaces() {
    const query = this.searchInput.nativeElement.value;
    if (!query) {
      alert("You need to write something.");
      return;
    }
    const center = this.map.getCenter();
    const request = {
      location: new google.maps.LatLng(center.lat, center.lng),
      radius: 5000,
      type: query.toLowerCase()
    };

    const service = new google.maps.places.PlacesService(document.createElement('div'));
    service.nearbySearch(request, (results, status) => {
      if (status === google.maps.places.PlacesServiceStatus.OK && results) {
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
            ${place.vicinity || ''}<br>
            ${place.rating ? 'Avaliação: ' + place.rating : ''}
          `);
          this.markers.push(marker);
        });
        if (this.markers.length > 0) {
          const group = new L.FeatureGroup(this.markers);
          this.map.fitBounds(group.getBounds());
        }
      } else {
        alert("Nenhum resultado foi encontrado para " + query);
      }
    });
  }

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

  initMap() {
    const defaultLocation: [number, number] = [38.521877, -8.839083]; 
    this.map = L.map(this.mapElement.nativeElement).setView(defaultLocation, 15);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors'
    }).addTo(this.map);

    // Function to add a marker at the user location (or fallback) 
    const addUserMarker = (lat: number, lng: number) => {
      this.userLocation = L.latLng(lat, lng);
      const marker = L.marker([lat, lng], { icon: redIcon }).addTo(this.map);
      marker.bindPopup('You are here.').openPopup();
    };

    // Try to get user's current location
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          const userLat = position.coords.latitude;
          const userLng = position.coords.longitude;
          this.map.setView([userLat, userLng]);
          addUserMarker(userLat, userLng);
        },
        (error) => {
          console.error('Erro ao obter a localização do utilizador:', error);
          // If error occurs or permission is denied, fall back to default location (Lisbon)
          addUserMarker(defaultLocation[0], defaultLocation[1]);
        }
      );
    } else {
      console.error('Geolocalização não é suportada pelo navegador.');
      // Fallback if geolocation is not available
      addUserMarker(defaultLocation[0], defaultLocation[1]);
    }

    // Map click event for adding a destination
    this.map.on('click', (event: L.LeafletMouseEvent) => {
      this.selectedDestination = event.latlng;
      this.addMarker(event.latlng);
      this.calculateRoute();
    });
  }

  goToUserLocation(): void {
    if (this.userLocation) {
      this.map.setView(this.userLocation, 15);
    } else {
      alert('Localização do utilizador não disponível.');
    }
  }

  addMarker(position: L.LatLng) {
    this.markers.forEach(marker => this.map.removeLayer(marker));
    this.markers = [];
    const marker = L.marker(position, { icon: redIcon }).addTo(this.map);
    marker.bindPopup(`
      <b>Route Summary</b><br>
      Distance: ${this.routeSummary?.distance ?? 'N/D'}<br>
      Time: ${this.routeSummary?.duration ?? 'N/D'}
    `).openPopup();

    marker.on('popupclose', () => {
      if (this.routeLayer) {
        this.map.removeLayer(this.routeLayer);
        this.routeLayer = null;
      }
      this.map.removeLayer(marker);
      this.markers = this.markers.filter(m => m !== marker);
    });
    this.markers.push(marker);
  }

  calculateRoute() {
    if (!this.userLocation || !this.selectedDestination) return;

    // Define as coordenadas de início (usando a localização do utilizador) e destino.
    const start = `${this.userLocation.lng},${this.userLocation.lat}`;
    const end = `${this.selectedDestination.lng},${this.selectedDestination.lat}`;

    // Modifica a URL para solicitar alternativas (várias rotas)
    const osrmUrl = `https://router.project-osrm.org/route/v1/driving/${start};${end}?overview=full&geometries=geojson&alternatives=true`;

    fetch(osrmUrl)
      .then(response => response.json())
      .then(data => {
        if (data.routes && data.routes.length > 0) {
          // Seleciona a rota com a menor distância (ou pode usar o tempo, se preferir)
          const bestRoute = data.routes.reduce((prev: any, curr: any) => {
            return (prev.distance < curr.distance) ? prev : curr;
          });

          // Remove a rota anterior, se existir
          if (this.routeLayer) {
            this.map.removeLayer(this.routeLayer);
          }

          this.routeLayer = L.geoJSON(bestRoute.geometry, {
            style: { color: 'red', weight: 5 }
          }).addTo(this.map);

          // Atualiza o resumo da rota (distância e tempo)
          this.routeSummary = {
            distance: (bestRoute.distance / 1000).toFixed(2) + ' km',
            duration: (bestRoute.duration / 60).toFixed(2) + ' min'
          };

          this.cdr.detectChanges();
        } else {
          alert('Não foi possível calcular a rota.');
        }
      })
      .catch(error => console.error('Erro ao obter a rota:', error));
  }

  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }
}

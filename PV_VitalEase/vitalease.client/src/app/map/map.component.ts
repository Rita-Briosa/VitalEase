import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import * as L from 'leaflet';
import { CommonModule } from '@angular/common';  // Adicionando CommonModule
import { ChangeDetectorRef } from '@angular/core';
import { RouterModule } from '@angular/router';  // Certifique-se de importar RouterModule

L.Marker.prototype.options.icon = L.icon({
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',  // URL do ícone
  iconSize: [25, 41],  // Tamanho do ícone
  iconAnchor: [12, 41],  // Posição do âncora do ícone
  popupAnchor: [1, -34],  // Posição do pop-up
  shadowUrl: ''  // Remover sombra (não vai buscar a imagem "marker-shadow.png")
});

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule, RouterModule],  // Certifique-se de importar o CommonModule aqui
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.css']
})
export class MapComponent implements OnInit {
  userInfo: any = null;
  isLoggedIn: boolean = false;
  @ViewChild('map', { static: true }) mapElement!: ElementRef;

  map!: L.Map;
  markers: L.Marker[] = [];
  routeLayer!: L.GeoJSON;
  routeSummary: { distance: string, duration: string } | null = null;
  selectedDestination: L.LatLng | null = null;

  constructor(private authService: AuthService, private router: Router, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.initMap();
    this.checkUserSession();
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
    this.map = L.map(this.mapElement.nativeElement).setView([38.521877, -8.839083], 15);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors'
    }).addTo(this.map);

    this.map.on('click', (event: L.LeafletMouseEvent) => {
      this.selectedDestination = event.latlng;
      this.addMarker(event.latlng);
      this.calculateRoute();
    });
  }

  
 

  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }
}

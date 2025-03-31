import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
@Component({
  selector: 'app-map',
  standalone: false,
  
  templateUrl: './map.component.html',
  styleUrl: './map.component.css'
})
export class MapComponent implements OnInit {
  userInfo: any = null;
  isLoggedIn: boolean = false;
  private googleMapsApiKey: string = 'AIzaSyCacOSrxFC3yjrFfIwqW1Y571gxtqrXEwk';
  @ViewChild('map', { static: true }) mapElement!: ElementRef;

  map!: google.maps.Map;
  searchBox!: google.maps.places.SearchBox;
  markers: google.maps.Marker[] = [];

  loadGoogleMaps() {
    const script = document.createElement('script');
    script.src = `https://maps.googleapis.com/maps/api/js?key=${this.googleMapsApiKey}&libraries=places`;
    script.defer = true;
    script.async = true;
    script.onload = () => this.initMap();
    document.head.appendChild(script);
  }

  initMap() {
    const initialLocation = new google.maps.LatLng(38.7223, -9.1393); // Example: Lisbon

    this.map = new google.maps.Map(this.mapElement.nativeElement, {
      center: initialLocation,
      zoom: 12
    });

    const input = document.getElementById('pac-input') as HTMLInputElement;
    this.searchBox = new google.maps.places.SearchBox(input);
    this.map.controls[google.maps.ControlPosition.TOP_LEFT].push(input);

    this.map.addListener('bounds_changed', () => {
      this.searchBox.setBounds(this.map.getBounds() as google.maps.LatLngBounds);
    });

    this.searchBox.addListener('places_changed', () => {
      const places = this.searchBox.getPlaces();

      if (!places || places.length === 0) {
        alert('No results found. Please try again.');
        return;
      }

      // Remove previous markers
      this.markers.forEach(marker => marker.setMap(null));
      this.markers = [];

      const bounds = new google.maps.LatLngBounds();

      places.forEach(place => {
        if (!place.geometry || !place.geometry.location) return;

        const marker = new google.maps.Marker({
          map: this.map,
          position: place.geometry.location,
          title: place.name
        });

        this.markers.push(marker);

        if (place.geometry.viewport) {
          bounds.union(place.geometry.viewport);
        } else {
          bounds.extend(place.geometry.location);
        }
      });

      this.map.fitBounds(bounds);
    });
  }

  constructor(private authService: AuthService, private router: Router) { }

  ngOnInit() {

    this.loadGoogleMaps();
    // Check if user is logged in by fetching the user info
    /* this.isLoggedIn = this.authService.isLoggedIn();  // A verificação da sessão agora é feita com o método isLoggedIn()
     if (this.isLoggedIn) {
       this.userInfo = this.authService.getUserInfo();  // Se estiver logado, pega as informações do usuário
     }
   }*/

    const token = this.authService.getSessionToken();

    if (token) {
      this.authService.validateSessionToken().subscribe(
        (response: any) => {
          this.isLoggedIn = true;
          this.userInfo = response.user;
        },
        (error) => {
          this.authService.logout();
          this.router.navigate(['/login']);
        }
      );
    } else {
      // No token found, redirect to login
      //this.router.navigate(['/login']);
    }

  }
  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.router.navigate(['/']);
  }
}

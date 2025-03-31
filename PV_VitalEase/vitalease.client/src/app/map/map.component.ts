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

  //serviço google para calcular as rotas
  directionsService!: google.maps.DirectionsService;
  //Renderizar a rota no mapa
  directionsRenderer!: google.maps.DirectionsRenderer;
  //Guardar o local escolhid pelo user
  selectedDestination: google.maps.LatLng | null = null;
  // Guaradar os dados do resumo da rota (distância e tempo estimado)
  routeSummary: { distance: string, duration: string } | null = null;


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

    //Configuração para o Google Maps para poder mostrar as rotas traçadas
    this.directionsService = new google.maps.DirectionsService();
    this.directionsRenderer = new google.maps.DirectionsRenderer();
    this.directionsRenderer.setMap(this.map);


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

      this.markers.forEach(marker => marker.setMap(null));
      this.markers = [];

      const bounds = new google.maps.LatLngBounds();
      places.forEach(place => {
        if (!place.geometry || !place.geometry.location) return;

        this.selectedDestination = place.geometry.location;

        const marker = new google.maps.Marker({
          map: this.map,
          position: place.geometry.location,
          title: place.name
        });

        this.markers.push(marker);
        bounds.extend(place.geometry.location);
      });

      this.map.fitBounds(bounds);
      if (this.selectedDestination) this.calculateRoute(); // Agora chama a função de cálculo de rota
    });
  }


  //define lisboa como ponto de partida e calcula a rota
  //Obtém tempo e distância da rota utilizando Google Directions API
  //Se a rota for encontrada, armazena os detalhes em routeSummary e exibe no template
  calculateRoute() {
    if (!this.selectedDestination) return;

    const origin = new google.maps.LatLng(38.7223, -9.1393); // Lisboa (simulação de ponto de partida)
    const destination = this.selectedDestination;

    this.directionsService.route({
      origin,
      destination,
      travelMode: google.maps.TravelMode.DRIVING
    }, (result, status) => {
      if (status === google.maps.DirectionsStatus.OK) {
        this.directionsRenderer.setDirections(result);

        const route = result.routes[0].legs[0];
        this.routeSummary = {
          distance: route.distance.text,
          duration: route.duration.text
        };
      } else {
        alert('Failed to get route. Try again.');
      }
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

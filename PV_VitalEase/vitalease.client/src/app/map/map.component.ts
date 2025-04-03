import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import * as L from 'leaflet';
import { CommonModule } from '@angular/common';
import { ChangeDetectorRef } from '@angular/core';
import { RouterModule } from '@angular/router';

// Standard marker icon (used for map click markers)
L.Marker.prototype.options.icon = L.icon({
    iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
    shadowUrl: ''
});

// A common red icon (used for search markers and user location)
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
    favoriteMarkers: L.Marker[] = [];
    // To store user's current location
    userLocation: L.LatLng | null = null;

    @ViewChild('map', { static: true }) mapElement!: ElementRef;
    @ViewChild('searchInput', { static: true }) searchInput!: ElementRef;

    map!: L.Map;
    markers: L.Marker[] = [];
    routeLayer: L.GeoJSON | null = null;
    routeSummary: { distance: string, duration: string } | null = null;
    selectedDestination: L.LatLng | null = null;

    // Google Places Autocomplete instance (for full address search)
    googleAutocomplete!: google.maps.places.Autocomplete;

    // Favorites array and visibility toggle
    favoriteLocations: Array<{ lat: number, lng: number, name: string }> = [];
    favoritesVisible: boolean = false;

    constructor(private authService: AuthService, private router: Router, private cdr: ChangeDetectorRef) { }

    ngOnInit() {
        this.initMap();
        this.checkUserSession();
        // Listen for custom "addFavorite" events from popups
        document.addEventListener('addFavorite', (event: any) => {
            const { lat, lng, name } = event.detail;
            this.addFavorite(L.latLng(lat, lng), name);
        });
        this.loadFavoriteLocations();

        const token = this.authService.getSessionToken();
        if (token) {
            this.authService.validateSessionToken().subscribe(
                (response: any) => {
                    this.isLoggedIn = true;
                    this.userInfo = response.user;
                },
                (error) => {
                    
                }
            );
        } else {
            // No token found, redirect to login
           
        }
        if (!this.authService.isAuthenticated()) {
           
            return;
        }

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

    // Initialize autocomplete (for full address search)
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
            // For autocomplete, we use a default details string
            const details = `<b>Selected Location</b>`;
            this.selectedDestination = leafletLatLng;
            this.addMarker(leafletLatLng, details);
            this.map.setView([lat, lng], 15);
            this.calculateRoute();
        });
    }

    // Basic search function – calls the filtered search function
    searchPlaces(): void {
        const query = this.searchInput.nativeElement.value;
        if (!query) {
            alert("You need to write something.");
            return;
        }
        this.searchPlacesFilter(query);
    }

    searchPlacesFilter(filter: string): void {
        const center = this.map.getCenter();
        const request = {
            location: new google.maps.LatLng(center.lat, center.lng),
            radius: 5000,
            type: filter.toLowerCase()
        };

        const service = new google.maps.places.PlacesService(document.createElement('div'));
        service.nearbySearch(request, (results, status) => {
            if (status === google.maps.places.PlacesServiceStatus.OK && results) {
                // Remove existing markers
                this.markers.forEach(marker => this.map.removeLayer(marker));
                this.markers = [];
                results.forEach(place => {
                    if (!place.geometry || !place.geometry.location) return;
                    const lat = place.geometry.location.lat();
                    const lng = place.geometry.location.lng();
                    const leafletLatLng = L.latLng(lat, lng);
                    const placeName = place.name ? place.name : 'No name';
                    const details = `<b>${placeName}</b><br>${place.vicinity || ''}<br>${place.rating ? 'Avaliação: ' + place.rating : ''}`;

                    // Build initial popup content with the Add to Favorites button
                    const popupContent = `
          ${details}
          <br><br>
          <button onclick="document.dispatchEvent(new CustomEvent('addFavorite', { detail: { lat: ${lat}, lng: ${lng}, name: '${placeName.replace(/'/g, "\\'")}' } }))">
            Add to Favorites
          </button>
          <br><br>
          <b>Route Summary</b><br>Loading route...
        `;

                    const marker = L.marker(leafletLatLng, { icon: redIcon }).addTo(this.map);
                    marker.bindPopup(popupContent).openPopup();

                    // When the marker is clicked, always include the Add to Favorites button in the popup.
                    marker.on('click', () => {
                        this.selectedDestination = leafletLatLng;
                        this.map.setView(leafletLatLng, 15);

                        // Rebuild initial popup content (with favorites button) when clicked
                        const initialPopup = `
            ${details}
            <br><br>
            <button onclick="document.dispatchEvent(new CustomEvent('addFavorite', { detail: { lat: ${lat}, lng: ${lng}, name: '${placeName.replace(/'/g, "\\'")}' } }))">
              Add to Favorites
            </button>
            <br><br>
            <b>Route Summary</b><br>Loading route...
          `;
                        marker.bindPopup(initialPopup).openPopup();

                        // Calculate route and then update the popup with unified content
                        this.calculateRoute().then(() => {
                            const updatedPopup = `
              ${details}
              <br><br>
              <button onclick="document.dispatchEvent(new CustomEvent('addFavorite', { detail: { lat: ${lat}, lng: ${lng}, name: '${placeName.replace(/'/g, "\\'")}' } }))">
                Add to Favorites
              </button>
              <br><br>
              <b>Route Summary</b><br>
              Distance: ${this.routeSummary?.distance ?? 'N/D'}<br>
              Time: ${this.routeSummary?.duration ?? 'N/D'}
            `;
                            marker.bindPopup(updatedPopup).openPopup();
                        });
                    });
                    this.markers.push(marker);
                });
                if (this.markers.length > 0) {
                    const group = new L.FeatureGroup(this.markers);
                    this.map.fitBounds(group.getBounds());
                }
            } else {
                alert("Nenhum resultado foi encontrado para " + filter);
            }
        });
    }

    // Called when the filter dropdown changes
    onFilterChange(event: Event): void {
        const filter = (event.target as HTMLSelectElement).value;
        if (!filter) {
            alert('Please select a filter.');
            return;
        }
        alert('Filter updated: ' + filter);
        this.searchPlacesFilter(filter);
    }

    // Toggle favorites list visibility
    toggleFavoritesList(): void {
        this.favoritesVisible = !this.favoritesVisible;
    }

    // When a favorite is selected from the list, center the map on that location
  showFavoriteOnMap(fav: { lat: number, lng: number, name: string }): void {
    const latlng = L.latLng(fav.lat, fav.lng);
    this.map.setView(latlng, 15);
    // Check if a marker for this favorite is already present
    let existingMarker = this.favoriteMarkers.find(m => {
      const pos = m.getLatLng();
      return pos.lat === fav.lat && pos.lng === fav.lng;
    });
    if (!existingMarker) {
      const marker = L.marker(latlng, { icon: redIcon }).addTo(this.map);
      marker.bindPopup(`<b>Favorite:</b> ${fav.name}`).openPopup();
      this.favoriteMarkers.push(marker);
      existingMarker = marker;
    } else {
      existingMarker.openPopup();
    }
    // Set the favorite as the selected destination and update its popup with route details
    this.selectedDestination = latlng;
    this.calculateRoute().then(() => {
      if (existingMarker) {
        existingMarker.bindPopup(
          `<b>Favorite:</b> ${fav.name}<br><br><b>Route Summary</b><br>` +
          `Distance: ${this.routeSummary?.distance ?? 'N/D'}<br>` +
          `Time: ${this.routeSummary?.duration ?? 'N/D'}`
        ).openPopup();
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

    // Initialize the map and attempt to get the user's location (with fallback)
    initMap() {
        const defaultLocation: [number, number] = [38.521877, -8.839083];
        this.map = L.map(this.mapElement.nativeElement).setView(defaultLocation, 15);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(this.map);

        const addUserMarker = (lat: number, lng: number) => {
            this.userLocation = L.latLng(lat, lng);
            const marker = L.marker([lat, lng], { icon: redIcon }).addTo(this.map);
            marker.bindPopup('You are here.').openPopup();
        };

        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                (position) => {
                    const userLat = position.coords.latitude;
                    const userLng = position.coords.longitude;
                    this.map.setView([userLat, userLng], 15);
                    addUserMarker(userLat, userLng);
                },
                (error) => {
                    console.error('Erro ao obter a localização do utilizador:', error);
                    addUserMarker(defaultLocation[0], defaultLocation[1]);
                },
                { enableHighAccuracy: true }
            );
        } else {
            console.error('Geolocalização não é suportada pelo navegador.');
            addUserMarker(defaultLocation[0], defaultLocation[1]);
        }

        // Map click event for manually adding a destination
        this.map.on('click', (event: L.LeafletMouseEvent) => {
            this.selectedDestination = event.latlng;
            // For manual clicks, use a default details string
            this.addMarker(event.latlng, `<b>Selected Location</b>`);
            this.calculateRoute();
        });
    }

    // Button function to go to the user's location
    goToUserLocation(): void {
        if (this.userLocation) {
            this.map.setView(this.userLocation, 15);
        } else {
            alert('User location not available.');
        }
    }

    // Modified addMarker: creates a marker at the given position with an optional details string.
    // The popup will include an "Add to Favorites" button.
    // Modified addMarker: always includes the "Add to Favorites" button in the popup
    addMarker(position: L.LatLng, details?: string) {
        // Remove existing markers if desired
        this.markers.forEach(marker => this.map.removeLayer(marker));
        this.markers = [];

        const marker = L.marker(position, { icon: redIcon }).addTo(this.map);
        // Use the first line of details (if provided) as the name, or default to "Selected Location"
        const name = details ? details.split('<br>')[0].replace(/<b>|<\/b>/g, '') : 'Selected Location';

        const content = `
    ${details ? details : '<b>Selected Location</b>'}
    <br><br>
    <button onclick="document.dispatchEvent(new CustomEvent('addFavorite', { detail: { lat: ${position.lat}, lng: ${position.lng}, name: '${name.replace(/'/g, "\\'")}' } }))">
      Add to Favorites
    </button>
    <br><br>
    <b>Route Summary</b><br>
    Distance: ${this.routeSummary?.distance ?? 'N/D'}<br>
    Time: ${this.routeSummary?.duration ?? 'N/D'}
  `;
        marker.bindPopup(content).openPopup();

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

    // Modify calculateRoute to return a Promise so we can update popups after route calculation
    calculateRoute(): Promise<void> {
        return new Promise((resolve, reject) => {
            if (!this.userLocation || !this.selectedDestination) {
                resolve();
                return;
            }
            const start = `${this.userLocation.lng},${this.userLocation.lat}`;
            const end = `${this.selectedDestination.lng},${this.selectedDestination.lat}`;
            const osrmUrl = `https://router.project-osrm.org/route/v1/driving/${start};${end}?overview=full&geometries=geojson&alternatives=true`;

            fetch(osrmUrl)
                .then(response => response.json())
                .then(data => {
                    if (data.routes && data.routes.length > 0) {
                        const bestRoute = data.routes.reduce((prev: any, curr: any) =>
                            (prev.distance < curr.distance ? prev : curr)
                        );
                        if (this.routeLayer) {
                            this.map.removeLayer(this.routeLayer);
                        }
                        this.routeLayer = L.geoJSON(bestRoute.geometry, {
                            style: { color: 'red', weight: 5 }
                        }).addTo(this.map);

                        this.routeSummary = {
                            distance: (bestRoute.distance / 1000).toFixed(2) + ' km',
                            duration: (bestRoute.duration / 60).toFixed(2) + ' min'
                        };

                        this.cdr.detectChanges();
                        resolve();
                    } else {
                        alert('Não foi possível calcular a rota.');
                        resolve();
                    }
                })
                .catch(error => {
                    console.error('Erro ao obter a rota:', error);
                    resolve();
                });
        });
    }

    // Favorite locations functionality

    addFavorite(location: L.LatLng, name: string): void {
        const fav = { lat: location.lat, lng: location.lng, name };
        this.favoriteLocations.push(fav);
        localStorage.setItem('favoriteLocations', JSON.stringify(this.favoriteLocations));
        alert('Location added to favorites!');
    }

    loadFavoriteLocations(): void {
        const favs = localStorage.getItem('favoriteLocations');
        if (favs) {
            this.favoriteLocations = JSON.parse(favs);
        }
    }

  deleteFavorite(fav: { lat: number, lng: number, name: string }): void {
    // Remove the favorite from the array and update local storage.
    this.favoriteLocations = this.favoriteLocations.filter(f =>
      !(f.lat === fav.lat && f.lng === fav.lng && f.name === fav.name)
    );
    localStorage.setItem('favoriteLocations', JSON.stringify(this.favoriteLocations));
    alert('Favorite location deleted.');

    // Find and remove the corresponding marker from the map.
    const markerIndex = this.favoriteMarkers.findIndex(m => {
      const pos = m.getLatLng();
      return pos.lat === fav.lat && pos.lng === fav.lng;
    });
    if (markerIndex > -1) {
      const marker = this.favoriteMarkers[markerIndex];
      this.map.removeLayer(marker);
      this.favoriteMarkers.splice(markerIndex, 1);
    }
  }


    logout() {
        this.authService.logout();
        this.isLoggedIn = false;
        this.router.navigate(['/']);
    }
}

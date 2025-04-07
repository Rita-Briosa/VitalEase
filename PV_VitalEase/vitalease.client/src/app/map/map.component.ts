import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import * as L from 'leaflet';
import { CommonModule } from '@angular/common';
import { ChangeDetectorRef } from '@angular/core';
import { RouterModule } from '@angular/router';

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

/**
 * @component MapComponent
 * @description
 * The MapComponent is responsible for displaying an interactive map using Leaflet, integrating with the Google Maps API for place search and autocomplete,
 * and providing routing functionality via the OSRM service. It also manages user authentication, session validation, and the handling of favorite locations.
 *
 * The component supports the following functionalities:
 * - Initialization of the Leaflet map with a default or user geolocation.
 * - Integration with the Google Maps API to provide an autocomplete search for places.
 * - Displaying markers on the map for searched places, user location, and favorites.
 * - Calculating routes from the user's location to a selected destination using OSRM.
 * - Managing a list of favorite locations with add, display, and delete operations.
 * - User session validation and redirection based on authentication status.
 *
 * @dependencies
 * - AuthService: Handles authentication operations, such as session token retrieval, validation, and logout.
 * - Router: Provides navigation between application routes.
 * - ChangeDetectorRef: Triggers change detection for dynamic updates, particularly after route calculations.
 * - Leaflet: The mapping library used for map rendering, marker management, and layer control.
 * - Google Maps API: Used for places autocomplete and nearby search.
 *
 * @usage
 * This component is declared as standalone and imports necessary modules (CommonModule, RouterModule) for template and routing functionalities.
 */
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
    isAdmin: boolean = false;
    favoriteMarkers: L.Marker[] = [];
    userLocation: L.LatLng | null = null;
    @ViewChild('map', { static: true }) mapElement!: ElementRef;
    @ViewChild('searchInput', { static: true }) searchInput!: ElementRef;
    map!: L.Map;
    markers: L.Marker[] = [];
    routeLayer: L.GeoJSON | null = null;
    routeSummary: { distance: string, duration: string } | null = null;
    selectedDestination: L.LatLng | null = null;
    googleAutocomplete!: google.maps.places.Autocomplete;
    favoriteLocations: Array<{ lat: number, lng: number, name: string }> = [];
    favoritesVisible: boolean = false;

    constructor(private authService: AuthService, private router: Router, private cdr: ChangeDetectorRef) { }

  /**
* @method ngOnInit
* @description
* Lifecycle hook that is called after data-bound properties are initialized.
* It initializes the map, validates the user session, loads favorite locations,
* and sets up an event listener for adding favorites from marker popups.
*/
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

                if (this.userInfo.type === 1) {
                  this.isAdmin = true;
                } else {
                  this.isAdmin = false;
                }

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

  /**
* @method goToDashboard
* @description
* Redirects the user back to the app's dashboard.
*/
  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  /**
 * @method ngAfterViewInit
 * @description
 * Lifecycle hook that is called after the component's view has been fully initialized.
 * It loads the Google Maps API and initializes the autocomplete feature for place search.
 */
  ngAfterViewInit() {
        this.loadGoogleMapsAPI()
            .then(() => {
                this.initializeGoogleAutocomplete();
            })
            .catch(err => console.error('Erro ao carregar a Google Maps API:', err));
    }

  /**
* @method loadGoogleMapsAPI
* @returns {Promise<void>}
* @description
* Dynamically loads the Google Maps JavaScript API if it is not already loaded.
* Resolves the promise when the API has been successfully loaded.
*/
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

  /**
 * @method initializeGoogleAutocomplete
 * @description
 * Initializes the Google Places Autocomplete service on the search input element.
 * Configures the autocomplete to restrict results to geocoding (address) type.
 * When a place is selected, a marker is added to the map and a route is calculated.
 */
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

  /**
* @method searchPlaces
* @description
* Retrieves the query from the search input and triggers a filtered search for places.
* Alerts the user if the search query is empty.
*/
  searchPlaces(): void {
        const query = this.searchInput.nativeElement.value;
        if (!query) {
            alert("You need to write something.");
            return;
        }
        this.searchPlacesFilter(query);
    }

  /**
  * @method searchPlacesFilter
  * @description
  * Searches for places near the center of the map that match the provided filter.
  * Clears existing markers and adds new markers based on the search results.
  * Each marker includes a popup with details, an "Add to Favorites" button, and route summary information.
  * @param filter - The search filter to apply (e.g., type of place).
  */
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

  /**
* @method onFilterChange
* @description
* Handles changes in the filter selection for place search.
* Triggers a new search based on the selected filter value.
* @param event - The filter change event.
*/
  onFilterChange(event: Event): void {
        const filter = (event.target as HTMLSelectElement).value;
        if (!filter) {
            alert('Please select a filter.');
            return;
        }
        alert('Filter updated: ' + filter);
        this.searchPlacesFilter(filter);
    }

  /**
* @method showFavoriteOnMap
* @description
* Centers the map on a favorite location, displays its marker and recalculates the route to it.
* @param fav - The favorite location containing latitude, longitude, and name.
*/
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
  /**
   * @method checkUserSession
   * @description
   * Validates the current user session by checking for a session token.
   * If a token exists, it validates the token and updates the login status and user information.
   * Otherwise, it triggers a logout and redirects to the login page.
   */
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

  /**
* @method initMap
* @description
* Initializes the Leaflet map with a default location and sets up the tile layer.
* It attempts to obtain the user's current geolocation to center the map.
* If geolocation is not available or fails, it uses the default location.
*/
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

  /**
    * @method goToUserLocation
    * @description
    * Centers the map on the user's current location.
    * Alerts the user if the location is not available.
    */
  goToUserLocation(): void {
        if (this.userLocation) {
            this.map.setView(this.userLocation, 15);
        } else {
            alert('User location not available.');
        }
  }

  /**
* @method addMarker
* @description
* Adds a marker to the map at the specified position with a popup displaying details.
* The popup includes an "Add to Favorites" button and route summary information.
* @param position - The geographic location for the marker.
* @param details - Optional HTML string containing details to display in the popup.
*/
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

  /**
* @method calculateRoute
* @returns {Promise<void>}
* @description
* Calculates the driving route from the user's current location to the selected destination using the OSRM API.
* If a valid route is found, it adds a GeoJSON layer to the map to display the route and updates the route summary.
* In case of an error or if no route is found, appropriate alerts or error messages are logged.
*/
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

  /**
* @method addFavorite
* @description
* Adds a location to the list of favorite locations and saves the updated list to local storage.
* @param location - The geographic coordinates of the favorite location.
* @param name - The name of the favorite location.
*/
  addFavorite(location: L.LatLng, name: string): void {
        const fav = { lat: location.lat, lng: location.lng, name };
        this.favoriteLocations.push(fav);
        localStorage.setItem('favoriteLocations', JSON.stringify(this.favoriteLocations));
        alert('Location added to favorites!');
    }

  /**
  * @method loadFavoriteLocations
  * @description
  * Loads the favorite locations from local storage and populates the favoriteLocations array.
  */
  loadFavoriteLocations(): void {
        const favs = localStorage.getItem('favoriteLocations');
        if (favs) {
            this.favoriteLocations = JSON.parse(favs);
        }
    }

  /**
   * @method deleteFavorite
   * @description
   * Deletes a specified favorite location from the list and updates local storage.
   * It also removes the corresponding marker from the map if it exists.
   * @param fav - The favorite location to delete.
   */
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

  /**
 * @method logout
 * @description
 * Logs out the current user by clearing the session and navigates to the homepage.
 */
  logout() {
        this.authService.logout();
        this.isLoggedIn = false;
        this.router.navigate(['/']);
    }
}

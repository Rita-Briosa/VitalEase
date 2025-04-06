import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import * as L from 'leaflet';
import { CommonModule } from '@angular/common';
import { ChangeDetectorRef } from '@angular/core';
import { RouterModule } from '@angular/router';

/**
 * Define o ícone padrão para os marcadores de cliques no mapa.
 *
 * Esta configuração substitui o ícone padrão dos objetos L.Marker, atribuindo-lhes um novo ícone criado com L.icon.
 *
 * As propriedades configuradas são:
 * - iconUrl: Especifica o URL da imagem a utilizar como ícone. Neste caso, utiliza a imagem oficial do Leaflet.
 * - iconSize: Define as dimensões do ícone (largura e altura, em píxeis).
 * - iconAnchor: Determina o ponto do ícone que será colocado na coordenada exata do marcador.
 * - popupAnchor: Define o deslocamento do pop-up relativamente ao ponto de ancoragem do ícone.
 * - shadowUrl: Indica o URL da imagem da sombra; aqui encontra-se vazio, o que significa que não é utilizada sombra.
 */
L.Marker.prototype.options.icon = L.icon({
    iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
    shadowUrl: ''
});

/**
 * Define um ícone vermelho personalizado para utilização em marcadores no mapa.
 *
 * Este ícone é criado através da função L.icon do Leaflet e configura as seguintes propriedades:
 * - iconUrl: Especifica o URL da imagem a utilizar para o ícone; neste caso, é uma imagem de um ponto vermelho proveniente do Google Maps.
 * - iconSize: Define as dimensões do ícone (largura e altura, em píxeis).
 * - iconAnchor: Determina o ponto de ancoragem do ícone, ou seja, a parte da imagem que será alinhada à localização do marcador.
 * - popupAnchor: Define a posição relativa ao iconAnchor onde o popup será exibido.
 */
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
* Método executado durante a inicialização do componente (lifecycle hook ngOnInit).
*
* Este método realiza as seguintes operações:
* 
* 1. Inicializa o mapa chamando o método initMap().
* 2. Verifica a sessão do utilizador através do método checkUserSession().
* 3. Adiciona um ouvinte de eventos para o evento personalizado "addFavorite" emitido pelos popups:
*    - Quando o evento é detetado, extrai as coordenadas (lat e lng) e o nome do local a partir de event.detail.
*    - Invoca o método addFavorite() passando um objeto LatLng (criado com L.latLng(lat, lng)) e o nome do local.
* 4. Carrega as localizações favoritas utilizando o método loadFavoriteLocations().
* 5. Obtém o token de sessão através de authService.getSessionToken():
*    - Se existir um token, valida-o com validateSessionToken(). Ao obter uma resposta:
*      - Define isLoggedIn como verdadeiro.
*      - Atribui os dados do utilizador à propriedade userInfo.
*      - Determina se o utilizador é administrador (userInfo.type === 1); caso afirmativo, define isAdmin como verdadeiro, caso contrário, falso.
*    - Em caso de erro durante a validação do token, a secção de tratamento de erros é acionada (implementação pendente).
* 6. Se não for encontrado nenhum token, o comentário indica que o redirecionamento para a página de login deverá ser implementado.
* 7. Verifica se o utilizador está autenticado através de authService.isAuthenticated(); se não estiver, o método termina a execução.
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
 * Redireciona o utilizador para o dashboard.
 *
 * Este método utiliza o router para navegar até à rota '/dashboard',
 * permitindo o acesso à interface principal ou à área de gestão da aplicação.
 */
  goToDashboard() {
    this.router.navigate(['/dashboard']);
  }

  /**
 * Lifecycle hook executado após a visualização ter sido inicializada (ngAfterViewInit).
 *
 * Este método é responsável por:
 * - Carregar a API do Google Maps através do método loadGoogleMapsAPI().
 * - Após a carga bem-sucedida, invocar o método initializeGoogleAutocomplete() para
 *   configurar a funcionalidade de autocompletar.
 * - Caso ocorra algum erro durante o carregamento da API, este é registado no console.
 */
  ngAfterViewInit() {
        this.loadGoogleMapsAPI()
            .then(() => {
                this.initializeGoogleAutocomplete();
            })
            .catch(err => console.error('Erro ao carregar a Google Maps API:', err));
    }

  /**
   * Carrega a API do Google Maps de forma assíncrona e retorna uma Promise.
   *
   * Se a API do Google Maps já estiver disponível (verificando se window.google e window.google.maps estão definidos),
   * a Promise é imediatamente resolvida.
   *
   * Caso contrário, o método cria dinamicamente um elemento <script> para carregar a API com a chave e a biblioteca "places".
   * As propriedades async e defer são definidas para que o carregamento do script não bloqueie o parsing da página.
   *
   * Quando o script é carregado com sucesso, a Promise é resolvida; se ocorrer um erro durante o carregamento,
   * a Promise é rejeitada com o erro correspondente.
   *
   * @returns {Promise<void>} Uma Promise que se resolve quando a API do Google Maps é carregada com sucesso ou já está disponível.
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
 * Inicializa a funcionalidade de autocompletar da Google para o campo de pesquisa.
 *
 * Este método realiza as seguintes operações:
 * - Verifica se o elemento de pesquisa (searchInput) está definido; caso contrário, regista um erro no console e termina a execução.
 * - Instancia um objeto google.maps.places.Autocomplete, utilizando o elemento nativo de searchInput e restringindo os resultados a endereços (types: ['geocode']).
 * - Adiciona um ouvinte para o evento 'place_changed', que:
 *   - Obtém o local selecionado através de googleAutocomplete.getPlace().
 *   - Verifica se o local possui dados de geometria e localização; se não, exibe um alerta informando que nenhum resultado foi encontrado.
 *   - Extrai a latitude e longitude do local, convertendo-os para um objeto Leaflet LatLng.
 *   - Define uma string de detalhes padrão para o marcador.
 *   - Atualiza a propriedade selectedDestination com a localização obtida.
 *   - Adiciona um marcador no mapa com a localização e os detalhes através do método addMarker().
 *   - Reposiciona a vista do mapa para a nova localização com um nível de zoom de 15.
 *   - Invoca o método calculateRoute() para calcular a rota a partir do local seleccionado.
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
  * Executa a pesquisa de locais com base no valor inserido no campo de pesquisa.
  *
  * Este método realiza as seguintes operações:
  * 1. Obtém o valor do campo de pesquisa através de this.searchInput.nativeElement.value.
  * 2. Verifica se o termo de pesquisa (query) está vazio. Se estiver:
  *    - Exibe um alerta informando o utilizador de que é necessário escrever algo.
  *    - Termina a execução do método.
  * 3. Caso o termo de pesquisa não esteja vazio, invoca o método searchPlacesFilter()
  *    passando o termo de pesquisa, para proceder à filtragem dos locais.
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
   * Realiza uma pesquisa de locais próximos com base num filtro especificado e atualiza o mapa com os resultados.
   *
   * Este método executa as seguintes operações:
   * 1. Obtém o centro atual do mapa.
   * 2. Cria um objeto de pedido (request) para a API do Google Places, utilizando:
   *    - A localização central do mapa.
   *    - Um raio de 5000 metros.
   *    - O tipo de local, derivado do filtro (convertido para minúsculas).
   * 3. Inicializa um serviço de Places (google.maps.places.PlacesService) associado a um elemento <div> fictício.
   * 4. Executa a pesquisa de locais próximos (nearbySearch) com o pedido criado.
   * 5. Se a pesquisa for bem-sucedida (status OK):
   *    - Remove todos os marcadores existentes do mapa.
   *    - Para cada local retornado:
   *      - Verifica se o local possui dados de geometria e localização; caso contrário, ignora o local.
   *      - Extrai a latitude e longitude da localização do local.
   *      - Cria um objeto LatLng do Leaflet.
   *      - Define o nome do local ou utiliza 'No name' se este não estiver disponível.
   *      - Cria uma string com detalhes do local (nome, proximidade e avaliação, se disponível).
   *      - Constrói o conteúdo inicial do popup, incluindo um botão "Add to Favorites" que emite um evento personalizado.
   *      - Adiciona um marcador ao mapa utilizando um ícone vermelho (redIcon) e associa o popup inicial.
   *      - Regista um ouvinte de clique no marcador que:
   *          a) Actualiza a localização seleccionada e centra o mapa.
   *          b) Reconstroi o popup inicial.
   *          c) Invoca a função calculateRoute() para calcular a rota e, após a sua conclusão,
   *             actualiza o popup com um sumário da rota (distância e tempo).
   *      - Adiciona o marcador à lista de marcadores.
   *    - Se existirem marcadores, ajusta os limites do mapa para englobá-los.
   * 6. Se nenhum resultado for encontrado ou se a pesquisa falhar, exibe um alerta informativo.
   *
   * @param {string} filter - O filtro a utilizar na pesquisa dos tipos de locais.
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
   * Trata a alteração do filtro de pesquisa.
   *
   * Este método é invocado quando o utilizador altera a seleção num elemento HTML (por exemplo, num menu dropdown).
   * As operações efectuadas são as seguintes:
   * 1. Obtém o valor do filtro a partir do elemento que disparou o evento.
   * 2. Verifica se o filtro está vazio; se estiver, exibe um alerta a pedir que seleccione um filtro e termina a execução.
   * 3. Se o filtro for válido, exibe um alerta a informar que o filtro foi actualizado.
   * 4. Invoca o método searchPlacesFilter passando o filtro seleccionado, de forma a proceder à pesquisa dos locais.
   *
   * @param event - O evento que contém a nova seleção do filtro.
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
  * Alterna a visibilidade da lista de favoritos.
  *
  * Este método inverte o valor da propriedade `favoritesVisible`, determinando se a lista de locais favoritos
  * será exibida ou oculta.
  */
  toggleFavoritesList(): void {
        this.favoritesVisible = !this.favoritesVisible;
    }

  /**
  * Exibe o local favorito no mapa.
  *
  * Este método realiza as seguintes operações:
  * 1. Converte as coordenadas do objeto favorito num objeto LatLng do Leaflet.
  * 2. Centra o mapa na localização do favorito com um nível de zoom de 15.
  * 3. Verifica se já existe um marcador para este favorito na coleção de marcadores favoritos:
  *    - Se existir, abre o popup associado.
  *    - Caso contrário, cria um novo marcador com o ícone vermelho, adiciona-o ao mapa,
  *      associa um popup com o nome do favorito e regista-o na lista de marcadores.
  * 4. Define o favorito como a destinação seleccionada para o cálculo da rota.
  * 5. Invoca o método calculateRoute() e, quando a rota estiver calculada, atualiza o popup
  *    do marcador com o sumário da rota, incluindo a distância e o tempo estimado.
  *
  * @param fav Objeto contendo as propriedades lat, lng e name do local favorito.
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
   * Verifica a sessão do utilizador.
   *
   * Este método obtém o token de sessão através do serviço de autenticação. Caso um token seja encontrado, procede à sua validação.
   * Se a validação for bem-sucedida, o estado do utilizador é atualizado para "autenticado" e as informações do utilizador são armazenadas.
   * Em caso de falha na validação, o método efetua o logout do utilizador e redireciona-o para a página de login.
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
  * Inicializa o mapa e configura a visualização e interacção com o mesmo.
  *
  * Este método executa as seguintes operações:
  * 1. Define uma localização predefinida (defaultLocation) que será utilizada caso não seja possível obter a localização actual do utilizador.
  * 2. Cria um mapa Leaflet utilizando o elemento DOM referenciado por this.mapElement e define a vista inicial com defaultLocation e um nível de zoom de 15.
  * 3. Adiciona uma camada de tiles do OpenStreetMap com os créditos adequados.
  * 4. Define a função addUserMarker, que:
  *    - Actualiza a propriedade userLocation com as coordenadas fornecidas.
  *    - Adiciona um marcador ao mapa na posição indicada com o ícone vermelho (redIcon).
  *    - Liga um popup ao marcador com a mensagem "You are here.".
  * 5. Se o navegador suportar geolocalização:
  *    - Tenta obter a localização actual do utilizador.
  *    - Em caso de sucesso, actualiza a vista do mapa para a localização do utilizador e adiciona um marcador.
  *    - Em caso de erro, regista o erro no console e utiliza a localização predefinida para adicionar o marcador.
  * 6. Se a geolocalização não for suportada pelo navegador, regista uma mensagem de erro e utiliza a localização predefinida.
  * 7. Regista um ouvinte de eventos no mapa para detetar cliques:
  *    - Quando o mapa é clicado, actualiza a propriedade selectedDestination com as coordenadas do clique.
  *    - Adiciona um marcador na posição seleccionada com um popup a exibir "Selected Location".
  *    - Invoca o método calculateRoute para calcular uma rota com base na destinação seleccionada.
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
  * Focaliza o mapa na localização do utilizador.
  *
  * Este método verifica se a localização do utilizador (armazenada em `this.userLocation`) está disponível:
  * - Se estiver, o mapa centra-se nessa localização com um nível de zoom de 15.
  * - Caso contrário, exibe um alerta a informar que a localização do utilizador não está disponível.
  */
  goToUserLocation(): void {
        if (this.userLocation) {
            this.map.setView(this.userLocation, 15);
        } else {
            alert('User location not available.');
        }
    }

  /**
   * Adiciona um marcador ao mapa e configura o seu popup com detalhes e ações associadas.
   *
   * Este método executa as seguintes operações:
   * 1. Remove todos os marcadores existentes do mapa e limpa a lista de marcadores.
   * 2. Cria um novo marcador na posição especificada, utilizando o ícone vermelho (redIcon), e adiciona-o ao mapa.
   * 3. Determina o nome do marcador a partir da primeira linha do parâmetro 'details', removendo as tags <b> e </b>,
   *    ou utiliza "Selected Location" caso os detalhes não sejam fornecidos.
   * 4. Constrói o conteúdo do popup, que inclui:
   *    - Os detalhes fornecidos ou uma mensagem por omissão.
   *    - Um botão "Add to Favorites" que, quando clicado, despacha um evento personalizado 'addFavorite'
   *      com a latitude, longitude e nome do marcador.
   *    - Um sumário da rota que exibe a distância e o tempo (usando valores de this.routeSummary ou "N/D" se não disponíveis).
   * 5. Liga o popup ao marcador e abre-o imediatamente.
   * 6. Regista um ouvinte para o evento 'popupclose', que remove o marcador do mapa e da lista, e,
   *    se existir, remove também a camada da rota (this.routeLayer).
   * 7. Adiciona o marcador recém-criado à lista de marcadores.
   *
   * @param position - A posição (objeto L.LatLng) onde o marcador será adicionado.
   * @param details - Opcional. Uma string com os detalhes a exibir no popup do marcador.
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
   * Calcula a rota entre a localização do utilizador e a destinação seleccionada.
   *
   * Este método utiliza a API OSRM para obter uma rota entre dois pontos:
   * - A localização do utilizador (this.userLocation).
   * - A destinação seleccionada (this.selectedDestination).
   *
   * Funcionalidade:
   * 1. Se a localização do utilizador ou a destinação seleccionada não estiverem definidas, a Promise é resolvida imediatamente.
   * 2. Constrói as coordenadas de início e fim no formato "lng,lat".
   * 3. Cria a URL para a chamada à API OSRM com o perfil "driving", pedindo uma visão completa (overview=full)
   *    com geometrias no formato GeoJSON e alternativas possíveis.
   * 4. Efetua uma chamada fetch à URL criada:
   *    - Se a resposta contiver rotas válidas, seleciona a rota com a menor distância.
   *    - Remove, se existir, a camada de rota anterior (this.routeLayer) do mapa.
   *    - Adiciona uma nova camada GeoJSON ao mapa com a geometria da melhor rota, estilizada em vermelho (weight 5).
   *    - Atualiza this.routeSummary com a distância (em km) e a duração (em minutos) formatadas com duas casas decimais.
   *    - Invoca detectChanges() para actualizar a vista.
   * 5. Se não forem encontradas rotas ou ocorrer algum erro na chamada fetch, exibe uma mensagem de alerta
   *    ou regista o erro e, em ambos os casos, resolve a Promise.
   *
   * @returns {Promise<void>} Uma Promise que se resolve após o cálculo (ou tentativa) da rota.
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
  * Adiciona uma localização aos favoritos.
  *
  * Este método realiza as seguintes operações:
  * 1. Cria um objeto contendo a latitude, longitude e o nome da localização.
  * 2. Acrescenta a localização à lista de locais favoritos (favoriteLocations).
  * 3. Actualiza o localStorage, armazenando a lista de favoritos em formato JSON.
  * 4. Exibe um alerta a confirmar que a localização foi adicionada aos favoritos.
  *
  * @param location - A localização (L.LatLng) a adicionar.
  * @param name - O nome associado à localização.
  */
  addFavorite(location: L.LatLng, name: string): void {
        const fav = { lat: location.lat, lng: location.lng, name };
        this.favoriteLocations.push(fav);
        localStorage.setItem('favoriteLocations', JSON.stringify(this.favoriteLocations));
        alert('Location added to favorites!');
    }

  /**
  * Carrega as localizações favoritas a partir do localStorage.
  *
  * Este método verifica se existe um item guardado com a chave 'favoriteLocations'
  * no localStorage. Se existir, converte o valor JSON armazenado num array de objetos
  * e atribui-o à propriedade favoriteLocations.
  */
  loadFavoriteLocations(): void {
        const favs = localStorage.getItem('favoriteLocations');
        if (favs) {
            this.favoriteLocations = JSON.parse(favs);
        }
    }

  /**
   * Remove uma localização dos favoritos, atualiza o localStorage e remove o marcador correspondente do mapa.
   *
   * Este método executa as seguintes operações:
   * 1. Filtra a lista de favoritos, removendo o favorito que corresponda à latitude, longitude e nome fornecidos.
   * 2. Actualiza o localStorage com a lista de favoritos actualizada, convertida para JSON.
   * 3. Exibe um alerta para confirmar que o favorito foi eliminado.
   * 4. Procura na lista de marcadores do mapa o marcador que corresponda à localização do favorito:
   *    - Se encontrar, remove esse marcador do mapa e da lista de marcadores.
   *
   * @param fav - Objeto que representa o favorito, contendo as propriedades lat, lng e name.
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
   * Efetua o logout do utilizador e redirecciona para a página inicial.
   *
   * Este método realiza as seguintes operações:
   * 1. Invoca o método logout() do serviço de autenticação para invalidar a sessão do utilizador.
   * 2. Atualiza o estado interno, definindo isLoggedIn como false.
   * 3. Redirecciona o utilizador para a rota base ('/').
   */
  logout() {
        this.authService.logout();
        this.isLoggedIn = false;
        this.router.navigate(['/']);
    }
}

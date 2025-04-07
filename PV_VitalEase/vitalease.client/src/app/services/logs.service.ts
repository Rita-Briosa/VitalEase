import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * @service LogsService
 * @description
 * The LogsService is responsible for fetching logs from the backend API.
 * It provides methods to retrieve all logs and to filter logs based on provided criteria.
 *
 * @dependencies
 * - HttpClient: Used to perform HTTP GET requests to the backend API.
 *
 * @usage
 * This service is provided in the root injector and can be injected into components that require access to application logs.
 */
@Injectable({
  providedIn: 'root'
})
export class LogsService {

  // O endereço da sua API que retorna os logs
  private apiUrl = 'https://vitaleaseserver20250401155631-frabebccg8ckhmcj.spaincentral-01.azurewebsites.net/api'; // Endereço do seu backend ASP.NET Core

  constructor(private http: HttpClient) { }

  /**
   * @method getLogs
   * @description
   * Retrieves all logs by sending a GET request to the '/getLogs' endpoint.
   *
   * @returns An Observable that emits the list of logs.
   */
  getLogs(): Observable<any> {
    return this.http.get(`${this.apiUrl}/getLogs`); // Envia uma requisição GET para pegar os logs
  }

  /**
   * @method getLogsFilter
   * @description
   * Retrieves logs that match the provided filter criteria.
   * The filters are appended as query parameters to the GET request.
   *
   * @param filters - An object containing filter criteria (e.g. date range, user ID, action type, etc.).
   * @returns An Observable that emits the filtered list of logs.
   */
  getLogsFilter(filters: any): Observable<any> {
    let params = new HttpParams();

    Object.keys(filters).forEach(key => {
      if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
        params = params.set(key, filters[key])
      }
    });

    return this.http.get(`${this.apiUrl}/getLogsFilter`, {params});
  }
}

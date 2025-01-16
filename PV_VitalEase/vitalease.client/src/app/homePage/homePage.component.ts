import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-homePage',
  templateUrl: './homePage.component.html',
  standalone: false,
  styleUrl: './homePage.component.css'
})
export class homePageComponent implements OnInit {

  constructor(private http: HttpClient) { }

  ngOnInit() {
  }

  title = 'vitalease.client';
}

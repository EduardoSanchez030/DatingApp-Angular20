import { HttpContext, HttpClient } from '@angular/common/http';
import { Component, OnInit, Signal, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit{
  private http = inject(HttpClient);
  protected readonly title = 'Dating app';
  protected members = signal<any>([]);
  
  async ngOnInit() {
     this.members.set(await this.getMembers());
  }

  async getMembers() {
    try{
       return await lastValueFrom(this.http.get('https://localhost:5001/api/members'));
    }   
    catch (error) {
      console.error('Error fetching members', error);
      throw error;
    }
  }
}

import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './components/header/header.component'; 
import { SidebarComponent } from './components/sidebar/sidebar.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent, SidebarComponent],
  template: `
    <div class="layout-wrapper" [class.mobile-open]="isMobileOpen()">
      
      <aside class="sidebar-area">
        <app-sidebar (close)="isMobileOpen.set(false)" />
      </aside>

      <div class="main-area">
        
        <header class="top-bar">
          <button class="hamburger" (click)="toggleMenu()">☰</button>
          <app-header /> </header>

        <main class="content">
          <router-outlet></router-outlet>
        </main>
        
        @if (isMobileOpen()) {
          <div class="overlay" (click)="isMobileOpen.set(false)"></div>
        }
      </div>

    </div>
  `,
  styleUrls: ['./main-layout.component.scss']
})

export class MainLayoutComponent {
  isMobileOpen = signal(false);

  toggleMenu() {
    this.isMobileOpen.update(v => !v);
  }
}
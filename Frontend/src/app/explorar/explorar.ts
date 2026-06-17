import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { TcgService } from '../services/tcg.service';
import { CompNavBarComponent } from '../comp-nav-bar/comp-nav-bar';

@Component({
  selector: 'app-explorar',
  standalone: true,
  imports: [CommonModule, CompNavBarComponent],
  templateUrl: './explorar.html',
  styleUrls: ['./explorar.css']
})
export class ExplorarComponent implements OnInit {
  private authService = inject(AuthService);
  private tcgService = inject(TcgService);
  private router = inject(Router);

  public cartasTcg = signal<any[]>([]);

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      if (!user) {
        this.router.navigate(['/login']);
      }
    });

    this.tcgService.getCartas().subscribe({
      next: (data) => this.cartasTcg.set(data),
      error: (err) => console.error('Error cargando TCGdex', err)
    });
  }
}

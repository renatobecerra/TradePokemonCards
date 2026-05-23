import { Component, inject } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-terms',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './terms.html',
  styleUrls: ['./terms.css']
})
export class TermsComponent {
  private location = inject(Location);

  ngOnInit() {
    window.scrollTo(0, 0);
  }

  goBack() {
    this.location.back();
  }
}

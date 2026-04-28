// home.component.ts
import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

interface PokemonCard {
  name: string;
  hp: string;
  attack: string;
  damage: string;
  rarity: string;
  setId: string;
  image: string;
  color: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, OnDestroy {
  public cards = signal<PokemonCard[]>([
    {
      name: 'Mew VMAX',
      hp: '310 HP',
      attack: 'Max Miracle',
      damage: '130',
      rarity: 'RRR',
      setId: '#114/264',
      image: 'mew.png',
      color: '#f8a5c2'
    },
    {
      name: 'Pikachu V',
      hp: '200 HP',
      attack: 'Pika Ball',
      damage: '80',
      rarity: 'RR',
      setId: '#043/185',
      image: 'pikachu.png',
      color: '#f1c40f'
    },
    {
      name: 'Charizard GX',
      hp: '250 HP',
      attack: 'Flare Blitz',
      damage: '300',
      rarity: 'SSR',
      setId: '#150/147',
      image: 'charizard.png',
      color: '#e67e22'
    },
    {
      name: 'Lugia VSTAR',
      hp: '280 HP',
      attack: 'Tempest Dive',
      damage: '220',
      rarity: 'RRR',
      setId: '#139/195',
      image: 'lugia.png',
      color: '#95afc0'
    },
    {
      name: 'Rayquaza V',
      hp: '210 HP',
      attack: 'Dragon Pulse',
      damage: '40',
      rarity: 'RR',
      setId: '#123/203',
      image: 'rayquaza.png',
      color: '#2ecc71'
    }
  ]);

  private timer: any;

  ngOnInit() {
    // Rotación ultra lenta: cada 10 segundos
    this.timer = setInterval(() => {
      this.rotateCards();
    }, 10000);
  }

  rotateCards() {
    const currentCards = [...this.cards()];
    const firstCard = currentCards.shift();
    if (firstCard) {
      currentCards.push(firstCard);
      this.cards.set(currentCards);
    }
  }

  ngOnDestroy() {
    if (this.timer) {
      clearInterval(this.timer);
    }
  }
}

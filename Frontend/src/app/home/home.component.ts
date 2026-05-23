// home.component.ts
import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { Subscription } from 'rxjs';

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

interface TopRegistro {
  name: string;
  image: string;
  owner: string;
  rarity: string;
  value: string;
  color: string;
}

interface FAQItem {
  id: number;
  question: string;
  answer: string;
  isOpen: boolean;
}

interface ReviewItem {
  id: number;
  author: string;
  title: string;
  content: string;
  rating: number;
  date: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, OnDestroy {
  public menuOpen = signal<boolean>(false);
  public scrollProgress = signal<number>(0);
  public currentUser = signal<any>(null);

  private authService = inject(AuthService);
  private router = inject(Router);
  private authSub?: Subscription;

  public resenias = signal<ReviewItem[]>([
    {
      id: 1,
      author: 'Tony S.',
      title: 'Excelente comunidad',
      content: 'He logrado completar mi mazo de juego gracias a los intercambios que realicé aquí. La plataforma es intuitiva y la comunidad es muy seria. Muy recomendada.',
      rating: 5,
      date: '15 Mar, 2024'
    },
    {
      id: 2,
      author: 'Valentina P.',
      title: 'Intercambios seguros',
      content: 'Lo que más me gusta es que no hay comisiones ocultas. Puedes hablar directamente con otros coleccionistas y acordar los términos. He hecho más de 10 intercambios exitosos.',
      rating: 4.5,
      date: '22 Abr, 2024'
    },
    {
      id: 3,
      author: 'Dylan P.',
      title: 'La mejor web de trading',
      content: 'He probado varios sitios pero este es por lejos el más fácil de usar. El sistema de reputación te da mucha tranquilidad al momento de enviar tus cartas más valiosas.',
      rating: 4,
      date: '10 May, 2024'
    }
  ]);
  public faqItems = signal<FAQItem[]>([
    {
      id: 1,
      question: '¿Qué pasa si la carta que recibo no es la que vi?',
      answer: 'Como este es un espacio de intercambio directo entre coleccionistas, te recomendamos siempre solicitar fotos actuales o verificar los detalles del estado de la carta antes de cerrar el trato. La comunicación previa es tu mejor herramienta para asegurar que el artículo cumpla con tus expectativas.',
      isOpen: false
    },
    {
      id: 2,
      question: '¿La web me cobra comisión por cada venta?',
      answer: 'No, la plataforma no aplica comisiones. Nuestro objetivo es facilitar el contacto directo entre coleccionistas para que realicen sus transacciones y cambios sin intermediarios, permitiéndote gestionar tus ventas con total libertad.',
      isOpen: false
    },
    {
      id: 3,
      question: '¿Cómo sé si alguien es un coleccionista de confianza?',
      answer: 'Más allá de la verificación de correo electrónico, nuestra mayor seguridad reside en la reputación construida por la comunidad. Te recomendamos revisar el historial de reseñas en el perfil de cada usuario y utilizar el chat para establecer un contacto previo. Si detectas un comportamiento sospechoso, contamos con un sistema de reportes y moderación para sancionar a los usuarios que no cumplan con las normas, protegiendo así la integridad de nuestra comunidad.',
      isOpen: false
    },
    {
      id: 4,
      question: '¿Qué hago si me intentan estafar?',
      answer: 'Si experimentas un comportamiento irregular o falta de transparencia, repórtalo inmediatamente desde el perfil del usuario o dentro del chat. Contamos con un sistema de moderación activo para sancionar y excluir de la plataforma a cualquier usuario que actúe de mala fe, garantizando un entorno seguro y profesional para todos los coleccionistas.',
      isOpen: false
    }
  ]);

  public isUSD = signal<boolean>(false);
  private exchangeRate = signal<number>(898.23);

  public topRegistros = signal<TopRegistro[]>([
    {
      name: 'Lucario V',
      image: 'Cartas Top Registros/LucarioV.png',
      owner: 'Lexie H.',
      rarity: 'Normal',
      value: '$45.00',
      color: '#2ecc71'
    },
    {
      name: 'Mewtwo V',
      image: 'Cartas Top Registros/MewtwoV.png',
      owner: 'Nate J.',
      rarity: 'holo',
      value: '$120.00',
      color: '#2ecc71'
    },
    {
      name: 'Mew V',
      image: 'Cartas Top Registros/MewV.png',
      owner: 'Cassie H.',
      rarity: 'Normal',
      value: '$65.00',
      color: '#2ecc71'
    },
    {
      name: 'Moltres V',
      image: 'Cartas Top Registros/MoltresV.png',
      owner: 'Maddie P.',
      rarity: 'Holo',
      value: '$85.00',
      color: '#2ecc71'
    },
    {
      name: 'Ninetales V',
      image: 'Cartas Top Registros/NinetalesV.png',
      owner: 'Steve R.',
      rarity: 'Holo',
      value: '$55.00',
      color: '#2ecc71'
    },
    {
      name: 'Victini V',
      image: 'Cartas Top Registros/VictiniV.png',
      owner: 'Bruce B.',
      rarity: 'Normal',
      value: '$40.00',
      color: '#2ecc71'
    }
  ]);

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
    // Cerrar sesión si el usuario vuelve a la landing page, 
    // ya que esto se considera salir de la cuenta.
    if (this.authService.currentUserValue) {
      this.authService.logout();
    }

    // Suscribirse al estado del usuario
    this.authSub = this.authService.currentUser$.subscribe(user => {
      this.currentUser.set(user);
    });

    // Rotación ultra lenta: cada 10 segundos
    this.timer = setInterval(() => {
      this.rotateCards();
    }, 10000);

    // Fetch real-time exchange rate
    fetch('https://mindicador.cl/api/dolar')
      .then(response => response.json())
      .then(data => {
        if (data && data.dolar && data.dolar.valor) {
          this.exchangeRate.set(data.dolar.valor);
        }
      })
      .catch(err => console.error('Error fetching exchange rate:', err));
  }

  rotateCards() {
    const currentCards = [...this.cards()];
    const firstCard = currentCards.shift();
    if (firstCard) {
      currentCards.push(firstCard);
      this.cards.set(currentCards);
    }
  }

  toggleMenu() {
    this.menuOpen.set(!this.menuOpen());
  }

  updateScrollProgress(event: Event) {
    const container = event.target as HTMLElement;
    const maxScroll = container.scrollWidth - container.clientWidth;
    if (maxScroll <= 0) return;
    const progress = (container.scrollLeft / maxScroll) * 100;
    this.scrollProgress.set(progress);
  }

  scrollCards(direction: 'left' | 'right') {
    const container = document.querySelector('.top-cards-grid');
    if (container) {
      const scrollAmount = 400;
      container.scrollBy({
        left: direction === 'left' ? -scrollAmount : scrollAmount,
        behavior: 'smooth'
      });
    }
  }

  toggleFAQ(id: number) {
    this.faqItems.update(items => 
      items.map(item => ({
        ...item,
        isOpen: item.id === id ? !item.isOpen : false
      }))
    );
  }

  toggleCurrency() {
    this.isUSD.set(!this.isUSD());
  }

  getConvertedValue(usdValue: string): string {
    const numericValue = parseFloat(usdValue.replace('$', ''));
    if (this.isUSD()) {
      return `${usdValue} USD`;
    } else {
      const clpValue = Math.round(numericValue * this.exchangeRate());
      return `$${clpValue.toLocaleString('es-CL')} CLP`;
    }
  }

  scrollresenias(direction: 'left' | 'right') {
    const container = document.querySelector('.resenias-cards-scroll');
    if (container) {
      const scrollAmount = 450;
      container.scrollBy({
        left: direction === 'left' ? -scrollAmount : scrollAmount,
        behavior: 'smooth'
      });
    }
  }

  onLogout() {
    this.authService.logout();
    this.router.navigate(['/']);
  }

  ngOnDestroy() {
    if (this.authSub) {
      this.authSub.unsubscribe();
    }
    if (this.timer) {
      clearInterval(this.timer);
    }
  }
}

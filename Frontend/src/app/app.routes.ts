import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register';
import { VerifyComponent } from './verify/verify.component';
import { RecoveryComponent } from './recovery/recovery';
import { ExplorarComponent } from './explorar/explorar';
import { InventarioComponent } from './inventario/inventario';
import { PerfilComponent } from './perfil/perfil';
import { TermsComponent } from './terms/terms';
import { authGuard } from './auth.guard';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'verify', component: VerifyComponent },
  { path: 'recovery', component: RecoveryComponent },
  { path: 'explorar', component: ExplorarComponent, canActivate: [authGuard] },
  { path: 'inventario', component: InventarioComponent, canActivate: [authGuard] },
  { path: 'perfil', component: PerfilComponent, canActivate: [authGuard] },
  { path: 'terminos', component: TermsComponent }
];

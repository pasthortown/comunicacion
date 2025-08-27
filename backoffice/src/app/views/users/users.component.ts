import { Component, OnInit } from '@angular/core';
import { IconDirective } from '@coreui/icons-angular';
import {
  ButtonDirective,
  ColComponent,
  ContainerComponent,
  FormControlDirective,
  InputGroupComponent,
  InputGroupTextDirective,
  RowComponent
} from '@coreui/angular';
import Swal from 'sweetalert2';
import { UsersListComponent } from './users-list/users-list.component';
import { UserDialogComponent } from './user-dialog/user-dialog.component';
import { BackofficeService, BackofficeUser } from '../../services/backoffice.service';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss'],
  imports: [ContainerComponent, RowComponent, ColComponent, InputGroupComponent, InputGroupTextDirective, IconDirective, FormControlDirective, ButtonDirective, UserDialogComponent, UsersListComponent]
})

export class UsersComponent implements OnInit {
  users: BackofficeUser[] = [];
  loading = false;

  constructor(private backoffice: BackofficeService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.backoffice.getAllUsers$().subscribe({
      next: (res) => {
        this.users = res.response ?? [];
        this.loading = false;
        console.log('[UsersComponent] loaded users:', this.users);
      },
      error: (err) => {
        this.loading = false;
        // Muestra el error de forma amigable
        Swal.fire({
          icon: 'error',
          title: 'Error al cargar usuarios',
          text: err?.error?.response || 'Intenta nuevamente.',
        });
        console.error('[UsersComponent] getAllUsers error:', err);
      }
    });
  }
}

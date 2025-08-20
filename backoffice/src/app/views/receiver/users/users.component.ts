import { Component, Input, OnInit } from '@angular/core';

interface User {
  id: number;
  email: string;
  sin_grupo: boolean;
  select: boolean;
}

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [],
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export class UsersComponent implements OnInit {

  @Input() show_details: boolean = true;
  @Input() can_delete: boolean = false;

  users: User[] = [
    { id: 1, email: 'usuario1ejemplo.com', sin_grupo: true,  select: false },
    { id: 2, email: 'usuario2ejemplo.com', sin_grupo: false, select: true  },
    { id: 3, email: 'usuario3ejemplo.com', sin_grupo: false, select: false },
    { id: 4, email: 'usuario4ejemplo.com', sin_grupo: true,  select: false },
    { id: 5, email: 'usuario5ejemplo.com', sin_grupo: false, select: true  }
  ];

  ngOnInit(): void {
    this.sortUsers();
  }

  onToggleSelect(user: User): void {
    user.select = !user.select;
    this.sortUsers();
  }

  /** Prioridad: seleccionados (0) -> no asignados (1) -> asignados (2) */
  private priority(u: User): number {
    if (u.select) return 0;
    if (u.sin_grupo) return 1;
    return 2;
  }

  private sortUsers(): void {
    this.users.sort((a, b) => {
      const pa = this.priority(a);
      const pb = this.priority(b);
      if (pa !== pb) return pa - pb;

      // alfabético ascendente
      return a.email.localeCompare(b.email, undefined, { sensitivity: 'base' });
    });
  }

  inspectUser(user: any) {
    console.log(user);
  }
}

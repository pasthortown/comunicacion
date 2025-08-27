import { Component, Input, OnInit } from '@angular/core';

interface Group {
  id: number;
  name: string;
  sin_grupo: boolean;
  select: boolean;
}

@Component({
  selector: 'app-groups',
  standalone: true,
  imports: [],
  templateUrl: './groups.component.html',
  styleUrls: ['./groups.component.scss']
})
export class GroupsComponent implements OnInit {

  @Input() show_details: boolean = true;

  groups: Group[] = [
    { id: 1, name: 'usuario1ejemplo.com', sin_grupo: true,  select: false },
    { id: 2, name: 'usuario2ejemplo.com', sin_grupo: false, select: true  },
    { id: 3, name: 'usuario3ejemplo.com', sin_grupo: false, select: false },
    { id: 4, name: 'usuario4ejemplo.com', sin_grupo: true,  select: false },
    { id: 5, name: 'usuario5ejemplo.com', sin_grupo: false, select: true  }
  ];

  ngOnInit(): void {
    this.sortGroups();
  }

  onToggleSelect(group: Group): void {
    group.select = !group.select;
    this.sortGroups();
  }

  /** Prioridad: seleccionados (0) -> no asignados (1) -> asignados (2) */
  private priority(u: Group): number {
    if (u.select) return 0;
    if (u.sin_grupo) return 1;
    return 2;
  }

  private sortGroups(): void {
    this.groups.sort((a, b) => {
      const pa = this.priority(a);
      const pb = this.priority(b);
      if (pa !== pb) return pa - pb;

      // alfabético ascendente
      return a.name.localeCompare(b.name, undefined, { sensitivity: 'base' });
    });
  }

  inspectGroup(group: any) {
    console.log(group);
  }
}

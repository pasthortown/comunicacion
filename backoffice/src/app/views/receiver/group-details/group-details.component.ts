import { Component } from '@angular/core';
import { UsersComponent } from '../users/users.component';

@Component({
  selector: 'app-group-details',
  imports: [UsersComponent],
  templateUrl: './group-details.component.html',
  styleUrl: './group-details.component.scss'
})
export class GroupDetailsComponent {

}

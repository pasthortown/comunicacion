import { Component } from '@angular/core';
import { GroupsComponent } from '../groups/groups.component';

@Component({
  selector: 'app-user-details',
  imports: [GroupsComponent],
  templateUrl: './user-details.component.html',
  styleUrl: './user-details.component.scss'
})
export class UserDetailsComponent {

}

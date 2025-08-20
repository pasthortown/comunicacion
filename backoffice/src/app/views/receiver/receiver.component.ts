import { Component } from '@angular/core';
import { IconDirective } from '@coreui/icons-angular';
import {
  ButtonCloseDirective,
  ButtonDirective,
  ColComponent,
  ContainerComponent,
  FormControlDirective,
  InputGroupComponent,
  InputGroupTextDirective,
  ModalBodyComponent,
  ModalComponent,
  ModalFooterComponent,
  ModalHeaderComponent,
  ModalTitleDirective,
  ModalToggleDirective,
  RowComponent
} from '@coreui/angular';
import { UsersComponent } from './users/users.component';
import { GroupsComponent } from './groups/groups.component';
import { UserDetailsComponent } from './user-details/user-details.component';
import { GroupDetailsComponent } from './group-details/group-details.component';

@Component({
  selector: 'app-receiver',
  templateUrl: './receiver.component.html',
  styleUrls: ['./receiver.component.scss'],
  imports: [
    ModalBodyComponent,
    ModalComponent,
    ModalFooterComponent,
    ModalHeaderComponent,
    ModalTitleDirective,
    ModalToggleDirective,
    UserDetailsComponent,
    GroupDetailsComponent,
    UsersComponent,
    GroupsComponent,
    ContainerComponent,
    RowComponent,
    ColComponent,
    InputGroupComponent,
    InputGroupTextDirective,
    IconDirective,
    FormControlDirective,
    ButtonCloseDirective,
    ButtonDirective]
})
export class ReceiverComponent {

  visibleUserDetails: boolean = false;
  visibleGroupDetails: boolean = false;

  handleChangeVisibleUserDetails(event: any) {

  }

  cancelarUserDetails() {
    this.visibleUserDetails = false;
  }

  handleChangeVisibleGroupDetails(event: any) {

  }

  cancelarGroupDetails() {
    this.visibleGroupDetails = false;
  }

}

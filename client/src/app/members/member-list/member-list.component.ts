import { Component, inject, OnInit } from '@angular/core';
import { AccountService } from '../../_services/account.service';
import { MemberService } from '../../_services/member.service';
import { Member } from '../../_models/member';
import { CommonModule } from '@angular/common';
import { MemberCardComponent } from "../member-card/member-card.component";

@Component({
  selector: 'app-member-list',
  standalone: true,
  templateUrl: './member-list.component.html',
  styleUrl: './member-list.component.css',
  imports: [CommonModule, MemberCardComponent]
})
export class MemberListComponent implements OnInit {
  memberService = inject(MemberService);

  ngOnInit(): void {
    if (this.memberService.members().length == 0) this.loadMembers();
  }

  loadMembers() {
    this.memberService.getMembers();
  }

}

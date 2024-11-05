import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { MemberService } from '../../_services/member.service';
import { ActivatedRoute } from '@angular/router';
import { Member } from '../../_models/member';
import { CommonModule } from '@angular/common';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { MemberMessageComponent } from "../member-message/member-message.component";
import { Message } from '../../_models/message';
import { MessageService } from '../../_services/message.service';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css',
  imports: [TabsModule, CommonModule, GalleryModule, MemberMessageComponent]
})
export class MemberDetailComponent implements OnInit {
  @ViewChild('memberTabs', { static: true }) memberTabs?: TabsetComponent;
  memberService = inject(MemberService);
  messageService = inject(MessageService);
  route = inject(ActivatedRoute);
  member: Member = {} as Member;
  images: GalleryItem[] = [];
  activeTab?: TabDirective;
  messages: Message[] = []

  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => {
        this.member = data['member'];
        this.member && this.member.photos.map(p => this.images.push(new ImageItem({ src: p.url, thumb: p.url })))
      }
    })

    this.route.queryParams.subscribe({
      next: params => {
        params['tab'] && this.selectTab(params['tab'])
      }
    })
  }

  onUpdateMessages(event: any) {
    this.messages.push(event);
  }

  selectTab(heading: string) {
    if (this.memberTabs) {
      const selectedTab = this.memberTabs.tabs.find(t => t.heading === heading);
      if (selectedTab) selectedTab.active = true;
    }
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === 'Messages' && this.messages.length === 0 && this.member) {
      this.messageService.getMessageThread(this.member.username).subscribe({
        next: messages => this.messages = messages
      })
    }
  }

  loadMember() {
    let username = this.route.snapshot.paramMap.get('username');
    if (!username) return;
    this.memberService.getMember(username).subscribe({
      next: member => {
        this.member = member;
        member.photos.map(p => this.images.push(new ImageItem({ src: p.url, thumb: p.url })))
      }
    })
  }
}

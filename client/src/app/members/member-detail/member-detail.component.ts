import { Component, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MemberService } from '../../_services/member.service';
import { ActivatedRoute, Router, RouterEvent } from '@angular/router';
import { Member } from '../../_models/member';
import { CommonModule } from '@angular/common';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { MemberMessageComponent } from "../member-message/member-message.component";
import { Message } from '../../_models/message';
import { MessageService } from '../../_services/message.service';
import { PresenceService } from '../../_services/presence.service';
import { AccountService } from '../../_services/account.service';
import { HubConnectionState } from '@microsoft/signalr';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css',
  imports: [TabsModule, CommonModule, GalleryModule, MemberMessageComponent]
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @ViewChild('memberTabs', { static: true }) memberTabs?: TabsetComponent;
  memberService = inject(MemberService);
  messageService = inject(MessageService);
  presenceService = inject(PresenceService);
  accountService = inject(AccountService);
  route = inject(ActivatedRoute);
  router = inject(Router);
  member: Member = {} as Member;
  images: GalleryItem[] = [];
  activeTab?: TabDirective;

  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => {
        this.member = data['member'];
        this.member && this.member.photos.map(p => this.images.push(new ImageItem({ src: p.url, thumb: p.url })))
      }
    })

    this.route.paramMap.subscribe({
      next: _ => this.onRouteParamsChange()
    })

    this.route.queryParams.subscribe({
      next: params => {
        params['tab'] && this.selectTab(params['tab'])
      }
    })
  }

  selectTab(heading: string) {
    if (this.memberTabs) {
      const selectedTab = this.memberTabs.tabs.find(t => t.heading === heading);
      if (selectedTab) selectedTab.active = true;
    }
  }

  onRouteParamsChange() {
    const user = this.accountService.currentUser();
    if (!user) return;
    if (this.messageService.hubConnection?.state === HubConnectionState.Connected && this.activeTab?.heading === 'Messages') {
      this.messageService.hubConnection.stop().then(() => {
        this.messageService.createHubConnection(user, this.member.username);
      })
    }
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: this.activeTab.heading },
      queryParamsHandling: 'merge'
    })
    if (this.activeTab.heading === 'Messages' && this.member) {
      let user = this.accountService.currentUser();
      if (!user) return;
      this.messageService.createHubConnection(user, this.member.username);
    } else {
      this.messageService.stopHubConnection();
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

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }
}

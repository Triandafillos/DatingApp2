import { Component, computed, inject, input } from '@angular/core';
import { Member } from '../../_models/member';
import { RouterLink } from '@angular/router';
import { LikesService } from '../../_services/likes.service';
import { PresenceService } from '../../_services/presence.service';

@Component({
  selector: 'app-member-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './member-card.component.html',
  styleUrl: './member-card.component.css'
})
export class MemberCardComponent {
  private presenceService = inject(PresenceService);
  member = input.required<Member>();
  likesService = inject(LikesService);
  hasLiked = computed(() => this.likesService.likeIds().includes(this.member().appUserId));
  isOnline = computed(() => this.presenceService.onlineUsers().includes(this.member().username));

  toggleLike() {
    this.likesService.toggleLike(this.member().appUserId).subscribe({
      next: () => {
        if (this.hasLiked()) {
          this.likesService.likeIds.update(ids => ids.filter(x => x !== this.member().appUserId))
        }
        else {
          this.likesService.likeIds.update(ids => [...ids, this.member().appUserId])
        }
      }
    })
  }

}

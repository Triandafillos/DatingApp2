import { HttpClient, HttpHeaders, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { AccountService } from './account.service';
import { Observable, of, tap } from 'rxjs';
import { Photo } from '../_models/photo';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  http = inject(HttpClient);
  accountService = inject(AccountService);
  baseUrl = environment.apiUrl;
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);
  user = this.accountService.currentUser();
  userParams = signal<UserParams>(new UserParams(this.user));
  memberCache = new Map();

  resetUserParams() {
    this.userParams.set(new UserParams(this.user));
  }

  getMembers() {
    let response = this.memberCache.get(Object.values(this.userParams()).join('-'));
    if (response) return setPaginatedResponse(response, this.paginatedResult);

    let params = setPaginationHeaders(this.userParams().pageNumber, this.userParams().pageSize);
    params = params.append('minAge', this.userParams().minAge);
    params = params.append('maxAge', this.userParams().maxAge);
    params = params.append('gender', this.userParams().gender);
    params = params.append('orderBy', this.userParams().orderBy);

    return this.http.get<Member[]>(this.baseUrl + 'users', { observe: 'response', params }).subscribe({
      next: response => {
        setPaginatedResponse(response, this.paginatedResult);
        this.memberCache.set(Object.values(this.userParams()).join('-'), response);
      }
    });
  }

  getMember(username: string): Observable<Member> {
    const member = [...this.memberCache.values()].reduce((arr, elem) => arr.concat(elem.body), [])
      .find((m: Member) => m.username === username);
    if (member) return of(member);

    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      tap(() => this.paginatedResult()?.items?.map(m => m.username === member.username ? member : m))
    );
  }

  setMainPhoto(photo: Photo) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photo.photoId, {}).pipe(
      tap(() => {
        this.paginatedResult()?.items?.map(m => {
          if (m.photos.includes(photo)) {
            m.photoUrl = photo.url;
          }
          return m;
        })
      })
    );
  }

  deletePhoto(photo: Photo) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photo.photoId).pipe(
      tap(() => {
        this.paginatedResult()?.items?.map(m => {
          if (m.photos.includes(photo)) {
            m.photos = m.photos.filter(p => p.photoId != photo.photoId)
          }
          return m;
        })
      })
    );
  }
}

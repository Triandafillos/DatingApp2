import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { PhotosForApproval } from '../../_models/photo';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-photo-management',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css'
})
export class PhotoManagementComponent implements OnInit {
  adminService = inject(AdminService);
  toast = inject(ToastrService);
  photosForApproval: PhotosForApproval[] = [];

  ngOnInit(): void {
    this.loadPhotos();
  }

  loadPhotos() {
    this.adminService.getPhotosForApproval().subscribe({
      next: photos => this.photosForApproval = photos
    })
  }

  approvePhoto(photoId: number) {
    this.adminService.approvePhoto(photoId).subscribe({
      next: () => {
        this.loadPhotos();
        this.toast.success('Photo approved');
      }
    })
  }

}

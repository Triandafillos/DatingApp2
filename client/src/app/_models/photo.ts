export interface Photo {
    photoId: number
    url: string
    isMain: boolean
    isApproved?: boolean
}

export interface PhotosForApproval {
    username: string,
    photos: Photo[]
}
namespace API.DTOs
{
    public class PhotoForApprovalDto
    {
        public string? Username { get; set; }
        public List<PhotoDto>? Photos { get; set; }
    }
}

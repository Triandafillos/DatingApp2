using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities
{
    [Table("Photos")]
    public class Photo
    {
        public int PhotoId { get; set; }
        public required string Url { get; set; }
        public bool IsMain { get; set; }
        public string? PublicId { get; set; }
        public required bool IsApproved { get; set; } = false;

        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; } = null!;
    }
}

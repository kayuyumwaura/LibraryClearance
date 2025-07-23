using System.ComponentModel.DataAnnotations;

namespace LibraryClearance.Models
{
    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class ClearanceRequest
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Request Title")]
        public string Title { get; set; }

        [Required]
        public string Campus { get; set; }

        [Required]
        public string Department { get; set; }

        [Required]
        [Display(Name = "Requester Name")]
        public string RequesterName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Extension { get; set; }

        [Required]
        [Display(Name = "User Category")]
        public string UserCategory { get; set; }

        [Display(Name = "Other User Category (if applicable)")]
        public string OtherUserCategory { get; set; }

        [Required]
        [Display(Name = "Clearance Purpose")]
        public string ClearancePurpose { get; set; }

        [Display(Name = "Other Clearance Purpose (if applicable)")]
        public string OtherClearancePurpose { get; set; }

        [Required]
        [Display(Name = "Use of Content")]
        public string UseOfContent { get; set; }

        [Display(Name = "Other Use of Content (if applicable)")]
        public string OtherUseOfContent { get; set; }

        [Required]
        public string Duration { get; set; }

        [Display(Name = "Other Duration (if applicable)")]
        public string OtherDuration { get; set; }

        [Required]
        [Display(Name = "Article Title")]
        public string ArticleTitle { get; set; }

        [Required]
        public string Authors { get; set; }

        [Required]
        [Display(Name = "Journal Title")]
        public string JournalTitle { get; set; }

        public string Volume { get; set; }

        public string Issue { get; set; }

        [Display(Name = "Page Number")]
        public string PageNumber { get; set; }

        [Required]
        [Display(Name = "Publication Year")]
        public string PublicationYear { get; set; }

        [Required]
        public string Publisher { get; set; }

        [Display(Name = "DOI")]
        public string Doi { get; set; }

        [Display(Name = "URL")]
        public string Url { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        [Display(Name = "Admin Comments")]
        public string AdminComments { get; set; }

        [Display(Name = "Submitted Date")]
        public DateTime SubmittedDate { get; set; } = DateTime.Now;

        [Display(Name = "Last Updated")]
        public DateTime? LastUpdated { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
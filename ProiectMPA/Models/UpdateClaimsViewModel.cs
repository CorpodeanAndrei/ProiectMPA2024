namespace ProiectMPA.Models
{
    public class UpdateClaimsViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        public List<ClaimViewModel> Claims { get; set; }
    }
}

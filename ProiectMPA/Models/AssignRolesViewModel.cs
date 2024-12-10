using Microsoft.AspNetCore.Mvc.Rendering;


namespace ProiectMPA.Models
{
    public class AssignRolesViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<UserRoleAssignmentViewModel> Members { get; set; } = new();
        public List<UserRoleAssignmentViewModel> NonMembers { get; set; } = new();
        public List<string> MemberIds { get; set; } = new();
        public List<string> NonMemberIds { get; set; } = new();
    }
}

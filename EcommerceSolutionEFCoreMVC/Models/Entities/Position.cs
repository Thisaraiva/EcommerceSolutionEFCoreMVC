namespace EcommerceSolutionEFCoreMVC.Models.Entities
{
    public class Position
    {
        public int PositionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public ICollection<ApplicationUser> ApplicationUser { get; set; } = new List<ApplicationUser>();
    }
}

namespace EcommerceSolutionEFCoreMVC.Services.Interfaces
{
    public interface ISeedUserRoleInitial
    {
        Task SeedRolesAsync();
        Task SeedUsersAsync();
    }
}

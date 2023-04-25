namespace MultipleAuthIdentity.Services
{
    public interface IAdminService
    {
        public void changeUsersPanel(int month);
        public List<string> getMonthlyUsers();
    }
}

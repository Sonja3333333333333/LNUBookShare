using LNUBookShareBLL.DTOs;

namespace LNUBookShareUI.Common
{
    public class UserSession : IUserSession
    {
        public LoginResultDto? CurrentUser { get; set; }

        public int GetUserId()
        {
            return CurrentUser?.UserId ?? 0;
        }

        public bool IsLoggedIn()
        {
            return CurrentUser != null;
        }

        public void ClearSession()
        {
            CurrentUser = null;
        }
    }
}
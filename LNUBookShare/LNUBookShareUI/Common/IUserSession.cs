using LNUBookShareBLL.DTOs;

namespace LNUBookShareUI.Common
{
    public interface IUserSession
    {
        LoginResultDto? CurrentUser { get; set; }
        int GetUserId();
        bool IsLoggedIn();
    }
}
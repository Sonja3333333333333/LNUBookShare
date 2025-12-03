using System.Threading.Tasks;

namespace LNUBookShareUI.Common
{
    public interface INavigationService
    {
        void ShowProfile();

        void ShowViewProfile(int id);

        void ShowFavorites();

        void ShowMainView();

        void ShowLogin();

        void ShowRegister();

        Task ShowEditProfile();

        Task ShowEditBookAsync(int bookId);

        void ShowBookDetails(int bookId);

        Task ShowAddBookAsync();
    }
}
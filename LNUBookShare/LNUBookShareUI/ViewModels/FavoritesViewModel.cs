using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LNUBookShareBLL.DTOs;                
using LNUBookShareBLL.Enums;                
using LNUBookShareBLL.Features.Favorites;

namespace LNUBookShareUI.ViewModels 
{
    public partial class FavoritesViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        public ObservableCollection<FavoriteBookCardDto> FavoriteBooks { get; } = new();

        [ObservableProperty]
        private BookFilterStatus _selectedFilter = BookFilterStatus.All;
        [ObservableProperty]
        private BookSortCriteria _selectedSort = BookSortCriteria.Title;
        [ObservableProperty]
        private int _pageNumber = 1;
        [ObservableProperty]
        private int _pageSize = 10;
        [ObservableProperty]
        private int _totalCount;

        public FavoritesViewModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        [RelayCommand]
        public async Task LoadFavoritesAsync()
        {
            int currentUserId = 1; 

            if (PageNumber == 1)
            {
                FavoriteBooks.Clear();
            }

            try
            {
                var query = new GetFavoriteBooksQuery
                {
                    CurrentUserId = currentUserId,
                    FilterBy = SelectedFilter,
                    SortBy = SelectedSort,
                    PageNumber = PageNumber,
                    PageSize = PageSize
                };

                var result = await _mediator.Send(query);

                foreach (var book in result.Items)
                {
                    FavoriteBooks.Add(book);
                }
                TotalCount = result.TotalCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка завантаження уподобаних: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task RemoveFromFavorites(int bookId) 
        {
            if (bookId == 0) return;
            int currentUserId = 1; 
            await _mediator.Send(new ToggleFavoriteCommand
            {
                BookId = bookId,
                UserId = currentUserId
            });

            var bookToRemove = FavoriteBooks.FirstOrDefault(b => b.BookId == bookId);
            if (bookToRemove != null)
            {
                FavoriteBooks.Remove(bookToRemove);
            }
        }

        [RelayCommand]
        private async Task ApplyFilter(string status)
        {
            SelectedFilter = status switch
            {
                "Available" => BookFilterStatus.Available,
                "Issued" => BookFilterStatus.Issued,
                _ => BookFilterStatus.All
            };

            PageNumber = 1;
            await LoadFavoritesAsync(); 
        }
    }
}
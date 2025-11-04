using System;
using System.Windows.Controls;
using System.Windows;
using LNUBookShareUI.ViewModels;
using System.Windows;

namespace LNUBookShareUI.Views 
{
    public partial class FavoritesView : Window
    {
        private readonly FavoritesViewModel _viewModel;

        public FavoritesView(FavoritesViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel.LoadFavoritesCommand != null && _viewModel.LoadFavoritesCommand.CanExecute(null))
            {
                _viewModel.LoadFavoritesCommand.Execute(null);
            }
        }
    }
}
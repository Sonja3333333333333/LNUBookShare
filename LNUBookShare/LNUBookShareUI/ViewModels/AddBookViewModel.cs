using MediatR;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LNUBookShareBLL.DTOs;
using LNUBookShareBLL.Features.Books;
using LNUBookShareBLL.Features.Categories;
using LNUBookShareBLL.Features.Files;
using LNUBookShareUI.Common;
using System.Linq;
using System;

namespace LNUBookShareUI.ViewModels
{
    public class AddBookViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly IUserSession _userSession;

        private string _title = string.Empty;
        public string Title
        {
            get => this._title;
            set => this.SetProperty(ref this._title, value);
        }

        private string _author = string.Empty;
        public string Author
        {
            get => this._author;
            set => this.SetProperty(ref this._author, value);
        }

        private string _isbn;
        public string Isbn
        {
            get => this._isbn;
            set => this.SetProperty(ref this._isbn, value);
        }

        private int? _year;
        public int? Year
        {
            get => this._year;
            set => this.SetProperty(ref this._year, value);
        }

        private string _publisher;
        public string Publisher
        {
            get => this._publisher;
            set => this.SetProperty(ref this._publisher, value);
        }

        private string _language;
        public string Language
        {
            get => this._language;
            set => this.SetProperty(ref this._language, value);
        }

        private string _coverImagePath;
        public string CoverImagePath
        {
            get => this._coverImagePath;
            set => this.SetProperty(ref this._coverImagePath, value);
        }

        public ObservableCollection<CategoryDto> Categories { get; } = new();
        private CategoryDto _selectedCategory;
        public CategoryDto SelectedCategory
        {
            get => this._selectedCategory;
            set => this.SetProperty(ref this._selectedCategory, value);
        }

        public ICommand ChangeCoverCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddBookViewModel(IMediator mediator, IUserSession userSession)
        {
            this._mediator = mediator;
            this._userSession = userSession;
            this.ChangeCoverCommand = new RelayCommand(async () => await this.ChangeCover());
            this.SaveCommand = new RelayCommand<object>(async (w) => await this.Save(w));
            this.CancelCommand = new RelayCommand<object>(this.Cancel);
        }

        public async Task LoadDataAsync()
        {
            var categoryList = await this._mediator.Send(new GetAllCategoriesQuery());
            this.Categories.Clear();
            foreach (var category in categoryList)
            {
                this.Categories.Add(category);
            }
            this.SelectedCategory = this.Categories.FirstOrDefault();
        }

        private async Task ChangeCover()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = openFileDialog.FileName;
                    byte[] imageData = File.ReadAllBytes(filePath);

                    var uploadCommand = new UploadImageCommand
                    {
                        FileName = Path.GetFileName(filePath),
                        ImageData = imageData
                    };

 
                    string newPhysicalPath = await this._mediator.Send(uploadCommand);
                    this.CoverImagePath = newPhysicalPath; 
                }
                catch (Exception ex)
                {
                    _ = MessageBox.Show($"Не вдалося завантажити фото: {ex.Message}", "Помилка");
                }
            }
        }

        private async Task Save(object window)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.Title) || string.IsNullOrWhiteSpace(this.Author) || this.SelectedCategory == null)
                {
                    throw new Exception("Назва, Автор та Категорія є обов'язковими.");
                }

                var dto = new AddBookDto
                {
                    Title = this.Title,
                    Author = this.Author,
                    Isbn = this.Isbn,
                    Year = this.Year,
                    Publisher = this.Publisher,
                    Language = this.Language,
                    CategoryId = this.SelectedCategory.CategoryId,
                    CoverImagePath = this.CoverImagePath
                };

                var command = new AddBookCommand
                {
                    Dto = dto,
                    OwnerUserId = _userSession.GetUserId()
                };

                _ = await this._mediator.Send(command);

                if (window is Window w) { w.Close(); }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Не вдалося зберегти книгу: {ex.Message}", "Помилка");
            }
        }

        private void Cancel(object window)
        {
            if (window is Window w) { w.Close(); }
        }
    }
}

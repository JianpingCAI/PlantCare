using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data.DbModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PlantCare.App.ViewModels
{
    public partial class PlantAddEditViewModel : ViewModelBase
    {
        private readonly IPlantService _plantService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        public PlantAddEditViewModel(IPlantService plantService, IDialogService dialogService, INavigationService navigationService)
        {
            ErrorsChanged += AddPlantViewModel_ErrorsChanged;
            _plantService = plantService;
            _dialogService = dialogService;
            _navigationService = navigationService;
        }

        [ObservableProperty]
        private Guid _id;

        [ObservableProperty]
        private string _pageTitle = default!;

        [ObservableProperty]
        //[Required]
        [MinLength(3)]
        [MaxLength(50)]
        [NotifyDataErrorInfo]
        private string? _name;

        [ObservableProperty]
        private string _species = string.Empty;

        [ObservableProperty]
        private int _age;

        [ObservableProperty]
        private DateTime _lastWatered = DateTime.Now;

        [ObservableProperty]
        private string _photoPath = string.Empty;

        [RelayCommand]
        private async Task Submit()
        {
            ValidateAllProperties();

            if (Errors.Any())
            {
                return;
            }

            PlantDbModel plant = MapToPlantModel();

            // Add/Create a plant
            if (Id == Guid.Empty)
            {
                try
                {
                    await _plantService.CreatePlantAsync(plant);
                }
                catch (Exception ex)
                {
                    await _dialogService.Notify("Failed", "Adding the plant failed.");
                }

                WeakReferenceMessenger.Default.Send(new PlantAddedOrChangedMessage());

                await _dialogService.Notify("Success", "The event is added.");
                await _navigationService.GoToPlantsOverview();
            }
            // Edit/Update a plant
            else
            {
                try
                {
                    await _plantService.UpdatePlantAsync(plant);
                }
                catch
                {
                    await _dialogService.Notify("Failed", "Editing the plant failed.");
                }

                WeakReferenceMessenger.Default.Send(new PlantAddedOrChangedMessage());
                await _dialogService.Notify("Success", "The plant is updated.");
                await _navigationService.GoBack();
            }
        }

        #region Photo related

        [RelayCommand]
        private async Task UploadImage()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Please select an image"
                });

                if (result != null)
                {
                    var stream = await result.OpenReadAsync();
                    var filePath = Path.Combine(FileSystem.AppDataDirectory, result.FileName);

                    using (var newStream = File.OpenWrite(filePath))
                    {
                        await stream.CopyToAsync(newStream);
                    }

                    PhotoPath = filePath;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
        }

        [RelayCommand]
        public async Task TakePhoto()
        {
            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

                    if (photo != null)
                    {
                        // save the file into local storage
                        string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                        using Stream sourceStream = await photo.OpenReadAsync();
                        using FileStream localFileStream = File.OpenWrite(localFilePath);

                        await sourceStream.CopyToAsync(localFileStream);

                        PhotoPath = localFilePath;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
        }

        #endregion Photo related

        private PlantDbModel MapToPlantModel()
        {
            return new PlantDbModel
            {
                Id = Guid.NewGuid(),
                Name = Name,
                Species = Species,
                Age = Age,
                LastWatered = LastWatered,
                PhotoPath = PhotoPath,
            };
        }

        #region Data Validation

        public ObservableCollection<ValidationResult> Errors { get; } = new();

        private void AddPlantViewModel_ErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
        {
            Errors.Clear();
            GetErrors().ToList().ForEach(Errors.Add);
            SubmitCommand.NotifyCanExecuteChanged();
        }

        #endregion Data Validation
    }
}
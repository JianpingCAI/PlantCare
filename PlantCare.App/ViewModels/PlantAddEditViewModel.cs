using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data;
using PlantCare.Data.DbModels;
using PlantCare.Data.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace PlantCare.App.ViewModels
{
    public partial class PlantAddEditViewModel : ViewModelBase, IQueryAttributable
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
        private string _pageTitle = default!;

        private Guid Id { get; set; } = default;

        [ObservableProperty]
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        [NotifyDataErrorInfo]
        private string _name = "Unknown";

        [ObservableProperty]
        private string _species = "Unknown";

        [ObservableProperty]
        [Range(0, 3000)]
        private int _age = 1;

        [ObservableProperty]
        [Required]
        private DateTime _lastWateredDate = DateTime.Now.Date;

        [ObservableProperty]
        [Required]
        private TimeSpan _lastWateredTime = DateTime.Now.TimeOfDay;

        public DateTime LastWatered => LastWateredDate + LastWateredTime;

        [ObservableProperty]
        [Required]
        [Range(0, 365)]
        [NotifyPropertyChangedFor(nameof(NextWateringTime))]
        [NotifyPropertyChangedFor(nameof(WateringFrequencyInHours))]
        private int _wateringFrequencyDays = 0;

        [ObservableProperty]
        [Required]
        [Range(0, 24)]
        [NotifyPropertyChangedFor(nameof(NextWateringTime))]
        [NotifyPropertyChangedFor(nameof(WateringFrequencyInHours))]
        private int _wateringFrequencyHours = 0;

        public int WateringFrequencyInHours => 24 * WateringFrequencyDays + WateringFrequencyHours;

        public DateTime NextWateringTime => LastWatered.AddDays(WateringFrequencyDays).AddHours(WateringFrequencyHours);

        [ObservableProperty]
        private string _photoPath = Consts.DefaultPhotoPath;

        [RelayCommand]
        private async Task Submit()
        {
            try
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
                    catch (Exception)
                    {
                        await _dialogService.Notify("Failed", "Adding the plant failed.");
                    }

                    WeakReferenceMessenger.Default.Send(new PlantAddedOrChangedMessage());

                    await _dialogService.Notify("Success", "The plant is added.");
                    await _navigationService.GoToPlantsOverview();
                }
                // Edit/Update a plant
                else
                {
                    bool updated = false;
                    try
                    {
                        updated = await _plantService.UpdatePlantAsync(plant);
                    }
                    catch (Exception e)
                    {
                        await _dialogService.Notify("Error", e.Message);
                    }

                    if (!updated)
                    {
                        await _dialogService.Notify("Failed", "Editing the plant failed.");
                    }
                    else
                    {
                        WeakReferenceMessenger.Default.Send(new PlantAddedOrChangedMessage { PlantId = Id });
                        await _dialogService.Notify("Success", "The plant is updated.");
                        //await _navigationService.GoBack();
                        await _navigationService.GoToPlantDetail(plant.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.Notify("Error", ex.Message);
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
                await _dialogService.Notify("Error", ex.Message, "Error");
            }
        }

        [RelayCommand]
        public async Task TakePhoto()
        {
            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    FileResult? photo = await MediaPicker.Default.CapturePhotoAsync();

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
                await _dialogService.Notify("Error", ex.Message, "Error");
            }
        }

        #endregion Photo related

        private PlantDbModel MapToPlantModel()
        {
            return new PlantDbModel
            {
                Id = Id,
                Name = Name,
                Species = Species,
                Age = Age,
                LastWatered = LastWatered,
                PhotoPath = PhotoPath,
                WateringFrequencyInHours = WateringFrequencyInHours,
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

        void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (!query.ContainsKey("Plant"))
                return;

            PlantDbModel? plant = query["Plant"] as PlantDbModel;
            if (plant == null) return;

            MapPlantData(plant);
        }

        private void MapPlantData(PlantDbModel plant)
        {
            Id = plant.Id;
            Name = plant.Name;
            Species = plant.Species;
            PhotoPath = plant.PhotoPath;
            Age = plant.Age;

            LastWateredDate = plant.LastWatered.Date;
            LastWateredTime = plant.LastWatered.TimeOfDay;

            WateringFrequencyDays = plant.WateringFrequencyInHours / 24;
            WateringFrequencyHours = plant.WateringFrequencyInHours % 24;
        }

        internal async Task NavigateBack()
        {
            if (Id == default)
            {
                await _navigationService.GoToPlantsOverview();
            }
            else
            {
                await _navigationService.GoToPlantDetail(Id);
            }
        }
    }
}
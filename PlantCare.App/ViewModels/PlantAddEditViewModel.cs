using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.App.Services.DBService;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data;
using PlantCare.Data.DbModels;
using PlantCare.Data.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
        [MinLength(1)]
        [MaxLength(50)]
        [NotifyDataErrorInfo]
        private string _name = "Plant";

        [ObservableProperty]
        private string _species = "Unknown";

        [ObservableProperty]
        [Range(0, 3000)]
        private int _age = 1;

        #region Watering

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
        [NotifyPropertyChangedFor(nameof(WateringFrequencyInHours))]
        private int _wateringFrequencyDays = 3;

        [ObservableProperty]
        [Required]
        [Range(0, 24)]
        [NotifyPropertyChangedFor(nameof(WateringFrequencyInHours))]
        private int _wateringFrequencyHours = 0;

        public int WateringFrequencyInHours => 24 * WateringFrequencyDays + WateringFrequencyHours;
        #endregion Watering

        #region Fertilization

        [ObservableProperty]
        [Required]
        private DateTime _lastFertilizationDate = DateTime.Now.Date;

        [ObservableProperty]
        [Required]
        private TimeSpan _lastFertilizationTime = DateTime.Now.TimeOfDay;

        public DateTime LastFertilization => LastFertilizationDate + LastFertilizationTime;

        [ObservableProperty]
        [Required]
        [Range(0, 365)]
        //[NotifyPropertyChangedFor(nameof(NextWateringTime))]
        [NotifyPropertyChangedFor(nameof(FertilizationFrequencyInHours))]
        private int _fertilizationFrequencyDays = 30;

        [ObservableProperty]
        [Required]
        [Range(0, 24)]
        //[NotifyPropertyChangedFor(nameof(NextWateringTime))]
        [NotifyPropertyChangedFor(nameof(WateringFrequencyInHours))]
        private int _fertilizationFrequencyHours = 0;

        public int FertilizationFrequencyInHours => 24 * FertilizationFrequencyDays + FertilizationFrequencyHours;

        //public DateTime NextWateringTime => LastWatered.AddDays(WateringFrequencyDays).AddHours(WateringFrequencyHours);

        #endregion Fertilization

        [ObservableProperty]
        private string _photoPath = Consts.DefaultPhotoPath;

        [RelayCommand]
        private async Task SetCurrentTimeAsLastWatered()
        {
            try
            {
                await Task.Run(() =>
                    {
                        LastWateredDate = DateTime.Now.Date;
                        LastWateredTime = DateTime.Now.TimeOfDay;
                    });
            }
            catch (Exception ex)
            {
                await _dialogService.Notify("Error", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task SetCurrentTimeAsLastFertilization()
        {
            try
            {
                await Task.Run(() =>
                {
                    LastFertilizationDate = DateTime.Now.Date;
                    LastFertilizationTime = DateTime.Now.TimeOfDay;
                });
            }
            catch (Exception ex)
            {
                await _dialogService.Notify("Error", ex.Message, "OK");
            }
        }

        [RelayCommand(CanExecute = nameof(CanSubmitPlant))]
        private async Task Submit()
        {
            try
            {
                ValidateAllProperties();

                if (Errors.Any())
                {
                    return;
                }

                IsLoading = true;

                // Add/Create a plant
                if (Id == default)
                {
                    try
                    {
                        await Task.Run(async () =>
                        {
                            PlantDbModel plant = MapToPlantModel();
                            Id = await _plantService.CreatePlantAsync(plant);
                        });
                    }
                    catch (Exception)
                    {
                        await _dialogService.Notify("Failed", "Adding the plant failed.");
                    }

                    WeakReferenceMessenger.Default.Send(new PlantAddedMessage(Id));

                    await _dialogService.Notify("Success", "The plant is added.");
                    await _navigationService.GoToPlantsOverview();
                }
                // Edit/Update a plant
                else
                {
                    bool updated = false;
                    try
                    {
                        await Task.Run(async () =>
                        {
                            PlantDbModel plant = MapToPlantModel();
                            updated = await _plantService.UpdatePlantAsync(plant);
                        });
                    }
                    catch (Exception e)
                    {
                        await _dialogService.Notify("Error", e.Message);
                    }

                    if (updated)
                    {
                        WeakReferenceMessenger.Default.Send(new PlantModifiedMessage(Id));
                        await _dialogService.Notify("Success", "The plant is updated.");
                        //await _navigationService.GoBack();
                        await _navigationService.GoToPlantDetail(Id);
                    }
                    else
                    {
                        await _dialogService.Notify("Failed", "Editing the plant failed.");
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.Notify("Error", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanSubmitPlant() => !HasErrors;

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
                await _dialogService.Notify("Error", ex.Message, "OK");
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
                await _dialogService.Notify("Error", ex.Message, "OK");
            }
        }

        #endregion Photo related

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
            if (query is null)
            {
                return;
            }

            // edit an existing plant
            if (query.TryGetValue("Plant", out object? value))
            {
                if (value is not Plant plant) return;

                MapPlantData(plant);
            }
            // add a new plant
            else if (query.TryGetValue("PlantCount", out object? plantCount))
            {
                // give a default name according to the existing count of plants
                Name = $"Plant {plantCount}";
            }
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

        private PlantDbModel MapToPlantModel()
        {
            return new PlantDbModel
            {
                Id = Id,
                Name = Name,
                Species = Species,
                Age = Age,
                PhotoPath = PhotoPath,

                LastWatered = LastWatered,
                WateringFrequencyInHours = WateringFrequencyInHours,

                LastFertilized = LastFertilization,
                FertilizeFrequencyInHours = FertilizationFrequencyInHours
            };
        }

        private void MapPlantData(Plant plant)
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

            LastFertilizationDate = plant.LastFertilized.Date;
            LastFertilizationTime = plant.LastFertilized.TimeOfDay;
            FertilizationFrequencyDays = plant.FertilizeFrequencyInHours / 24;
            FertilizationFrequencyHours = plant.FertilizeFrequencyInHours % 24;
        }
    }
}
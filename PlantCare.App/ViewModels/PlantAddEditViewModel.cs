using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.App.Services.DBService;
using PlantCare.App.Utils;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data;
using PlantCare.Data.DbModels;
using PlantCare.Data.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PlantCare.App.ViewModels
{
    public partial class PlantAddEditViewModel : ViewModelBase, IQueryAttributable, IDisposable
    {
        private readonly IPlantService _plantService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        public PlantAddEditViewModel(IPlantService plantService, IDialogService dialogService, INavigationService navigationService)
        {
            ErrorsChanged += ViewModel_ErrorsChanged;
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

        // For detecting changes
        private DateTime _originalLastWatered;

        private DateTime _originalLastFertilized;

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
        private string _photoPath = ConstStrings.DefaultPhotoPath;

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
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message, "OK");
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
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message, "OK");
            }
        }

        /// <summary>
        /// Save a new plant or changes to a plant
        /// </summary>
        /// <returns></returns>
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
                IsBusy = true;

                // Add/Create a plant
                if (Id == default)
                {
                    try
                    {
                        PlantDbModel plant = MapViewModelPropertiesToPlantModel(isExistingPlant: false);
                        Id = await _plantService.CreatePlantAsync(plant);
                    }
                    catch (Exception)
                    {
                        await _dialogService.Notify("Failed", "Adding the plant failed.");
                    }

                    WeakReferenceMessenger.Default.Send(new PlantAddedMessage(Id));

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    _navigationService.GoToPlantsOverview();

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                // Edit/Update a plant
                else
                {
                    bool updated = false;
                    try
                    {
                        
                        PlantDbModel plant = MapViewModelPropertiesToPlantModel(isExistingPlant: true);
                        updated = await _plantService.UpdatePlantAsync(plant);

                        if (_originalLastWatered != plant.LastWatered)
                        {
                            await _plantService.AddWateringHistoryAsync(plant.Id, plant.LastWatered);
                        }
                        if (_originalLastFertilized != plant.LastFertilized)
                        {
                            await _plantService.AddFertilizationHistoryAsync(plant.Id, plant.LastFertilized);
                        }
                    }
                    catch (Exception e)
                    {
                        await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, e.Message);
                    }

                    if (updated)
                    {
                        WeakReferenceMessenger.Default.Send(new PlantModifiedMessage(Id));

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        _navigationService.GoToPlantDetail(Id);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    else
                    {
                        await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, "Editing the plant failed.");
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
            }
            finally
            {
                IsLoading = false;
                IsBusy = false;
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
                    PickerTitle = $"{LocalizationManager.Instance[ConstStrings.PickPhoto]??ConstStrings.PickPhoto}"
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
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message, "OK");
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
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message, "OK");
            }
        }

        #endregion Photo related

        #region Data Validation

        public ObservableCollection<ValidationResult> Errors { get; } = [];

        private void ViewModel_ErrorsChanged(object? sender, DataErrorsChangedEventArgs e)
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

                MapPlantDataToViewModelProperties(plant);
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

        private PlantDbModel MapViewModelPropertiesToPlantModel(bool isExistingPlant)
        {
            PlantDbModel dbModel = new()
            {
                Name = Name,
                Species = Species,
                Age = Age,
                PhotoPath = PhotoPath,

                LastWatered = LastWatered,
                WateringFrequencyInHours = WateringFrequencyInHours,

                LastFertilized = LastFertilization,
                FertilizeFrequencyInHours = FertilizationFrequencyInHours
            };

            // An existing plant
            if (isExistingPlant)
            {
                dbModel.Id = Id;
            }
            // A new plant
            else
            {
                dbModel.WateringHistories = [new WateringHistory { PlantId = Id, CareTime = LastWatered }];
                dbModel.FertilizationHistories = [new FertilizationHistory { PlantId = Id, CareTime = LastFertilization }];
            }

            return dbModel;
        }

        private void MapPlantDataToViewModelProperties(Plant plant)
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

            _originalLastWatered = plant.LastWatered;
            _originalLastFertilized = plant.LastFertilized;
        }

        public void Dispose()
        {
            ErrorsChanged -= ViewModel_ErrorsChanged;
        }
    }
}
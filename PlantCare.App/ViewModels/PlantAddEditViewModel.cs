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
        private readonly IImageOptimizationService _imageOptimizationService;

        public PlantAddEditViewModel(
            IPlantService plantService, 
            IDialogService dialogService, 
            INavigationService navigationService,
            IImageOptimizationService imageOptimizationService)
        {
            ErrorsChanged += ViewModel_ErrorsChanged;
            _plantService = plantService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _imageOptimizationService = imageOptimizationService;
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

        [ObservableProperty]
        private string _notes = string.Empty;

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
                LastWateredDate = DateTime.Now.Date;
                LastWateredTime = DateTime.Now.TimeOfDay;
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
                LastFertilizationDate = DateTime.Now.Date;
                LastFertilizationTime = DateTime.Now.TimeOfDay;
            }
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message, "OK");
            }
        }

        [RelayCommand]
        private void IncrementWateringDays()
        {
            if (WateringFrequencyDays < 365)
            {
                WateringFrequencyDays++;
            }
        }

        [RelayCommand]
        private void DecrementWateringDays()
        {
            if (WateringFrequencyDays > 0)
            {
                WateringFrequencyDays--;
            }
        }

        [RelayCommand]
        private void IncrementWateringHours()
        {
            if (WateringFrequencyHours < 23)
            {
                WateringFrequencyHours++;
            }
        }

        [RelayCommand]
        private void DecrementWateringHours()
        {
            if (WateringFrequencyHours > 0)
            {
                WateringFrequencyHours--;
            }
        }

        [RelayCommand]
        private void IncrementFertilizationDays()
        {
            if (FertilizationFrequencyDays < 365)
            {
                FertilizationFrequencyDays++;
            }
        }

        [RelayCommand]
        private void DecrementFertilizationDays()
        {
            if (FertilizationFrequencyDays > 0)
            {
                FertilizationFrequencyDays--;
            }
        }

        [RelayCommand]
        private void IncrementFertilizationHours()
        {
            if (FertilizationFrequencyHours < 23)
            {
                FertilizationFrequencyHours++;
            }
        }

        [RelayCommand]
        private void DecrementFertilizationHours()
        {
            if (FertilizationFrequencyHours > 0)
            {
                FertilizationFrequencyHours--;
            }
        }

        [RelayCommand]
        private void IncrementAge()
        {
            if (Age < 3000)
            {
                Age++;
            }
        }

        [RelayCommand]
        private void DecrementAge()
        {
            if (Age > 0)
            {
                Age--;
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
                        PlantDbModel plant = MapViewModelToDBModel(isExistingPlant: false);
                        Id = await _plantService.CreatePlantAsync(plant);
                    }
                    catch (Exception)
                    {
                        await _dialogService.Notify("Failed", "Adding the plant failed.");
                    }

                    WeakReferenceMessenger.Default.Send(new PlantAddedMessage(Id));

                    await _navigationService.GoToPlantsOverview();
                }
                // Edit/Update a plant
                else
                {
                    bool updated = false;
                    try
                    {
                        PlantDbModel plant = MapViewModelToDBModel(isExistingPlant: true);

                        if (_originalLastWatered != plant.LastWatered)
                        {
                            await _plantService.AddWateringHistoryAsync(plant.Id, plant.LastWatered);
                        }
                        if (_originalLastFertilized != plant.LastFertilized)
                        {
                            await _plantService.AddFertilizationHistoryAsync(plant.Id, plant.LastFertilized);
                        }

                        // no need to update the last watering/fertilization time, so revert them
                        // this is not very good design, but for performance concern
                        if (_originalLastWatered > plant.LastWatered)
                        {
                            plant.LastWatered = _originalLastWatered;
                        }
                        if (_originalLastFertilized > plant.LastFertilized)
                        {
                            plant.LastFertilized = _originalLastFertilized;
                        }

                        updated = await _plantService.UpdatePlantAsync(plant);
                    }
                    catch (Exception e)
                    {
                        await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, e.Message);
                    }

                    if (updated)
                    {
                        WeakReferenceMessenger.Default.Send(new PlantModifiedMessage(Id));

                        await _navigationService.GoToPlantDetail(Id);
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
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    IsLoading = false;
                    IsBusy = false;
                });
            }
        }

        private bool CanSubmitPlant() => !HasErrors;

        #region Photo related

        [RelayCommand]
        private async Task UploadImage()
        {
            try
            {
                IsLoading = true;
                FileResult? photoFileResult = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = $"{LocalizationManager.Instance[ConstStrings.PickPhoto] ?? ConstStrings.PickPhoto}"
                });

                if (photoFileResult != null)
                {
                    using Stream sourceStream = await photoFileResult.OpenReadAsync();

                    // Generate unique filename
                    string fileExtension = Path.GetExtension(photoFileResult.FileName) ?? ".jpg";
                    string fileName = $"{Guid.NewGuid()}{fileExtension}";

                    // Optimize and save the image (includes thumbnail generation)
                    string localFilePath = await _imageOptimizationService.OptimizeAndSaveImageAsync(
                        sourceStream, 
                        fileName);

                    await MainThread.InvokeOnMainThreadAsync(() => { PhotoPath = localFilePath; });
                }
            }
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task TakePhoto()
        {
            try
            {
                IsLoading = true;

                if (MediaPicker.Default.IsCaptureSupported)
                {
                    FileResult? photo = await MediaPicker.Default.CapturePhotoAsync();

                    if (photo != null)
                    {
                        using Stream sourceStream = await photo.OpenReadAsync();

                        // Generate unique filename
                        string fileExtension = Path.GetExtension(photo.FileName) ?? ".jpg";
                        string fileName = $"{Guid.NewGuid()}{fileExtension}";

                        // Optimize and save the photo (includes thumbnail generation)
                        string localFilePath = await _imageOptimizationService.OptimizeAndSaveImageAsync(
                            sourceStream, 
                            fileName);

                        await MainThread.InvokeOnMainThreadAsync(() => { PhotoPath = localFilePath; });
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
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

            MainThread.BeginInvokeOnMainThread(() =>
            {
                // edit an existing plant
                if (query.TryGetValue("Plant", out object? value))
                {
                    if (value is not Plant plant) return;

                    MapToViewModel(plant);
                }
                // add a new plant
                else if (query.TryGetValue("PlantCount", out object? plantCount))
                {
                    // give a default name according to the existing count of plants
                    Name = $"Plant {plantCount}";
                }
            });
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

        private PlantDbModel MapViewModelToDBModel(bool isExistingPlant)
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
                FertilizeFrequencyInHours = FertilizationFrequencyInHours,

                Notes = Notes,
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

        private void MapToViewModel(Plant plant)
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

            Notes = plant.Notes;
        }

        public void Dispose()
        {
            ErrorsChanged -= ViewModel_ErrorsChanged;
        }
    }
}

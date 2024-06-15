using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PlantCare.App.Messaging;
using PlantCare.App.Services;
using PlantCare.App.Services.DBService;
using PlantCare.App.Utils;
using PlantCare.App.ViewModels.Base;
using PlantCare.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace PlantCare.App.ViewModels
{
    public partial class SinglePlantCareHistoryViewModel : ViewModelBase, IQueryAttributable
    {
        private readonly IDialogService _dialogService;
        private readonly IPlantService _plantService;

        public SinglePlantCareHistoryViewModel(
            IDialogService dialogService,
            IPlantService plantService
            )
        {
            _dialogService = dialogService;
            _plantService = plantService;
        }

        [ObservableProperty]
        private CareType? _plantCareType = null;

        [ObservableProperty]
        private string _plantName = string.Empty;

        //[ObservableProperty]
        public ObservableCollection<TimeStampRecord> TimestampRecords { get; } = [];

        async void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
        {
            try
            {
                IsLoading = true;

                PlantCareType = null;
                PlantName = string.Empty;
                TimestampRecords.Clear();

                if (!query.TryGetValue("careType", out object? careTypeObject))
                    return;
                if (!query.TryGetValue("plantName", out object? plantNameObject))
                    return;
                if (!query.TryGetValue("records", out object? recordsObject))
                    return;

                if (careTypeObject != null && careTypeObject is CareType careType)
                {
                    PlantCareType = careType;
                }
                if (plantNameObject is string plantName)
                {
                    PlantName = plantName;
                }
                if (recordsObject != null && recordsObject is List<TimeStampRecord> records)
                {
                    foreach (TimeStampRecord record in records)
                    {
                        TimestampRecords.Add(record);
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
            }
        }

        [RelayCommand(CanExecute = nameof(CanDeleteRecord))]
        public async Task DeleteRecord(object? target)
        {
            if (target == null || target is not TimeStampRecord record) return;

            IsBusy = true;
            try
            {
                //Debug.WriteLine($"{record.Timestamp}, {record.HistoryId}");
                bool isConfirmed = await _dialogService.Ask(
                                        LocalizationManager.Instance[ConstStrings.Confirm] ?? "Confirm",
                                        $"{LocalizationManager.Instance[ConstStrings.ConfirmDelete]}: {record.Timestamp}?",
                                        LocalizationManager.Instance[ConstStrings.Yes] ?? "Yes",
                                        LocalizationManager.Instance[ConstStrings.No] ?? "No");

                if (!isConfirmed)
                {
                    return;
                }

                if (PlantCareType is CareType careType)
                {
                    try
                    {
                        switch (careType)
                        {
                            case CareType.Watering:
                                await _plantService.DeleteWateringHistoryAsync(record.PlantId, record.HistoryId);

                                break;

                            case CareType.Fertilization:
                                await _plantService.DeleteFertilizationHistoryAsync(record.PlantId, record.HistoryId);
                                break;
                        }

                        WeakReferenceMessenger.Default.Send<PlantCareHistoryChangedMessage>(new PlantCareHistoryChangedMessage(record.PlantId, careType));
                        TimestampRecords.Remove(record);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.Notify(LocalizationManager.Instance[ConstStrings.Error] ?? ConstStrings.Error, ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanDeleteRecord()
        {
            if (TimestampRecords.Count == 1)
                return false;

            return true;
        }
    }
}
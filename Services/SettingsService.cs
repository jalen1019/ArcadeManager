using System.ComponentModel;
using System;
using System.Threading.Tasks;

using ArcadeManager.Core.Services;
using ArcadeManager.Helpers;

using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Foundation.Collections;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;

namespace ArcadeManager.Services
{
    public sealed class SettingsService : ObservableObject, ISettingsService
    {
        private readonly ApplicationDataContainer SettingsStorage = ApplicationData.Current.LocalSettings;
        private static readonly StorageItemAccessList FutureAccessList = StorageApplicationPermissions.FutureAccessList;

        private IAsyncRelayCommand _setSourceFolderCommand;
        public IAsyncRelayCommand SetSourceFolderCommand
        {
            get
            {
                return _setSourceFolderCommand ?? new AsyncRelayCommand(async () =>
                {
                    SourceFolderToken = await PickStorageFolderAsync() ?? SourceFolderToken;
                });
            }
        }

        private IAsyncRelayCommand _setDestinationFolderCommand;
        public IAsyncRelayCommand SetDestinationFolderCommand
        {
            get
            {
                return _setDestinationFolderCommand ?? new AsyncRelayCommand(async () =>
                {
                    DestinationFolderToken = await PickStorageFolderAsync() ?? DestinationFolderToken;
                });
            }
        }

        private IAsyncRelayCommand _resetLocalSettingsCommand;
        public IAsyncRelayCommand ResetLocalSettingsCommand
        {
            get
            {
                return _resetLocalSettingsCommand ?? new AsyncRelayCommand(async () =>
                {
                    await ResetLocalSettingsAsync();
                });
            }
        }

        private StorageFolder _sourceFolder;
        public StorageFolder SourceFolder
        {
            get => _sourceFolder;
            set => SetProperty(ref _sourceFolder, value);
        }

        private string _sourceFolderToken;
        public string SourceFolderToken
        {
            get => _sourceFolderToken;
            set => SetProperty(ref _sourceFolderToken, value);
        }

        private StorageFolder _destinationFolder;
        public StorageFolder DestinationFolder
        {
            get => _destinationFolder;
            set => SetProperty(ref _destinationFolder, value);
        }

        private string _destinationFolderToken;
        public string DestinationFolderToken
        {
            get => _destinationFolderToken;
            set => SetProperty(ref _destinationFolderToken, value);
        }

        public SettingsService()
        {
            PropertyChanged += Settings_PropertyChanged;
        }

        public async Task InitializeAsync()
        {
            SourceFolderToken = await ApplicationData.Current.LocalSettings.ReadAsync<string>("SourceFolderToken");
            DestinationFolderToken = await ApplicationData.Current.LocalSettings.ReadAsync<string>("DestinationFolderToken");

            if (!(SourceFolderToken is null))
            {
                SourceFolder = await FutureAccessList.GetFolderAsync(SourceFolderToken);
            }

            if (!(DestinationFolderToken is null))
            {
                DestinationFolder = await FutureAccessList.GetFolderAsync(DestinationFolderToken);
            }
        }

        public async Task SetSettingAsync<T>(string key, T value)
        {
            await SettingsStorage.SaveAsync(key, value);
        }

        public async Task<T> GetSettingAsync<T>(string key)
        {
            if (SettingsStorage.Values.ContainsKey(key))
                return await SettingsStorage.ReadAsync<T>(key);

            return default;
        }

        private async void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SourceFolderToken":
                    SourceFolder = await FutureAccessList.GetFolderAsync(_sourceFolderToken) ?? SourceFolder;
                    await SetSettingAsync("SourceFolderToken", _sourceFolderToken);
                    break;

                case "DestinationFolderToken":
                    DestinationFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(_destinationFolderToken);
                    await SettingsStorage.SaveAsync("DestinationFolderToken", _destinationFolderToken);
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine($"Property ({e.PropertyName}) changed.");
                    break;
            }
        }

        public async Task<string> PickStorageFolderAsync()
        {
            FolderPicker folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (!(folder is null))
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                string token = StorageApplicationPermissions.FutureAccessList.Add(folder);
                return token;
            }
            return default;
        }

        public async Task ResetLocalSettingsAsync()
        {
            await ApplicationData.Current.ClearAsync();
            StorageApplicationPermissions.FutureAccessList.Clear();
            StorageApplicationPermissions.MostRecentlyUsedList.Clear();
        }
    }
}

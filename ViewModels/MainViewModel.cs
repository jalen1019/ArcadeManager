using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

using ArcadeManager.Services;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml.Controls;

namespace ArcadeManager.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private SettingsService _settings = new SettingsService();
        public SettingsService Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        private List<StorageFile> _sourceFolderItems;
        public List<StorageFile> SourceFolderItems
        {
            get => _sourceFolderItems;
            set => SetProperty(ref _sourceFolderItems, value);
        }

        private List<StorageFile> _destinationFolderItems;
        public List<StorageFile> DestinationFolderItems
        {
            get => _destinationFolderItems;
            set => SetProperty(ref _destinationFolderItems, value);
        }

        private IAsyncRelayCommand _autoSuggestBoxTextChangedCommand;
        public IAsyncRelayCommand AutoSuggestBoxTextChangedCommand
        {
            get
            {
                return _autoSuggestBoxTextChangedCommand ?? new AsyncRelayCommand( async () =>
                {
                    await Task.CompletedTask;
                });
            }
            set
            {
                SetProperty(ref _autoSuggestBoxTextChangedCommand, value);
            }
        }
        

        public MainViewModel()
        {
            Settings.PropertyChanged += Settings_PropertyChanged;
            PropertyChanged += MainViewModel_PropertyChanged;
        }

        private void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Debug.WriteLine(e.PropertyName + "changed.");

            switch (e.PropertyName)
            {
                case "SourceFolderItems":
                    break;

                case "DestinationFolderItems":
                    break;

                default:

                    break;
            }
        }

        private async void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SourceFolder":
                    SourceFolderItems = new List<StorageFile>(await Settings.SourceFolder.GetFilesAsync());
                    break;

                case "DestinationFolder":
                    DestinationFolderItems = new List<StorageFile>(await Settings.DestinationFolder.GetFilesAsync(CommonFileQuery.OrderByName));
                    break;

                default:
                    break;
            }
        }

        public async Task InitializeAsync()
        {
            await Settings.InitializeAsync();
        }
    }
}

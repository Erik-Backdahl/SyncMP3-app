using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.VisualBasic;

namespace AvaloniaTest
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;

            PresentMessagesAndServerConnection(this, new RoutedEventArgs());
        }

        private async void SelectFolder_Click(object? sender, RoutedEventArgs e)
        {
            var folder = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Music Folder",
                AllowMultiple = false
            });

            if (folder != null && folder.Count > 0)
            {
                var folderPath = folder[0].Path.LocalPath;
                _viewModel.LoadMusicFiles(folderPath);
            }
        }
        private void Button_SongPrevious(object sender, RoutedEventArgs e)
        {
            // Implement previous logic
            if (_viewModel.SelectedMusic != null)
            {
                var currentIndex = _viewModel.MusicFiles.IndexOf(_viewModel.SelectedMusic);
                if (currentIndex > 0)
                {
                    _viewModel.SelectedMusic = _viewModel.MusicFiles[currentIndex - 1];
                }
                else
                {
                    _viewModel.SelectedMusic = _viewModel.MusicFiles[_viewModel.MusicFiles.Count - 1];
                }
            }
        }
        private void Button_SongNext(object sender, RoutedEventArgs e)
        {
            // Implement next logic
            if (_viewModel.SelectedMusic != null)
            {
                var currentIndex = _viewModel.MusicFiles.IndexOf(_viewModel.SelectedMusic);
                if (currentIndex < _viewModel.MusicFiles.Count - 1 && currentIndex >= 0)
                {
                    _viewModel.SelectedMusic = _viewModel.MusicFiles[currentIndex + 1];
                }
                if (currentIndex == _viewModel.MusicFiles.Count - 1)
                {
                    _viewModel.SelectedMusic = _viewModel.MusicFiles[0];
                }
            }
        }

        private void Button_SongPlayPause(object sender, RoutedEventArgs e)
        {
            _viewModel.PlayPause();
        }
        private void Button_SongRandom(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Random Song");
        }
        private async void Button_Create(object sender, RoutedEventArgs e)
        {
            var (passKey, message) = await EndPoints.CreateDataBase();

            if (message != null && message.StartsWith("Error"))
            {
                DisplayResponse.Text = message;
            }
            else
            {
                string final = $"password: {passKey}" + "\n" + "use the password on other devices to add them to your network(expires in 30min. must make a new password after)";
                DisplayResponse.Text = final;
            }
        }
        private async void Button_SendSQLData(object sender, RoutedEventArgs e)
        {
            string? message = await EndPoints.SendSQLToServer();

            DisplayResponse.Text = message;
        }
        private async void Button_RequestNewPasskey(object sender, RoutedEventArgs e)
        {
            var (message, code) = await EndPoints.RequestNewPassKey();

            DisplayResponse.Text = message + "\n" + code;
        }
        private async void Button_OnReadDigits(object sender, RoutedEventArgs e)
        {
            string passKey = "";
            if (InputBoxCode.Text != null)
            {
                passKey = InputBoxCode.Text;
                string message = await EndPoints.TryJoinNetWork(passKey);

                DisplayResponse.Text = message;
            }
            else
            {
                DisplayResponse.Text = "No Input";
                return;
            }
        }
        private async void PresentMessagesAndServerConnection(object sender, RoutedEventArgs e)
        {
            string result = await StartUpAction.TrySendPing();

            string final = "";
            string compareResult = "";
            if (result.StartsWith("connected"))
            {
                compareResult = await EndPoints.SendSQLToServer();
            }

            final = compareResult + "\r" + result;

            DisplayResponse.Text = final;


            //EndPoints.DeleteOldMessages
        }
    }
}
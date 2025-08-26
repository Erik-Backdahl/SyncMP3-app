using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using DynamicData.Kernel;

namespace AvaloniaTest
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel viewModel = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.LoadMusicFiles(ModifyAppSettings.ReadRegisteredMusicFolders());
            UpdateMusic_click(this, new RoutedEventArgs());
            Button_Ping(this, new RoutedEventArgs());
        }
        private void UpdateMusic_click(object? sender, RoutedEventArgs e)
        {
            try
            {
                FolderButton.IsEnabled = false;
                UpdateButton.IsEnabled = false;

                viewModel.LoadMusicFiles(ModifyAppSettings.ReadRegisteredMusicFolders());
            }
            catch (Exception ex)
            {
                DisplayResponse.Text = ex.Message;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                FolderButton.IsEnabled = true;
                UpdateButton.IsEnabled = true;
            }
        }
        private async void AddFolder_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                FolderButton.IsEnabled = false;
                UpdateButton.IsEnabled = false;

                string selectedFolder = "";
                var select = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select Music Folder",
                    AllowMultiple = false
                });
                foreach (var folder in select)
                {
                    selectedFolder = folder.Path.AbsolutePath;
                }

                if (selectedFolder != null)
                {
                    ModifyAppSettings.AddRegisteredFolder(selectedFolder);
                }

                //Update currentAlbum
            }
            catch (Exception ex)
            {
                DisplayResponse.Text = ex.Message;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                FolderButton.IsEnabled = true;
                UpdateButton.IsEnabled = true;
            }
        }
        private void Button_SongPrevious(object sender, RoutedEventArgs e)
        {
            // Implement previous logic
            if (viewModel.SelectedMusic != null)
            {
                var currentIndex = viewModel.MusicFiles.IndexOf(viewModel.SelectedMusic);
                if (currentIndex > 0)
                {
                    viewModel.SelectedMusic = viewModel.MusicFiles[currentIndex - 1];
                }
                else
                {
                    viewModel.SelectedMusic = viewModel.MusicFiles[viewModel.MusicFiles.Count - 1];
                }
            }
        }
        private void Button_SongNext(object sender, RoutedEventArgs e)
        {
            // Implement next logic
            if (viewModel.SelectedMusic != null)
            {
                var currentIndex = viewModel.MusicFiles.IndexOf(viewModel.SelectedMusic);
                if (currentIndex < viewModel.MusicFiles.Count - 1 && currentIndex >= 0)
                {
                    viewModel.SelectedMusic = viewModel.MusicFiles[currentIndex + 1];
                }
                if (currentIndex == viewModel.MusicFiles.Count - 1)
                {
                    viewModel.SelectedMusic = viewModel.MusicFiles[0];
                }
            }
        }

        private void Button_SongPlayPause(object sender, RoutedEventArgs e)
        {
            viewModel.PlayPause();
        }
        private void Button_SongRandom(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Random Song");
        }
        private async void Button_Create(object sender, RoutedEventArgs e)
        {
            try
            {
                DisableOtherButtons();

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
            catch (Exception ex)
            {
                DisplayResponse.Text = ex.Message;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                EnableOtherButtons();
            }
        }
        private async void Button_SendSQLData(object sender, RoutedEventArgs e)
        {
            try
            {
                DisableOtherButtons();

                string? message = await EndPoints.SendSQLToServer();

                DisplayResponse.Text = message;
            }
            catch (Exception ex)
            {
                DisplayResponse.Text = ex.Message;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                EnableOtherButtons();
            }
        }
        private async void Button_RequestNewPasskey(object sender, RoutedEventArgs e)
        {
            try
            {
                DisableOtherButtons();

                var (message, code) = await EndPoints.RequestNewPassKey();

                DisplayResponse.Text = message + "\n" + code;
            }
            catch (Exception ex)
            {
                DisplayResponse.Text = ex.Message;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                EnableOtherButtons();
            }
        }
        private async void Button_OnReadDigits(object sender, RoutedEventArgs e)
        {
            try
            {
                DisableOtherButtons();

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
            catch (Exception ex)
            {
                DisplayResponse.Text = ex.Message;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                EnableOtherButtons();
            }
        }
        private async void Button_Ping(object sender, RoutedEventArgs e)
        {
            try
            {
                DisableOtherButtons();

                string result = await EndPoints.TrySendPing();

                string final = "";
                string compareResult = "";
                if (result.StartsWith("connected"))
                {
                    compareResult = await EndPoints.SendSQLToServer();
                }

                final = compareResult + "\r" + result;
                DisplayResponse.Text = final;

            }
            catch (Exception ex)
            {
                DisplayResponse.Text = ex.Message;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                EnableOtherButtons();
            }
        }
        private void DisableOtherButtons()
        {
            SendSQLData.IsEnabled = false;
            Ping.IsEnabled = false;
            Create.IsEnabled = false;
            RequestNewPasskey.IsEnabled = false;
            OnReadDigits.IsEnabled = false;
        }
        private void EnableOtherButtons()
        {
            SendSQLData.IsEnabled = true;
            Ping.IsEnabled = true;
            Create.IsEnabled = true;
            RequestNewPasskey.IsEnabled = true;
            OnReadDigits.IsEnabled = true;
        }
    }
}
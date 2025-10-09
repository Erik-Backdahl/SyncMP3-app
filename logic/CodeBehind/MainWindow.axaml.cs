using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using DynamicData.Kernel;
using TagLib.Id3v2;
using System.Net;
using AvaloniaTest;
using System.Collections.Generic;

namespace SyncMP3App
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel viewModel = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;

            StartUpAction();
        }
        private void StartUpAction()
        {
            try
            {
                CreateEssentialFiles.CheckEssentialFiles();

                UpdateMusic_click(null, new RoutedEventArgs());
                Button_SendSQLData(null, new RoutedEventArgs());
            }
            catch (Exception ex)
            {
                DisplayResponse.Text = ex.Message;
                Console.WriteLine(ex.Message);
            }
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
        private void Button_Clicker(object sender, RoutedEventArgs e)
        {

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
                if (!string.IsNullOrEmpty(InputBoxCode.Text))
                {
                    passKey = InputBoxCode.Text;
                    string message = await EndPoints.TryJoinNetwork(passKey);

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
        private async void Button_SendSQLData(object? sender, RoutedEventArgs e)
        {
            try
            {
                DisableOtherButtons();

                var (connectionResult, pingHeaders) = await EndPoints.TrySendPing();


                if (connectionResult.StartsWith("connected"))
                {
                    if (pingHeaders != null)
                    {
                        List<string> textMessages = [];
                        List<string> requestedSongsUUID = [];
                        foreach (var serverMessage in pingHeaders)
                        {
                            string[] splitMessage = serverMessage.Value.ToString().Split(" ");

                            string messageType = splitMessage[0];
                            if (messageType == "uploadRequest")
                            {
                                requestedSongsUUID.Add(splitMessage[1]);
                            }
                            else // i can use else here because the message type can only be uploadRequest and message
                            {//this is made sure on the server
                                textMessages.Add(splitMessage[1]);
                            }
                        }
                        if (requestedSongsUUID.Count > 0)
                            await EndPoints.SendMusicToServer(requestedSongsUUID);
                    }

                    var (headers, message) = await EndPoints.SendSQLToServer();

                    int successfulDownloads = 0;
                    int unSuccessfulDownloads = 0;


                    if (headers.TryGetValue("X-NewSongs", out string? newSongs) && int.TryParse(newSongs, out int ammountNewSongs))
                    {
                        if (ammountNewSongs > 0)
                            message += $"\t {ammountNewSongs} Songs requested from server";
                        for (int i = 1; i < ammountNewSongs + 1; i++)
                        {
                            headers.TryGetValue($"X-Song{i}", out string? songID);
                            if (songID != null)
                            {
                                var (succes, resultMessage) = await EndPoints.RequestAndReceiveMusic(songID);
                                Console.WriteLine(resultMessage);
                                if (succes)
                                {
                                    successfulDownloads++;
                                }
                                else
                                {
                                    unSuccessfulDownloads++;
                                }

                            }
                        }
                    }
                    if (successfulDownloads > 0)
                    {
                        message += $"{successfulDownloads} Song(s) downloaded successfully";
                    }
                    if (unSuccessfulDownloads != 0)
                    {
                        message += $"\n {unSuccessfulDownloads} song(s) failed to download but have been requested";
                    }
                    DisplayResponse.Text = message;
                }

                DisplayResponse2.Text = connectionResult;

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
            Create.IsEnabled = false;
            RequestNewPasskey.IsEnabled = false;
            OnReadDigits.IsEnabled = false;
        }
        private void EnableOtherButtons()
        {
            SendSQLData.IsEnabled = true;
            Create.IsEnabled = true;
            RequestNewPasskey.IsEnabled = true;
            OnReadDigits.IsEnabled = true;
        }
    }
}
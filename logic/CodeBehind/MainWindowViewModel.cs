using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Rendering.Composition;
using AvaloniaTest.Services;
using ReactiveUI;

namespace AvaloniaTest
{
    public class MainWindowViewModel : ReactiveObject
    {
        public ObservableCollection<string> MusicFiles { get; } = new();
        public Dictionary<string, string> MusicFolder { get; } = new();
        private readonly AudioService _audioService;
        private string? _selectedMusic;

        public string? SelectedMusic
        {
            get => _selectedMusic;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedMusic, value);
                if (value != null)
                {
                    _ = PlaySelectedMusicAsync(value);
                }
            }
        }

        private async System.Threading.Tasks.Task PlaySelectedMusicAsync(string songName)
        {
            MusicFolder.TryGetValue(songName, out string? folderName);
            if (folderName == null)
                throw new Exception("folderName of selected song not found, song name likely changed");

            await _audioService.PlayAsync(folderName, songName);
        }

        public MainWindowViewModel()
        {
            _audioService = new AudioService();
        }

        public void PlayPause()
        {
            if (AudioService.isPlaying)
            {
                _audioService.Pause();
            }
            else
            {
                _audioService.Resume();
            }

        }
        public void Stop()
        {
            _audioService.Stop();
        }

        public void LoadMusicFiles(string[] folderPath)
        {
            try
            {

                var supportedExtensions = new[] { ".mp3", ".wav", ".flac", ".ogg", ".aac" };

                List<string> allFiles = new List<string>();

                for (int i = 0; i < folderPath.Length; i++)
                {
                    if (Directory.Exists(folderPath[i]))
                    {
                        var files = Directory
                            .EnumerateFiles(folderPath[i])
                            .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLower()))
                            .ToList();

                        // Add these files to the overall list
                        allFiles.AddRange(files);
                    }
                    else
                    {
                        Console.WriteLine(folderPath[i] + " failed to load");
                    }
                }

                // Now, you can use 'allFiles' outside the loop
                MusicFiles.Clear();
                MusicFolder.Clear();

                foreach (var file in allFiles)
                {
                    MusicFiles.Add(Path.GetFileName(file));
                    MusicFolder.Add(Path.GetFileName(file), Path.GetDirectoryName(file) ?? string.Empty);
                }
            }
            catch
            {
                
            }
        }
    }
}
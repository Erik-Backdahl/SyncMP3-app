using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
            //if not found try to reload music because the most likely cause is the song has been changed

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

        public void LoadMusicFiles(string folderPath)
        {
            var supportedExtensions = new[] { ".mp3", ".wav", ".flac", ".ogg", ".aac" };

            if (Directory.Exists(folderPath))
            {
                var files = Directory
                    .EnumerateFiles(folderPath)
                    .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLower()));

                MusicFiles.Clear();
                foreach (var file in files)
                {
                    MusicFiles.Add(Path.GetFileName(file));
                    MusicFolder.Add(Path.GetFileName(file), Path.GetDirectoryName(file) ?? string.Empty);
                }
            }
        }
    }
}
using NAudio.Wave;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace AvaloniaTest.Services
{
    public class AudioService : IDisposable
    {
        private IWavePlayer waveOutDevice;
        private AudioFileReader? audioFileReader;
        private string? oldSong;
        public static bool isPlaying;
        private bool isProcessing;

        public AudioService()
        {
            waveOutDevice = new WaveOutEvent();
            waveOutDevice.PlaybackStopped += OnPlaybackStopped;
        }

        public async Task PlayAsync(string folderName, string songName)
        {
            if (isProcessing) return;

            try
            {
                isProcessing = true;
                await Task.Run(() =>
                {
                    if (isPlaying)
                    {
                        Stop();  // This now includes a small delay
                        Thread.Sleep(50);
                    }

                    if (oldSong != songName)
                    {
                        waveOutDevice.Dispose();  // Dispose old device
                        waveOutDevice = new WaveOutEvent();  // Create new device
                        waveOutDevice.PlaybackStopped += OnPlaybackStopped;
                        
                        audioFileReader?.Dispose();
                        audioFileReader = new AudioFileReader(folderName + "\\" + songName);
                        waveOutDevice.Init(audioFileReader);
                        oldSong = songName;
                    }

                    waveOutDevice.Play();
                    isPlaying = true;
                });
            }
            finally
            {
                isProcessing = false;
            }
        }

        public void Pause()
        {
            if (isPlaying)
            {
                waveOutDevice.Pause();
                isPlaying = false;
            }
        }

        public void Resume()
        {
            if (audioFileReader == null)
                return;

            if (!isPlaying)
            {

                waveOutDevice.Play();
                isPlaying = true;
            }
        }

        public void Stop()
        {
            waveOutDevice.Stop();
            isPlaying = false;
            // Wait a small amount of time to ensure the device has stopped
            if (audioFileReader != null)
            {
                audioFileReader.Position = 0;
            }
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs args)
        {
            isPlaying = false;
        }

        public void Dispose()
        {
            waveOutDevice?.Dispose();
            audioFileReader?.Dispose();
        }
    }
}
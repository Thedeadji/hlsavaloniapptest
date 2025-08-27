using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.IO;
using LibVLCSharp.Shared;

namespace HLSTestApp;

public partial class MainWindow : Window
{
    private LibVLC? _libVLC;
    private MediaPlayer? _mediaPlayer;
    private Media? _currentMedia;
    private bool _isPlaying = false;

    public MainWindow()
    {
        InitializeComponent();
        InitializeVLC();
        SetupEventHandlers();
    }

    private void InitializeVLC()
    {
        try
        {
            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            _mediaPlayer.EncounteredError += (_, _) =>
            {
                StatusText.Text = "Playback error";
            };
            StatusText.Text = "VLC initialized successfully";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"VLC initialization failed: {ex.Message}";
        }
    }

    private void SetupEventHandlers()
    {
        // Ensure VideoView has a native handle before binding MediaPlayer to it
        VideoView.AttachedToVisualTree += (_, _) =>
        {
            if (VideoView.MediaPlayer != _mediaPlayer)
            {
                VideoView.MediaPlayer = _mediaPlayer;
            }
        };

        this.AttachedToVisualTree += (_, _) =>
        {
            LoadDefaultHLSStream();
        };
    }

    private void LoadDefaultHLSStream()
    {
        try
        {
            StatusText.Text = "Ready";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error preparing player: {ex.Message}";
        }
    }

    private void OnPlayClick(object? sender, RoutedEventArgs e)
    {
        var streamType = StreamTypeCombo.SelectedIndex;
        var streamUrl = StreamUrlBox.Text;

        if (string.IsNullOrEmpty(streamUrl))
        {
            StatusText.Text = "Please enter a stream URL";
            return;
        }

        if (streamType == 0) // HLS
        {
            PlayHLSStream(streamUrl);
        }
        else // RTMP
        {
            PlayRTMPStream(streamUrl);
        }
    }

    private void PlayHLSStream(string url)
    {
        try
        {
            if (_mediaPlayer == null || _libVLC == null)
            {
                StatusText.Text = "VLC not initialized";
                return;
            }

            _currentMedia?.Dispose();
            _currentMedia = new Media(_libVLC, url, FromType.FromLocation,
                ":network-caching=1000",
                ":live-caching=1000");

            _mediaPlayer.Play(_currentMedia);
            
            StatusText.Text = $"Playing HLS stream: {url}";
            _isPlaying = true;
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error playing HLS stream: {ex.Message}";
        }
    }

    private void PlayRTMPStream(string url)
    {
        try
        {
            if (_mediaPlayer == null || _libVLC == null)
            {
                StatusText.Text = "VLC not initialized";
                return;
            }

            _currentMedia?.Dispose();
            _currentMedia = new Media(_libVLC, url, FromType.FromLocation,
                ":network-caching=1000");

            _mediaPlayer.Play(_currentMedia);
            
            StatusText.Text = $"Playing RTMP stream: {url}";
            _isPlaying = true;
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error playing RTMP stream: {ex.Message}";
        }
    }

    private void OnStopClick(object? sender, RoutedEventArgs e)
    {
        if (_isPlaying)
        {
            _mediaPlayer?.Stop();
            _currentMedia?.Dispose();
            _currentMedia = null;
            _isPlaying = false;
            StatusText.Text = "Stream stopped";
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        VideoView.MediaPlayer = null;
        _mediaPlayer?.Dispose();
        _currentMedia?.Dispose();
        _libVLC?.Dispose();
        base.OnUnloaded(e);
    }
}

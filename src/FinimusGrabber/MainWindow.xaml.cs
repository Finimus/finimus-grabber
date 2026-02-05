using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FinimusGrabber.Models;
using FinimusGrabber.Services;
using Microsoft.Win32;

namespace FinimusGrabber;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly AudioLoader _audioLoader;
    private readonly AnalysisEngine _analysisEngine;
    private readonly MidiExporter _midiExporter;
    private readonly NoiseSuppressionService _noiseSuppressionService;
    private readonly MusicSeparationService _musicSeparationService;
    
    private AudioData? _currentAudio;
    private AudioData? _originalAudio; // Store original for reset
    private List<Note> _detectedNotes = new();
    private bool _isAutomaticMode = true;
    private List<Note> _manualNotes = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        _audioLoader = new AudioLoader();
        _analysisEngine = new AnalysisEngine();
        _midiExporter = new MidiExporter();
        _noiseSuppressionService = new NoiseSuppressionService();
        _musicSeparationService = new MusicSeparationService();

        UpdateUI();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool HasAudio => _currentAudio != null;
    public bool HasNotes => _detectedNotes.Count > 0 || _manualNotes.Count > 0;
    public Visibility EmptyStateVisibility => _currentAudio == null ? Visibility.Visible : Visibility.Collapsed;

    private string _statusMessage = "Ready";
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged(nameof(StatusMessage));
        }
    }

    public string FileInfo
    {
        get
        {
            if (_currentAudio == null) return "No file loaded";
            return $"{_currentAudio.FileName} | {_currentAudio.SampleRate}Hz | {_currentAudio.DurationSeconds:F2}s";
        }
    }

    public string NotesInfo
    {
        get
        {
            var noteCount = _isAutomaticMode ? _detectedNotes.Count : _manualNotes.Count;
            return noteCount > 0 ? $"{noteCount} notes detected" : "No notes";
        }
    }

    private async void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Audio Files (*.mp3;*.wav;*.flac)|*.mp3;*.wav;*.flac|All Files (*.*)|*.*",
            Title = "Select an Audio File"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                StatusMessage = "Loading audio...";
                _currentAudio = await Task.Run(() => _audioLoader.LoadFile(openFileDialog.FileName));
                _originalAudio = CloneAudioData(_currentAudio); // Save original
                
                DrawWaveform();
                
                if (_isAutomaticMode)
                {
                    await AnalyzeAudioAsync();
                }
                else
                {
                    StatusMessage = "Audio loaded. Use manual mode to select notes.";
                }
                
                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Error loading file";
            }
        }
    }

    private async void Analyze_Click(object sender, RoutedEventArgs e)
    {
        if (_currentAudio == null) return;
        await AnalyzeAudioAsync();
    }

    private async Task AnalyzeAudioAsync()
    {
        if (_currentAudio == null) return;

        try
        {
            StatusMessage = "Analyzing audio...";
            AnalyzeButton.IsEnabled = false;

            _detectedNotes = await Task.Run(() => _analysisEngine.Analyze(_currentAudio));
            
            DrawPianoRoll();
            StatusMessage = $"Analysis complete! {_detectedNotes.Count} notes detected.";
            UpdateUI();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error analyzing audio: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Error during analysis";
        }
        finally
        {
            AnalyzeButton.IsEnabled = true;
        }
    }

    private void ExportMidi_Click(object sender, RoutedEventArgs e)
    {
        var notes = _isAutomaticMode ? _detectedNotes : _manualNotes;
        if (notes.Count == 0)
        {
            MessageBox.Show("No notes to export!", "Export MIDI", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var saveFileDialog = new SaveFileDialog
        {
            Filter = "MIDI Files (*.mid)|*.mid|All Files (*.*)|*.*",
            Title = "Export MIDI File",
            DefaultExt = ".mid"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                _midiExporter.ExportToFile(notes, saveFileDialog.FileName);
                MessageBox.Show($"MIDI file exported successfully!\n{notes.Count} notes exported.", 
                    "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                StatusMessage = "MIDI exported successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting MIDI: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void AutomaticMode_Checked(object sender, RoutedEventArgs e)
    {
        _isAutomaticMode = true;
        StatusMessage = "Automatic Mode: AI-powered note detection";
        if (_currentAudio != null)
        {
            DrawPianoRoll();
            UpdateUI();
        }
    }

    private void ManualMode_Checked(object sender, RoutedEventArgs e)
    {
        _isAutomaticMode = false;
        StatusMessage = "Manual Mode: Click on waveform to add notes";
        DrawPianoRoll();
        UpdateUI();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        var aboutMessage = @"Finimus Grabber
Version 2.0 - AI Edition

Created by Finimus

Musician (violin, piano, guitar) and technologist combining musical creativity with innovation.

This application makes music analysis easier and more intelligent through:
• Automatic note detection using AI-powered analysis
• Manual editing with visual feedback and precision control
• AI audio processing (vocal/drum isolation, noise suppression)
• Music stem separation inspired by OpenVINO AI Plugins
• Professional MIDI export for use in any DAW

New AI Features:
🎤 Isolate Vocals - Extract vocal tracks
🥁 Isolate Drums - Separate percussion
🔇 Remove Noise - Spectral noise suppression

GitHub: https://github.com/Finimus

Designed for beginners to professional producers.";

        MessageBox.Show(aboutMessage, "About Finimus Grabber", 
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void DrawWaveform()
    {
        if (_currentAudio == null) return;

        WaveformCanvas.Children.Clear();
        var width = WaveformCanvas.ActualWidth > 0 ? WaveformCanvas.ActualWidth : 1000;
        var height = WaveformCanvas.ActualHeight > 0 ? WaveformCanvas.ActualHeight : 300;

        // Downsample for display
        var monoSamples = _audioLoader.ConvertToMono(_currentAudio.Samples, _currentAudio.Channels);
        var samplesPerPixel = Math.Max(1, monoSamples.Length / (int)width);

        var polyline = new Polyline
        {
            Stroke = Brushes.DodgerBlue,
            StrokeThickness = 1
        };

        for (int x = 0; x < width && x * samplesPerPixel < monoSamples.Length; x++)
        {
            var sampleIndex = x * samplesPerPixel;
            var sample = monoSamples[sampleIndex];
            var y = height / 2 - (sample * height / 2);
            polyline.Points.Add(new System.Windows.Point(x, y));
        }

        WaveformCanvas.Children.Add(polyline);
    }

    private void DrawPianoRoll()
    {
        PianoRollCanvas.Children.Clear();
        
        var notes = _isAutomaticMode ? _detectedNotes : _manualNotes;
        if (notes.Count == 0 || _currentAudio == null) return;

        var width = 1000.0;
        var height = 400.0;
        var pixelsPerSecond = width / _currentAudio.DurationSeconds;
        var noteHeight = 5.0;

        // Draw grid lines
        for (int i = 0; i <= 127; i += 12)
        {
            var y = height - (i * noteHeight);
            var line = new Line
            {
                X1 = 0,
                Y1 = y,
                X2 = width,
                Y2 = y,
                Stroke = Brushes.LightGray,
                StrokeThickness = 0.5
            };
            PianoRollCanvas.Children.Add(line);
        }

        // Draw notes
        foreach (var note in notes)
        {
            var x = note.TimeSeconds * pixelsPerSecond;
            var noteWidth = note.DurationSeconds * pixelsPerSecond;
            var y = height - (note.MidiNote * noteHeight);

            var rect = new Rectangle
            {
                Width = Math.Max(2, noteWidth),
                Height = noteHeight - 1,
                Fill = new SolidColorBrush(Color.FromRgb(100, 150, 255)),
                Stroke = Brushes.DarkBlue,
                StrokeThickness = 0.5
            };

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            PianoRollCanvas.Children.Add(rect);

            // Add note name label for longer notes
            if (noteWidth > 30)
            {
                var label = new TextBlock
                {
                    Text = note.NoteName,
                    FontSize = 8,
                    Foreground = Brushes.White
                };
                Canvas.SetLeft(label, x + 2);
                Canvas.SetTop(label, y);
                PianoRollCanvas.Children.Add(label);
            }
        }
    }

    private void UpdateUI()
    {
        OnPropertyChanged(nameof(HasAudio));
        OnPropertyChanged(nameof(HasNotes));
        OnPropertyChanged(nameof(EmptyStateVisibility));
        OnPropertyChanged(nameof(FileInfo));
        OnPropertyChanged(nameof(NotesInfo));
    }

    // AI Processing Methods

    private async void IsolateVocals_Click(object sender, RoutedEventArgs e)
    {
        await ProcessWithAI("vocals", "Isolating vocals...");
    }

    private async void IsolateDrums_Click(object sender, RoutedEventArgs e)
    {
        await ProcessWithAI("drums", "Isolating drums...");
    }

    private async void RemoveNoise_Click(object sender, RoutedEventArgs e)
    {
        await ProcessWithAI("denoise", "Removing noise...");
    }

    private void ResetAudio_Click(object sender, RoutedEventArgs e)
    {
        if (_originalAudio != null)
        {
            _currentAudio = CloneAudioData(_originalAudio);
            DrawWaveform();
            StatusMessage = "Audio reset to original";
            
            // Clear notes as they may not be valid anymore
            _detectedNotes.Clear();
            _manualNotes.Clear();
            DrawPianoRoll();
            UpdateUI();
        }
    }

    private async Task ProcessWithAI(string processType, string statusText)
    {
        if (_currentAudio == null) return;

        try
        {
            StatusMessage = statusText;
            var originalStatus = StatusMessage;
            
            // Disable all buttons during processing
            IsEnabled = false;

            AudioData processed;
            
            switch (processType)
            {
                case "vocals":
                    processed = await Task.Run(() => 
                        _musicSeparationService.ExtractStem(
                            _currentAudio, 
                            MusicSeparationService.StemType.Vocals,
                            progress => Dispatcher.Invoke(() => StatusMessage = $"{statusText} {progress:P0}")
                        ));
                    break;
                    
                case "drums":
                    processed = await Task.Run(() => 
                        _musicSeparationService.ExtractStem(
                            _currentAudio, 
                            MusicSeparationService.StemType.Drums,
                            progress => Dispatcher.Invoke(() => StatusMessage = $"{statusText} {progress:P0}")
                        ));
                    break;
                    
                case "denoise":
                    processed = await Task.Run(() => 
                        _noiseSuppressionService.SuppressNoise(
                            _currentAudio,
                            progress => Dispatcher.Invoke(() => StatusMessage = $"{statusText} {progress:P0}")
                        ));
                    break;
                    
                default:
                    throw new ArgumentException($"Unknown process type: {processType}");
            }

            _currentAudio = processed;
            DrawWaveform();
            
            // Clear notes as they may not be valid anymore
            _detectedNotes.Clear();
            _manualNotes.Clear();
            DrawPianoRoll();
            
            StatusMessage = processType switch
            {
                "vocals" => "Vocals isolated! Click Analyze to detect notes.",
                "drums" => "Drums isolated! Click Analyze to detect notes.",
                "denoise" => "Noise removed! Click Analyze to detect notes.",
                _ => "Processing complete!"
            };
            
            UpdateUI();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during AI processing: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Processing failed";
        }
        finally
        {
            IsEnabled = true;
        }
    }

    private AudioData CloneAudioData(AudioData source)
    {
        return new AudioData
        {
            Samples = (float[])source.Samples.Clone(),
            SampleRate = source.SampleRate,
            Channels = source.Channels,
            FilePath = source.FilePath
        };
    }
}

# Building and Running Finimus Grabber

## Prerequisites

- .NET 10.0 SDK or later
- Windows 10/11 (64-bit)

## Quick Start

### Build the Application

```powershell
dotnet build src/FinimusGrabber/FinimusGrabber.csproj --configuration Release
```

### Run the Application

```powershell
dotnet run --project src/FinimusGrabber/FinimusGrabber.csproj
```

Or run the executable directly:
```powershell
.\src\FinimusGrabber\bin\Release\net10.0-windows\FinimusGrabber.exe
```

## Using the Application

### Automatic Mode (Default)
1. Click **"Open File"** and select an audio file (MP3, WAV, or FLAC)
2. The app will automatically analyze and detect notes
3. View the waveform and detected notes in the piano roll
4. Click **"Export MIDI"** to save as a MIDI file

### Manual Mode
1. Click the **"Manual"** radio button in the toolbar
2. Load an audio file
3. View the waveform visualization
4. Click **"Analyze"** to process the audio
5. Notes will be displayed in the piano roll
6. Export to MIDI when ready

### AI Audio Processing

Enhance your audio before analysis using AI-powered tools:

**🎤 Isolate Vocals**
- Extracts vocal frequencies (80Hz-8kHz)
- Perfect for transcribing singing or melodies
- Reduces instrumental interference

**🥁 Isolate Drums**
- Emphasizes percussive elements
- Ideal for rhythm pattern detection
- Uses temporal enhancement algorithms

**🔇 Remove Noise**
- Suppresses background noise using spectral gating
- Analyzes the first 0.5 seconds to create noise profile
- Improves detection accuracy for clean signals

**↻ Reset**
- Restores the original audio file
- Clears any applied processing
- Useful for trying different AI tools

**Workflow Tip**: Try using "Remove Noise" first, then "Isolate Vocals" or "Isolate Drums" for best results!

## Features

✅ **Dual Mode Operation**
- Automatic: AI-powered instant detection
- Manual: Visual editing and fine-tuning

✅ **Audio Format Support**
- MP3 files
- WAV files (16/24/32-bit)
- FLAC files

✅ **AI-Powered Audio Processing** 🆕
- **Isolate Vocals**: Extract vocal tracks for better note detection
- **Isolate Drums**: Separate drum patterns for rhythm analysis
- **Noise Suppression**: Remove background noise using spectral gating
- **Stem Separation**: Frequency-based separation (vocals, drums, bass, other)
- **Reset to Original**: Restore original audio anytime

✅ **Visualization**
- Real-time waveform display
- Piano roll with note names
- Grid-based layout

✅ **MIDI Export**
- Standard MIDI Type 0 format
- Professional quality output
- Compatible with all DAWs

## Troubleshooting

### "No .NET SDKs were found"
Install the .NET SDK from: https://dotnet.microsoft.com/download

### Audio file won't load
- Ensure the file format is supported (MP3, WAV, or FLAC)
- Check that the file isn't corrupted
- Try converting to WAV format

### No notes detected
- Increase the audio file volume
- Try a cleaner audio source
- Adjust sensitivity in future versions

## Development

To build in Debug mode:
```powershell
dotnet build src/FinimusGrabber/FinimusGrabber.csproj --configuration Debug
```

To clean build artifacts:
```powershell
dotnet clean
```

## About

Created by Finimus - musician and technologist

GitHub: https://github.com/Finimus/finimus-grabber

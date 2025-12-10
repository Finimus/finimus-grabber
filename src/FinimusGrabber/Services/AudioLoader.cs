using NAudio.Wave;
using FinimusGrabber.Models;

namespace FinimusGrabber.Services
{
    /// <summary>
    /// Loads and decodes audio files (MP3, WAV, FLAC)
    /// </summary>
    public class AudioLoader
    {
        /// <summary>
        /// Load an audio file and return audio data
        /// </summary>
        public AudioData LoadFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Audio file not found: {filePath}");

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var supportedFormats = new[] { ".mp3", ".wav", ".flac" };

            if (!supportedFormats.Contains(extension))
                throw new NotSupportedException($"Unsupported audio format: {extension}");

            try
            {
                using var reader = CreateAudioReader(filePath);
                var samples = ReadAllSamples(reader);

                return new AudioData
                {
                    Samples = samples,
                    SampleRate = reader.WaveFormat.SampleRate,
                    Channels = reader.WaveFormat.Channels,
                    FilePath = filePath
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load audio file: {ex.Message}", ex);
            }
        }

        private AudioFileReader CreateAudioReader(string filePath)
        {
            return new AudioFileReader(filePath);
        }

        private float[] ReadAllSamples(AudioFileReader reader)
        {
            var sampleList = new List<float>();
            var buffer = new float[reader.WaveFormat.SampleRate];
            int samplesRead;

            while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < samplesRead; i++)
                {
                    sampleList.Add(buffer[i]);
                }
            }

            return sampleList.ToArray();
        }

        /// <summary>
        /// Convert stereo to mono by averaging channels
        /// </summary>
        public float[] ConvertToMono(float[] samples, int channels)
        {
            if (channels == 1)
                return samples;

            var monoSamples = new float[samples.Length / channels];
            for (int i = 0; i < monoSamples.Length; i++)
            {
                float sum = 0;
                for (int ch = 0; ch < channels; ch++)
                {
                    sum += samples[i * channels + ch];
                }
                monoSamples[i] = sum / channels;
            }

            return monoSamples;
        }
    }
}

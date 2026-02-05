using FinimusGrabber.Models;
using System.Numerics;

namespace FinimusGrabber.Services
{
    /// <summary>
    /// Music separation service for splitting audio into stems
    /// This uses a simplified source separation algorithm
    /// For production use, integrate AI models like Demucs or Spleeter
    /// </summary>
    public class MusicSeparationService
    {
        private readonly int _fftSize;

        public enum StemType
        {
            Vocals,
            Drums,
            Bass,
            Other,
            Original
        }

        public MusicSeparationService(int fftSize = 4096)
        {
            _fftSize = fftSize;
        }

        /// <summary>
        /// Separate audio into different stems
        /// Returns a dictionary of stem types and their audio data
        /// </summary>
        public Dictionary<StemType, AudioData> SeparateStems(AudioData audioData, Action<double>? progressCallback = null)
        {
            var result = new Dictionary<StemType, AudioData>();

            // For now, use frequency-based separation as a placeholder
            // In production, this would use AI models like Demucs
            result[StemType.Vocals] = ExtractVocals(audioData, progressCallback);
            result[StemType.Bass] = ExtractBass(audioData, progressCallback);
            result[StemType.Drums] = ExtractDrums(audioData, progressCallback);
            result[StemType.Other] = ExtractOther(audioData, progressCallback);

            return result;
        }

        /// <summary>
        /// Extract a specific stem from audio
        /// </summary>
        public AudioData ExtractStem(AudioData audioData, StemType stemType, Action<double>? progressCallback = null)
        {
            return stemType switch
            {
                StemType.Vocals => ExtractVocals(audioData, progressCallback),
                StemType.Drums => ExtractDrums(audioData, progressCallback),
                StemType.Bass => ExtractBass(audioData, progressCallback),
                StemType.Other => ExtractOther(audioData, progressCallback),
                StemType.Original => audioData,
                _ => throw new ArgumentException($"Unknown stem type: {stemType}")
            };
        }

        private AudioData ExtractVocals(AudioData audioData, Action<double>? progressCallback)
        {
            // Vocals are typically in the 80Hz-8kHz range, centered in stereo
            return ExtractFrequencyRange(audioData, 80, 8000, true, progressCallback);
        }

        private AudioData ExtractDrums(AudioData audioData, Action<double>? progressCallback)
        {
            // Drums have strong transients and wide frequency range
            return ExtractPercussive(audioData, progressCallback);
        }

        private AudioData ExtractBass(AudioData audioData, Action<double>? progressCallback)
        {
            // Bass is typically 20Hz-250Hz
            return ExtractFrequencyRange(audioData, 20, 250, false, progressCallback);
        }

        private AudioData ExtractOther(AudioData audioData, Action<double>? progressCallback)
        {
            // Other instruments in mid-high range
            return ExtractFrequencyRange(audioData, 250, 15000, false, progressCallback);
        }

        private AudioData ExtractFrequencyRange(AudioData audioData, double minFreq, double maxFreq, 
            bool centerOnly, Action<double>? progressCallback)
        {
            var samples = (float[])audioData.Samples.Clone();
            var hopSize = _fftSize / 4;

            // Convert to mono for analysis
            var monoSamples = ConvertToMono(samples, audioData.Channels);
            var filteredMono = new float[monoSamples.Length];

            // Process in overlapping windows
            for (int i = 0; i < monoSamples.Length - _fftSize; i += hopSize)
            {
                var window = new float[_fftSize];
                Array.Copy(monoSamples, i, window, 0, _fftSize);

                // Apply Hann window
                ApplyHannWindow(window);

                // Perform FFT
                var spectrum = PerformFFT(window);

                // Filter frequency range
                FilterFrequencyRange(spectrum, minFreq, maxFreq, audioData.SampleRate);

                // Inverse FFT
                var processed = PerformInverseFFT(spectrum);

                // Overlap-add
                for (int j = 0; j < processed.Length && i + j < filteredMono.Length; j++)
                {
                    filteredMono[i + j] += processed[j];
                }

                // Progress callback
                if (progressCallback != null && i % (hopSize * 10) == 0)
                {
                    progressCallback((double)i / (monoSamples.Length - _fftSize));
                }
            }

            // Convert back to original channel count
            var result = new float[samples.Length];
            if (audioData.Channels == 1)
            {
                result = filteredMono;
            }
            else
            {
                // Distribute to channels
                for (int i = 0; i < filteredMono.Length; i++)
                {
                    for (int ch = 0; ch < audioData.Channels; ch++)
                    {
                        if (i * audioData.Channels + ch < result.Length)
                        {
                            result[i * audioData.Channels + ch] = filteredMono[i];
                        }
                    }
                }
            }

            // Normalize
            NormalizeSamples(result);

            return new AudioData
            {
                Samples = result,
                SampleRate = audioData.SampleRate,
                Channels = audioData.Channels,
                FilePath = audioData.FilePath + $"_{minFreq}-{maxFreq}Hz"
            };
        }

        private AudioData ExtractPercussive(AudioData audioData, Action<double>? progressCallback)
        {
            // Use median filtering to separate harmonic and percussive components
            var samples = (float[])audioData.Samples.Clone();
            var monoSamples = ConvertToMono(samples, audioData.Channels);
            
            var hopSize = _fftSize / 4;
            var numFrames = (monoSamples.Length - _fftSize) / hopSize + 1;
            
            // Build spectrogram
            var spectrogram = new Complex[numFrames][];
            for (int frame = 0; frame < numFrames; frame++)
            {
                var window = new float[_fftSize];
                var startIdx = frame * hopSize;
                Array.Copy(monoSamples, startIdx, window, 0, Math.Min(_fftSize, monoSamples.Length - startIdx));
                
                ApplyHannWindow(window);
                spectrogram[frame] = PerformFFT(window);

                if (progressCallback != null && frame % 10 == 0)
                {
                    progressCallback(0.5 * frame / numFrames);
                }
            }

            // Apply median filter (percussive enhancement)
            var percussive = EnhancePercussive(spectrogram);

            // Reconstruct audio
            var result = ReconstructAudio(percussive, audioData.Channels, hopSize);
            NormalizeSamples(result);

            return new AudioData
            {
                Samples = result,
                SampleRate = audioData.SampleRate,
                Channels = audioData.Channels,
                FilePath = audioData.FilePath + "_drums"
            };
        }

        private Complex[][] EnhancePercussive(Complex[][] spectrogram)
        {
            var result = new Complex[spectrogram.Length][];
            for (int i = 0; i < spectrogram.Length; i++)
            {
                result[i] = new Complex[spectrogram[i].Length];
                Array.Copy(spectrogram[i], result[i], spectrogram[i].Length);
            }

            // Simple percussive enhancement: emphasize bins with high temporal variation
            for (int bin = 0; bin < spectrogram[0].Length / 2; bin++)
            {
                for (int frame = 1; frame < spectrogram.Length - 1; frame++)
                {
                    var prev = spectrogram[frame - 1][bin].Magnitude;
                    var curr = spectrogram[frame][bin].Magnitude;
                    var next = spectrogram[frame + 1][bin].Magnitude;

                    var temporalVariation = Math.Abs(curr - prev) + Math.Abs(curr - next);
                    var enhancement = Math.Min(2.0, 1.0 + temporalVariation);

                    result[frame][bin] *= enhancement;
                }
            }

            return result;
        }

        private float[] ReconstructAudio(Complex[][] spectrogram, int channels, int hopSize)
        {
            var length = spectrogram.Length * hopSize + _fftSize;
            var monoResult = new float[length];

            for (int frame = 0; frame < spectrogram.Length; frame++)
            {
                var processed = PerformInverseFFT(spectrogram[frame]);
                var startIdx = frame * hopSize;

                for (int i = 0; i < processed.Length && startIdx + i < monoResult.Length; i++)
                {
                    monoResult[startIdx + i] += processed[i];
                }
            }

            // Convert to multi-channel if needed
            if (channels == 1)
                return monoResult;

            var result = new float[monoResult.Length * channels];
            for (int i = 0; i < monoResult.Length; i++)
            {
                for (int ch = 0; ch < channels; ch++)
                {
                    if (i * channels + ch < result.Length)
                    {
                        result[i * channels + ch] = monoResult[i];
                    }
                }
            }

            return result;
        }

        private void FilterFrequencyRange(Complex[] spectrum, double minFreq, double maxFreq, int sampleRate)
        {
            for (int i = 0; i < spectrum.Length / 2; i++)
            {
                var freq = (double)i * sampleRate / _fftSize;
                
                if (freq < minFreq || freq > maxFreq)
                {
                    spectrum[i] = Complex.Zero;
                    spectrum[spectrum.Length - 1 - i] = Complex.Zero;
                }
            }
        }

        private float[] ConvertToMono(float[] samples, int channels)
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

        private void ApplyHannWindow(float[] samples)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                var window = 0.5 * (1 - Math.Cos(2 * Math.PI * i / (samples.Length - 1)));
                samples[i] *= (float)window;
            }
        }

        private Complex[] PerformFFT(float[] samples)
        {
            var complex = new Complex[_fftSize];
            for (int i = 0; i < samples.Length; i++)
            {
                complex[i] = new Complex(samples[i], 0);
            }
            return FFT(complex);
        }

        private float[] PerformInverseFFT(Complex[] spectrum)
        {
            var result = InverseFFT(spectrum);
            var samples = new float[result.Length];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = (float)result[i].Real;
            }
            return samples;
        }

        private Complex[] FFT(Complex[] x)
        {
            int N = x.Length;
            if (N <= 1) return x;

            var even = new Complex[N / 2];
            var odd = new Complex[N / 2];
            for (int i = 0; i < N / 2; i++)
            {
                even[i] = x[i * 2];
                odd[i] = x[i * 2 + 1];
            }

            var fftEven = FFT(even);
            var fftOdd = FFT(odd);

            var result = new Complex[N];
            for (int k = 0; k < N / 2; k++)
            {
                var t = Complex.FromPolarCoordinates(1.0, -2 * Math.PI * k / N) * fftOdd[k];
                result[k] = fftEven[k] + t;
                result[k + N / 2] = fftEven[k] - t;
            }

            return result;
        }

        private Complex[] InverseFFT(Complex[] x)
        {
            int N = x.Length;
            var conjugated = x.Select(c => Complex.Conjugate(c)).ToArray();
            var result = FFT(conjugated);
            return result.Select(c => Complex.Conjugate(c) / N).ToArray();
        }

        private void NormalizeSamples(float[] samples)
        {
            var maxAbs = samples.Max(s => Math.Abs(s));
            if (maxAbs > 0.95f)
            {
                var scale = 0.95f / maxAbs;
                for (int i = 0; i < samples.Length; i++)
                {
                    samples[i] *= scale;
                }
            }
        }
    }
}

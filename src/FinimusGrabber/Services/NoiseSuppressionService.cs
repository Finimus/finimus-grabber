using FinimusGrabber.Models;
using System.Numerics;

namespace FinimusGrabber.Services
{
    /// <summary>
    /// Noise suppression service using spectral gating
    /// This is a traditional DSP approach that can be replaced with AI models
    /// </summary>
    public class NoiseSuppressionService
    {
        private readonly int _fftSize;
        private readonly double _noiseGateThreshold;
        private readonly double _reductionAmount;

        public NoiseSuppressionService(int fftSize = 2048, double noiseGateThreshold = 0.02, double reductionAmount = 0.8)
        {
            _fftSize = fftSize;
            _noiseGateThreshold = noiseGateThreshold;
            _reductionAmount = reductionAmount;
        }

        /// <summary>
        /// Suppress noise in audio using spectral gating
        /// </summary>
        public AudioData SuppressNoise(AudioData audioData, Action<double>? progressCallback = null)
        {
            var samples = (float[])audioData.Samples.Clone();
            var hopSize = _fftSize / 4;

            // Estimate noise profile from first 0.5 seconds
            var noiseProfile = EstimateNoiseProfile(samples, audioData.SampleRate, audioData.Channels);

            // Process audio in overlapping windows
            for (int ch = 0; ch < audioData.Channels; ch++)
            {
                var channelSamples = ExtractChannel(samples, ch, audioData.Channels);
                var processedChannel = new float[channelSamples.Length];

                for (int i = 0; i < channelSamples.Length - _fftSize; i += hopSize)
                {
                    var window = new float[_fftSize];
                    Array.Copy(channelSamples, i, window, 0, _fftSize);

                    // Apply Hann window
                    ApplyHannWindow(window);

                    // Perform FFT
                    var spectrum = PerformFFT(window);

                    // Apply spectral gating
                    ApplySpectralGating(spectrum, noiseProfile);

                    // Inverse FFT
                    var processed = PerformInverseFFT(spectrum);

                    // Overlap-add
                    for (int j = 0; j < processed.Length && i + j < processedChannel.Length; j++)
                    {
                        processedChannel[i + j] += processed[j];
                    }

                    // Report progress
                    if (progressCallback != null && i % (hopSize * 10) == 0)
                    {
                        var progress = (double)i / (channelSamples.Length - _fftSize);
                        progressCallback(progress);
                    }
                }

                // Write back to main sample array
                for (int i = 0; i < channelSamples.Length; i++)
                {
                    samples[i * audioData.Channels + ch] = processedChannel[i];
                }
            }

            // Normalize
            NormalizeSamples(samples);

            return new AudioData
            {
                Samples = samples,
                SampleRate = audioData.SampleRate,
                Channels = audioData.Channels,
                FilePath = audioData.FilePath
            };
        }

        private double[] EstimateNoiseProfile(float[] samples, int sampleRate, int channels)
        {
            // Use first 0.5 seconds to estimate noise
            var noiseSamples = Math.Min(sampleRate / 2, samples.Length / channels);
            var profile = new double[_fftSize / 2];

            var monoNoise = new float[noiseSamples];
            for (int i = 0; i < noiseSamples; i++)
            {
                float sum = 0;
                for (int ch = 0; ch < channels; ch++)
                {
                    if (i * channels + ch < samples.Length)
                        sum += samples[i * channels + ch];
                }
                monoNoise[i] = sum / channels;
            }

            // Average spectrum over noise period
            int windowCount = 0;
            for (int i = 0; i < monoNoise.Length - _fftSize; i += _fftSize / 2)
            {
                var window = new float[_fftSize];
                Array.Copy(monoNoise, i, window, 0, _fftSize);
                ApplyHannWindow(window);

                var spectrum = PerformFFT(window);
                for (int j = 0; j < profile.Length; j++)
                {
                    profile[j] += spectrum[j].Magnitude;
                }
                windowCount++;
            }

            // Average
            for (int i = 0; i < profile.Length; i++)
            {
                profile[i] /= Math.Max(1, windowCount);
            }

            return profile;
        }

        private void ApplySpectralGating(Complex[] spectrum, double[] noiseProfile)
        {
            for (int i = 0; i < Math.Min(spectrum.Length / 2, noiseProfile.Length); i++)
            {
                var magnitude = spectrum[i].Magnitude;
                var noiseLevel = noiseProfile[i];

                // Calculate gain reduction
                double gain = 1.0;
                if (magnitude < noiseLevel * (1 + _noiseGateThreshold))
                {
                    // Reduce gain proportionally
                    gain = 1.0 - _reductionAmount * (1 - magnitude / (noiseLevel * (1 + _noiseGateThreshold)));
                    gain = Math.Max(0, gain);
                }

                // Apply gain
                spectrum[i] *= gain;
                
                // Mirror for negative frequencies
                if (i > 0 && i < spectrum.Length / 2)
                {
                    spectrum[spectrum.Length - i] *= gain;
                }
            }
        }

        private float[] ExtractChannel(float[] samples, int channel, int totalChannels)
        {
            var channelSamples = new float[samples.Length / totalChannels];
            for (int i = 0; i < channelSamples.Length; i++)
            {
                channelSamples[i] = samples[i * totalChannels + channel];
            }
            return channelSamples;
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
            
            // Conjugate input
            var conjugated = x.Select(c => Complex.Conjugate(c)).ToArray();
            
            // Perform FFT
            var result = FFT(conjugated);
            
            // Conjugate and scale output
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

namespace FinimusGrabber.Models
{
    /// <summary>
    /// Holds audio data and metadata
    /// </summary>
    public class AudioData
    {
        public float[] Samples { get; set; }
        public int SampleRate { get; set; }
        public int Channels { get; set; }
        public double DurationSeconds => (double)Samples.Length / (SampleRate * Channels);
        public string FilePath { get; set; }
        public string FileName => Path.GetFileName(FilePath);

        public AudioData()
        {
            Samples = Array.Empty<float>();
            SampleRate = 44100;
            Channels = 1;
            FilePath = string.Empty;
        }
    }
}

using System.IO;
using NAudio.Utils;
using NAudio.Wave;

namespace HappyTetris.Audio
{
    public class AudioController
    {
        private bool _enabled = true;
        private WaveOutEvent? _waveOut;
        private MemoryStream? _activeStream;
        private WaveStream? _activeReader;
        public bool Enabled => _enabled;

        public void Toggle()
        {
            _enabled = !_enabled;
        }

        public void PlayTone(double frequency, int durationMs, WaveformType waveform = WaveformType.Sine, double volume = 0.3)
        {
            if (!_enabled) return;

            try
            {
                CleanupCurrentPlayback();

                var waveFormat = new WaveFormat(44100, 16, 1);
                int samples = (int)(durationMs / 1000.0 * waveFormat.SampleRate);
                var buffer = new short[samples];

                for (int i = 0; i < samples; i++)
                {
                    double t = (double)i / waveFormat.SampleRate;
                    double sample = 0;

                    switch (waveform)
                    {
                        case WaveformType.Sine:
                            sample = Math.Sin(2 * Math.PI * frequency * t);
                            break;
                        case WaveformType.Square:
                            sample = Math.Sin(2 * Math.PI * frequency * t) > 0 ? 1 : -1;
                            break;
                        case WaveformType.Triangle:
                            sample = 2 * Math.Asin(Math.Sin(2 * Math.PI * frequency * t)) / Math.PI;
                            break;
                        case WaveformType.Sawtooth:
                            sample = 2 * ((frequency * t) % 1 - 0.5);
                            break;
                    }

                    buffer[i] = (short)(sample * volume * short.MaxValue);
                }

                // Create a WaveStream from the buffer.
                var ms = new MemoryStream();
                using (var writer = new WaveFileWriter(new IgnoreDisposeStream(ms), waveFormat))
                {
                    writer.WriteSamples(buffer, 0, samples);
                }

                ms.Position = 0;
                var reader = new WaveFileReader(ms);

                _activeStream = ms;
                _activeReader = reader;

                _waveOut = new WaveOutEvent();
                _waveOut.PlaybackStopped += OnPlaybackStopped;
                _waveOut.Init(reader);
                _waveOut.Play();
            }
            catch
            {
                CleanupCurrentPlayback();
                // Ignore audio errors
            }
        }

        public void PlayMove() 
        {
            if (!_enabled) return;
            PlayTone(400, 80, WaveformType.Sine, 0.3);
        }

        public void PlayRotate() 
        {
            if (!_enabled) return;
            PlayTone(600, 100, WaveformType.Sine, 0.3);
        }

        public void PlayDrop() 
        {
            if (!_enabled) return;
            PlayTone(300, 150, WaveformType.Triangle, 0.3);
        }

        public void PlayClear()
        {
            if (!_enabled) return;
            // Play a pleasant chord for line clear
            PlayTone(523, 200, WaveformType.Sine, 0.3);  // C5
            System.Threading.Thread.Sleep(50);
            PlayTone(659, 200, WaveformType.Sine, 0.3);  // E5
            System.Threading.Thread.Sleep(50);
            PlayTone(784, 300, WaveformType.Sine, 0.3);  // G5
        }

        public void PlayLevelUp()
        {
            if (!_enabled) return;
            // Play ascending arpeggio
            PlayTone(523, 150, WaveformType.Sine, 0.3);
            System.Threading.Thread.Sleep(100);
            PlayTone(659, 150, WaveformType.Sine, 0.3);
            System.Threading.Thread.Sleep(100);
            PlayTone(784, 150, WaveformType.Sine, 0.3);
            System.Threading.Thread.Sleep(100);
            PlayTone(1047, 300, WaveformType.Sine, 0.3);
        }

        public void PlayGameOver()
        {
            if (!_enabled) return;
            // Play descending sad melody
            PlayTone(400, 400, WaveformType.Sawtooth, 0.25);
            System.Threading.Thread.Sleep(350);
            PlayTone(350, 400, WaveformType.Sawtooth, 0.25);
            System.Threading.Thread.Sleep(350);
            PlayTone(300, 600, WaveformType.Sawtooth, 0.25);
        }

        public void PlayStart()
        {
            if (!_enabled) return;
            // Play happy startup jingle
            PlayTone(523, 150, WaveformType.Sine, 0.3);
            System.Threading.Thread.Sleep(100);
            PlayTone(659, 150, WaveformType.Sine, 0.3);
            System.Threading.Thread.Sleep(100);
            PlayTone(784, 150, WaveformType.Sine, 0.3);
            System.Threading.Thread.Sleep(100);
            PlayTone(1047, 400, WaveformType.Sine, 0.3);
        }

        public void Cleanup()
        {
            CleanupCurrentPlayback();
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            CleanupCurrentPlayback(stopWaveOut: false);
        }

        private void CleanupCurrentPlayback(bool stopWaveOut = true)
        {
            if (_waveOut != null)
            {
                _waveOut.PlaybackStopped -= OnPlaybackStopped;
                if (stopWaveOut && _waveOut.PlaybackState != PlaybackState.Stopped)
                {
                    _waveOut.Stop();
                }
                _waveOut.Dispose();
                _waveOut = null;
            }

            _activeReader?.Dispose();
            _activeReader = null;

            _activeStream?.Dispose();
            _activeStream = null;
        }
    }

    public enum WaveformType
    {
        Sine,
        Square,
        Triangle,
        Sawtooth
    }
}

using System.IO;
using NAudio.Utils;
using NAudio.Wave;

namespace HappyTetris.Audio
{
    public class AudioController
    {
        private bool _effectsEnabled = true;
        private bool _musicEnabled = false;
        private WaveOutEvent? _effectWaveOut;
        private MemoryStream? _effectStream;
        private WaveStream? _effectReader;

        private WaveOutEvent? _musicWaveOut;
        private MemoryStream? _musicStream;
        private WaveStream? _musicReader;
        private WaveStream? _musicLoopStream;

        private const int SampleRate = 44100;
        private const double MusicVolume = 0.13;
        private const int TempoBpm = 76; // Gentler tempo for older adults.
        private const int QuarterNoteMs = 60000 / TempoBpm;
        private const int EighthNoteMs = QuarterNoteMs / 2;
        private const int HalfNoteMs = QuarterNoteMs * 2;
        private const int DottedQuarterNoteMs = QuarterNoteMs + EighthNoteMs;

        // Full-length Type-A style arrangement represented as note names + pitches.
        private static readonly IReadOnlyList<MusicNote> TetrisThemeScore = BuildTetrisThemeScore();

        public bool EffectsEnabled => _effectsEnabled;
        public bool MusicEnabled => _musicEnabled;

        // Backward-compatible alias for older call sites.
        public void Toggle()
        {
            ToggleEffects();
        }

        public void ToggleEffects()
        {
            _effectsEnabled = !_effectsEnabled;

            if (!_effectsEnabled)
            {
                CleanupEffectPlayback();
            }
        }

        public void ToggleMusic()
        {
            _musicEnabled = !_musicEnabled;
            if (!_musicEnabled)
            {
                StopBackgroundMusic();
            }
        }

        public void PlayTone(double frequency, int durationMs, WaveformType waveform = WaveformType.Sine, double volume = 0.3)
        {
            if (!_effectsEnabled) return;

            try
            {
                CleanupEffectPlayback();

                var waveFormat = new WaveFormat(SampleRate, 16, 1);
                int samples = (int)(durationMs / 1000.0 * waveFormat.SampleRate);
                var buffer = new short[samples];

                for (int i = 0; i < samples; i++)
                {
                    double t = (double)i / waveFormat.SampleRate;
                    double sample = GenerateWaveSample(frequency, t, waveform);

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

                _effectStream = ms;
                _effectReader = reader;

                _effectWaveOut = new WaveOutEvent();
                _effectWaveOut.PlaybackStopped += OnEffectPlaybackStopped;
                _effectWaveOut.Init(reader);
                _effectWaveOut.Play();
            }
            catch
            {
                CleanupEffectPlayback();
                // Ignore audio errors
            }
        }

        public void StartBackgroundMusic()
        {
            if (!_musicEnabled) return;

            if (_musicWaveOut != null && _musicWaveOut.PlaybackState == PlaybackState.Playing)
            {
                return;
            }

            try
            {
                StopBackgroundMusic();

                _musicStream = CreateTetrisThemeStream();
                _musicReader = new WaveFileReader(_musicStream);
                _musicLoopStream = new LoopStream(_musicReader);

                _musicWaveOut = new WaveOutEvent();
                _musicWaveOut.Init(_musicLoopStream);
                _musicWaveOut.Play();
            }
            catch
            {
                StopBackgroundMusic();
                // Ignore audio errors
            }
        }

        public void StopBackgroundMusic()
        {
            if (_musicWaveOut != null)
            {
                if (_musicWaveOut.PlaybackState != PlaybackState.Stopped)
                {
                    _musicWaveOut.Stop();
                }

                _musicWaveOut.Dispose();
                _musicWaveOut = null;
            }

            _musicLoopStream?.Dispose();
            _musicLoopStream = null;

            _musicReader?.Dispose();
            _musicReader = null;

            _musicStream?.Dispose();
            _musicStream = null;
        }

        public void PlayMove() 
        {
            if (!_effectsEnabled) return;
            PlayTone(400, 80, WaveformType.Sine, 0.3);
        }

        public void PlayRotate()
        {
            if (!_effectsEnabled) return;
            // Play a distinctive two-tone sound for rotation
            PlayTone(800, 60, WaveformType.Sine, 0.3);
            System.Threading.Thread.Sleep(50);
            PlayTone(1200, 80, WaveformType.Sine, 0.3);
        }

        public void PlaySoftDrop()
        {
            if (!_effectsEnabled) return;
            // Softer, shorter tone for one-step down movement.
            PlayTone(170, 90, WaveformType.Triangle, 0.26);
        }

        public void PlayHardDrop() 
        {
            if (!_effectsEnabled) return;
            // Low "boom" style impact for hard drop.
            PlayTone(95, 110, WaveformType.Sawtooth, 0.35);
            System.Threading.Thread.Sleep(70);
            PlayTone(62, 180, WaveformType.Triangle, 0.40);
        }

        public void PlayClear()
        {
            if (!_effectsEnabled) return;
            // Play a pleasant chord for line clear
            PlayTone(523, 200, WaveformType.Sine, 0.3);  // C5
            System.Threading.Thread.Sleep(50);
            PlayTone(659, 200, WaveformType.Sine, 0.3);  // E5
            System.Threading.Thread.Sleep(50);
            PlayTone(784, 300, WaveformType.Sine, 0.3);  // G5
        }

        public void PlayLevelUp()
        {
            if (!_effectsEnabled) return;
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
            if (!_effectsEnabled) return;
            // Play descending sad melody
            PlayTone(400, 400, WaveformType.Sawtooth, 0.25);
            System.Threading.Thread.Sleep(350);
            PlayTone(350, 400, WaveformType.Sawtooth, 0.25);
            System.Threading.Thread.Sleep(350);
            PlayTone(300, 600, WaveformType.Sawtooth, 0.25);
        }

        public void PlayStart()
        {
            if (!_effectsEnabled) return;
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
            StopBackgroundMusic();
            CleanupEffectPlayback();
        }

        private void OnEffectPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            CleanupEffectPlayback(stopWaveOut: false);
        }

        private void CleanupEffectPlayback(bool stopWaveOut = true)
        {
            if (_effectWaveOut != null)
            {
                _effectWaveOut.PlaybackStopped -= OnEffectPlaybackStopped;
                if (stopWaveOut && _effectWaveOut.PlaybackState != PlaybackState.Stopped)
                {
                    _effectWaveOut.Stop();
                }
                _effectWaveOut.Dispose();
                _effectWaveOut = null;
            }

            _effectReader?.Dispose();
            _effectReader = null;

            _effectStream?.Dispose();
            _effectStream = null;
        }

        private MemoryStream CreateTetrisThemeStream()
        {
            var waveFormat = new WaveFormat(SampleRate, 16, 1);
            int totalSamples = 0;
            foreach (var note in TetrisThemeScore)
            {
                totalSamples += (int)(note.DurationMs / 1000.0 * waveFormat.SampleRate);
            }

            var buffer = new short[Math.Max(totalSamples, 1)];
            int writeIndex = 0;
            int fadeSamples = (int)(0.006 * waveFormat.SampleRate);

            foreach (var note in TetrisThemeScore)
            {
                int samples = (int)(note.DurationMs / 1000.0 * waveFormat.SampleRate);
                if (samples <= 0)
                {
                    continue;
                }

                if (note.IsRest)
                {
                    writeIndex += samples;
                    continue;
                }

                int localFade = Math.Min(fadeSamples, samples / 2);
                for (int i = 0; i < samples && writeIndex < buffer.Length; i++)
                {
                    double t = (double)i / waveFormat.SampleRate;
                    double sample = GenerateWaveSample(note.PitchHz, t, WaveformType.Square);

                    if (localFade > 0)
                    {
                        if (i < localFade)
                        {
                            sample *= (double)i / localFade;
                        }
                        else if (i >= samples - localFade)
                        {
                            sample *= (double)(samples - i - 1) / localFade;
                        }
                    }

                    buffer[writeIndex++] = (short)(sample * MusicVolume * short.MaxValue);
                }
            }

            var ms = new MemoryStream();
            using (var writer = new WaveFileWriter(new IgnoreDisposeStream(ms), waveFormat))
            {
                writer.WriteSamples(buffer, 0, writeIndex);
            }

            ms.Position = 0;
            return ms;
        }

        private static IReadOnlyList<MusicNote> BuildTetrisThemeScore()
        {
            // Korobeiniki (Type-A Tetris melody), expanded into a full-length loop.
            return
            [
                ..BuildSectionA(),
                ..BuildSectionB(),
                ..BuildSectionA(),
                ..BuildSectionC(),
                ..BuildSectionA(),
                ..BuildSectionB(),
                ..BuildSectionD(),
                R(HalfNoteMs)
            ];
        }

        private static IReadOnlyList<MusicNote> BuildSectionA()
        {
            return
            [
                Q("E5"), E("B4"), E("C5"), Q("D5"),
                E("C5"), E("B4"), Q("A4"), E("A4"),
                E("C5"), Q("E5"), E("D5"), E("C5"),
                DQ("B4"), E("C5"), Q("D5"), Q("E5"),
                Q("C5"), Q("A4"), H("A4")
            ];
        }

        private static IReadOnlyList<MusicNote> BuildSectionB()
        {
            return
            [
                Q("D5"), E("F5"), Q("A5"), E("G5"),
                E("F5"), Q("E5"), E("C5"), Q("E5"),
                E("D5"), E("C5"), Q("B4"), E("B4"),
                E("C5"), Q("D5"), Q("E5"), Q("C5"),
                Q("A4"), H("A4")
            ];
        }

        private static IReadOnlyList<MusicNote> BuildSectionC()
        {
            return
            [
                Q("E5"), E("C5"), E("D5"), Q("B4"),
                E("C5"), E("A4"), Q("G#4"), E("B4"),
                E("E5"), E("C5"), E("D5"), E("B4"),
                E("C5"), E("E5"), Q("A5"), E("G#5"),
                E("A5"), E("G5"), E("F5"), E("E5"),
                Q("D5"), E("C5"), E("B4"), Q("A4")
            ];
        }

        private static IReadOnlyList<MusicNote> BuildSectionD()
        {
            return
            [
                E("A4"), E("A4"), E("A4"), E("B4"),
                E("C5"), E("D5"), Q("E5"), E("C5"),
                E("A4"), E("A4"), E("G4"), E("A4"),
                Q("B4"), E("C5"), E("D5"), Q("E5"),
                Q("C5"), Q("A4"), H("A4")
            ];
        }

        private static MusicNote N(string name, int durationMs)
        {
            return MusicNote.Named(name, durationMs);
        }

        private static MusicNote E(string name)
        {
            return N(name, EighthNoteMs);
        }

        private static MusicNote Q(string name)
        {
            return N(name, QuarterNoteMs);
        }

        private static MusicNote H(string name)
        {
            return N(name, HalfNoteMs);
        }

        private static MusicNote DQ(string name)
        {
            return N(name, DottedQuarterNoteMs);
        }

        private static MusicNote R(int durationMs)
        {
            return MusicNote.Rest(durationMs);
        }

        private static double GenerateWaveSample(double frequency, double t, WaveformType waveform)
        {
            return waveform switch
            {
                WaveformType.Sine => Math.Sin(2 * Math.PI * frequency * t),
                WaveformType.Square => Math.Sin(2 * Math.PI * frequency * t) > 0 ? 1 : -1,
                WaveformType.Triangle => 2 * Math.Asin(Math.Sin(2 * Math.PI * frequency * t)) / Math.PI,
                WaveformType.Sawtooth => 2 * ((frequency * t) % 1 - 0.5),
                _ => 0
            };
        }

        private sealed class LoopStream : WaveStream
        {
            private readonly WaveStream _sourceStream;

            public LoopStream(WaveStream sourceStream)
            {
                _sourceStream = sourceStream;
            }

            public override WaveFormat WaveFormat => _sourceStream.WaveFormat;

            public override long Length => _sourceStream.Length;

            public override long Position
            {
                get => _sourceStream.Position;
                set => _sourceStream.Position = value;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int totalBytesRead = 0;

                while (totalBytesRead < count)
                {
                    int bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                    if (bytesRead == 0)
                    {
                        _sourceStream.Position = 0;
                        bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                        if (bytesRead == 0)
                        {
                            break;
                        }
                    }

                    totalBytesRead += bytesRead;
                }

                return totalBytesRead;
            }
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

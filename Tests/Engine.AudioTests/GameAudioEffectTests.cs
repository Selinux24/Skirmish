using Engine.Audio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Engine.AudioTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class GameAudioEffectTests
    {
        private IGameAudio _gameAudio;
        private IGameAudioEffect _effect;
        private string _waveFile;

        [TestInitialize]
        public void Setup()
        {
            _gameAudio = new GameAudioLoader().GetDelegate(48000)();
            _waveFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.wav");
            WriteWaveFile(_waveFile);
        }

        [TestCleanup]
        public void Teardown()
        {
            _effect?.Dispose();
            _gameAudio?.Dispose();

            if (File.Exists(_waveFile))
            {
                File.Delete(_waveFile);
            }
        }

        [TestMethod]
        public void CreateEffect_InitializesStateAndParameters()
        {
            _effect = CreateEffect(new GameAudioEffectParameters
            {
                IsLooped = true,
                Pitch = 0.5f,
                Volume = 0.4f,
                ReverbPreset = null,
            });

            Assert.IsTrue(_effect.IsLooped);
            Assert.AreEqual(0.5f, _effect.Pitch);
            Assert.AreEqual(0.4f, _effect.Volume, 0.01f);
            Assert.IsFalse(_effect.UseAudio3D);
            Assert.IsNull(_effect.Emitter);
            Assert.IsNull(_effect.Listener);
            Assert.AreEqual(GameAudioState.Stopped, _effect.State);
            Assert.IsFalse(_effect.DueToDispose);
            Assert.IsGreaterThan(TimeSpan.Zero, _effect.Duration);
            Assert.IsNull(_effect.GetReverb());

            float[] outputMatrix = _effect.GetOutputMatrix();
            Assert.HasCount(2, outputMatrix);
            Assert.AreSequenceEqual([1f, 1f], outputMatrix);
        }

        [TestMethod]
        public void CreateEffect_WithAudio3D_InitializesEmitterAndListener()
        {
            _effect = CreateEffect(new GameAudioEffectParameters
            {
                UseAudio3D = true,
                ReverbPreset = null,
            });

            Assert.IsTrue(_effect.UseAudio3D);
            Assert.IsNotNull(_effect.Emitter);
            Assert.IsNotNull(_effect.Listener);
        }

        [TestMethod]
        public void SetPitch_ClampsValueToSupportedRange()
        {
            _effect = CreateEffect(new GameAudioEffectParameters { ReverbPreset = null });

            _effect.SetPitch(-2f);
            Assert.AreEqual(-1f, _effect.Pitch);

            _effect.SetPitch(2f);
            Assert.AreEqual(1f, _effect.Pitch);
        }

        [TestMethod]
        public void SetVolume_ClampsValueToSupportedRange()
        {
            _effect = CreateEffect(new GameAudioEffectParameters { ReverbPreset = null });

            _effect.SetVolume(-2f);
            Assert.AreEqual(0f, _effect.Volume, 0.01f);

            _effect.SetVolume(2f);
            Assert.AreEqual(1f, _effect.Volume, 0.01f);
        }

        [TestMethod]
        public void Position_IsReadableAndWritable()
        {
            _effect = CreateEffect(new GameAudioEffectParameters { ReverbPreset = null });
            var position = TimeSpan.FromMilliseconds(125);

            _effect.Position = position;

            Assert.AreEqual(position, _effect.Position);
        }

        [TestMethod]
        public void IsLoopedAndAudio3D_CanBeChanged()
        {
            _effect = CreateEffect(new GameAudioEffectParameters { ReverbPreset = null });

            _effect.IsLooped = true;
            _effect.UseAudio3D = true;

            Assert.IsTrue(_effect.IsLooped);
            Assert.IsTrue(_effect.UseAudio3D);
        }

        [TestMethod]
        public void Update_WhenAudio3DIsDisabled_DoesNotChangeEffectState()
        {
            _effect = CreateEffect(new GameAudioEffectParameters { ReverbPreset = null });

            _effect.Update(new GameTime());

            Assert.AreEqual(GameAudioState.Stopped, _effect.State);
        }

        [TestMethod]
        public void PauseResumeAndStop_WhenStopped_KeepEffectStopped()
        {
            _effect = CreateEffect(new GameAudioEffectParameters { ReverbPreset = null });

            _effect.Pause();
            _effect.Resume();
            _effect.Stop();

            Assert.AreEqual(GameAudioState.Stopped, _effect.State);
        }

        [TestMethod]
        public void ToString_ReturnsSourceFileName()
        {
            _effect = CreateEffect(new GameAudioEffectParameters { ReverbPreset = null });

            Assert.AreEqual(_waveFile, _effect.ToString());
        }

        [TestMethod]
        public void PlaybackControls_AfterDisposal_DoNotChangeStoppedState()
        {
            var effect = CreateEffect(new GameAudioEffectParameters { ReverbPreset = null });
            effect.Dispose();

            effect.Play();
            effect.Play(TimeSpan.FromMilliseconds(50));
            effect.Pause();
            effect.Resume();
            effect.Stop();
            effect.Reset();

            Assert.AreEqual(GameAudioState.Stopped, effect.State);
        }

        private IGameAudioEffect CreateEffect(GameAudioEffectParameters parameters)
        {
            return _gameAudio.CreateEffect(_waveFile, parameters);
        }

        private static void WriteWaveFile(string fileName)
        {
            const ushort channels = 1;
            const uint sampleRate = 48000;
            const ushort bitsPerSample = 16;
            const int sampleFrameCount = 4800;
            const int bytesPerSampleFrame = channels * (bitsPerSample / 8);
            const int dataSize = sampleFrameCount * bytesPerSampleFrame;

            using var stream = File.Create(fileName);
            using var writer = new BinaryWriter(stream);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataSize);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((ushort)1);
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * bytesPerSampleFrame);
            writer.Write((ushort)bytesPerSampleFrame);
            writer.Write(bitsPerSample);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);
            writer.Write(new byte[dataSize]);
        }
    }
}

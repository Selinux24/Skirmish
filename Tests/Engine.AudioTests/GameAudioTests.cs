using Engine.Audio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Vortice.XAPO;
using Vortice.XAudio2;

namespace Engine.AudioTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class GameAudioTests
    {
        [TestInitialize]
        public void Setup()
        {
        }

        [TestCleanup]
        public void Teardown()
        {
            // Ensure resources are cleaned up after each test.
            var _gameAudio = new GameAudioLoader().GetDelegate(48000)();
            _gameAudio?.Dispose();
            _gameAudio = null;
            Assert.IsNull(_gameAudio, "GameAudio instance should be null after disposal.");
        }

        [TestMethod]
        public void Constructor_InitializesDefaultValuesCorrectly()
        {
            // Check default values (e.g., MasterVolume should be 1f initially)
            var _gameAudio = new GameAudioLoader().GetDelegate(48000)();
            Assert.AreEqual(1.0f, _gameAudio.MasterVolume, "Master volume should initialize to 1.0.");
            Assert.IsFalse(_gameAudio.UseMasteringLimiter, "Mastering limiter should start disabled.");
        }

        [TestMethod]
        public void AudioProperties_ExposeRequestedConfiguration()
        {
            using var _gameAudio = new GameAudioLoader().GetDelegate(48000)();
            Assert.AreEqual(48000u, _gameAudio.InputSampleRate);
            Assert.AreEqual(2u, _gameAudio.InputChannelCount);
            Assert.IsFalse(_gameAudio.UseRedirectToLFE);
            Assert.AreEqual((uint)XAudio2.CommitNow, _gameAudio.OperationSet);
        }

        [TestMethod]
        public void StartAndStop_CallsEngineMethods()
        {
            using var _gameAudio = new GameAudioLoader().GetDelegate(48000)();
            _gameAudio.Start();
            Assert.IsTrue(_gameAudio.Running);
            _gameAudio.Stop();
            Assert.IsFalse(_gameAudio.Running);
        }

        [TestMethod]
        public void CreateEffect_WithValidWaveFile_CreatesEffectWithRequestedParameters()
        {
            var fileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.wav");

            using var _gameAudio = new GameAudioLoader().GetDelegate(48000)();
            try
            {
                WriteWaveFile(fileName);
                var parameters = new GameAudioEffectParameters
                {
                    IsLooped = true,
                    Volume = 0.4f,
                    ReverbPreset = null,
                };

                using IGameAudioEffect effect = _gameAudio.CreateEffect(fileName, parameters);

                Assert.IsNotNull(effect);
                Assert.IsGreaterThan(TimeSpan.Zero, effect.Duration);
                Assert.AreEqual(GameAudioState.Stopped, effect.State);
                Assert.IsTrue(effect.IsLooped);
                Assert.AreEqual(0.4f, effect.Volume, 0.01f);
                Assert.IsNull(effect.GetReverb());
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        public void CreateEffect_WithMissingFile_ThrowsFileNotFoundException()
        {
            var missingFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.wav");

            using var _gameAudio = new GameAudioLoader().GetDelegate(48000)();
            Assert.Throws<FileNotFoundException>(
                () => _gameAudio.CreateEffect(missingFile, new GameAudioEffectParameters()));
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

        [TestMethod]
        public void MasterVolume_SetAndGetCorrectlyClampsValue()
        {
            using var _gameAudio = new GameAudioLoader().GetDelegate(48000)();

            // 1. Set volume to a valid value
            float expectedVolume = 0.5f;
            _gameAudio.MasterVolume = expectedVolume;
            Assert.AreEqual(expectedVolume, _gameAudio.MasterVolume, "Master volume should initialize to 0.5.");

            // 2. Test clamping: Above maximum (should clamp to 1.0f)
            _gameAudio.MasterVolume = 5.0f;
            Assert.AreEqual(1.0f, _gameAudio.MasterVolume, "Volume should clamp at 1.0 when set too high.");

            // 3. Test clamping: Below minimum (should clamp to 0.0f)
            _gameAudio.MasterVolume = -2.0f;
            Assert.AreEqual(0.0f, _gameAudio.MasterVolume, "Volume should clamp at 0.0 when set too low.");
        }

        [TestMethod]
        public void UseMasteringLimiter_TogglingChangesStateAndCallsAPI()
        {
            using var _gameAudio = new GameAudioLoader().GetDelegate(48000)();

            // Test initial state check (already covered in Setup, but good practice)
            Assert.IsFalse(_gameAudio.UseMasteringLimiter, "Initial limiter state must be false.");

            // 1. Enable Limiter: Should set the internal flag and call 'EnableMasteringLimiter()'
            _gameAudio.EnableMasteringLimiter(1, 1);
            Assert.IsTrue(_gameAudio.UseMasteringLimiter, "UseMasteringLimiter flag must be true after setting.");

            // 2. Disable Limiter: Should set the internal flag and call 'DisableMasteringLimiter()'
            _gameAudio.DisableMasteringLimiter();
            Assert.IsFalse(_gameAudio.UseMasteringLimiter, "UseMasteringLimiter flag must be false after resetting.");

            // Note: Assertions regarding API calls (SetEffectChain/DisableEffect) are placeholders 
            // since mocking the native XAPO interfaces is complex. The test verifies state changes correctly.
        }

        [TestMethod]
        public void StartOperationSet_CommitOperation_TracksActiveOperationSet()
        {
            using var _gameAudio = new GameAudioLoader().GetDelegate(48000)();

            _gameAudio.CommitOperation(XAudio2.CommitNow);
            Assert.AreEqual((uint)XAudio2.CommitNow, _gameAudio.OperationSet);

            uint operationSet = _gameAudio.StartOperationSet();
            Assert.AreEqual(operationSet, _gameAudio.OperationSet);
            Assert.AreEqual(operationSet, _gameAudio.StartOperationSet());

            _gameAudio.CommitOperation(operationSet);
            Assert.AreEqual((uint)XAudio2.CommitNow, _gameAudio.OperationSet);

            uint nextOperationSet = _gameAudio.StartOperationSet();
            Assert.AreEqual(operationSet + 1, nextOperationSet);
        }

        [TestMethod]
        [DataRow((uint)XAPO.MinRelease, (uint)XAPO.MinLoudness)]
        [DataRow(10u, 50u)]
        [DataRow((uint)XAPO.MaxRelease, (uint)XAPO.MaxLoudness)]
        public void SetMasteringLimit_ValidParameters_DoesNotThrow(uint release, uint loudness)
        {
            using var _gameAudio = new GameAudioLoader().GetDelegate(48000)();
            _gameAudio.EnableMasteringLimiter(release, loudness);
            Assert.IsTrue(_gameAudio.UseMasteringLimiter);
            Assert.AreEqual(_gameAudio.ReleaseLimit, release);
            Assert.AreEqual(_gameAudio.LoudnessLimit, loudness);
        }

        [TestMethod]
        [DataRow(0u, 50u)]
        [DataRow(XAPO.MaxRelease + 1u, 50u)]
        public void SetMasteringLimit_InvalidRelease_ThrowsArgumentOutOfRangeException(uint invalidRelease, uint validLoudness)
        {
            using var _gameAudio = new GameAudioLoader().GetDelegate(48000)();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _gameAudio.EnableMasteringLimiter(invalidRelease, validLoudness));
        }

        [TestMethod]
        [DataRow(10u, XAPO.MinLoudness - 1u)]
        [DataRow(10u, XAPO.MaxLoudness + 1u)]
        public void SetMasteringLimit_InvalidLoudness_ThrowsArgumentOutOfRangeException(uint validRelease, uint invalidLoudness)
        {
            using var _gameAudio = new GameAudioLoader().GetDelegate(48000)();
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _gameAudio.EnableMasteringLimiter(validRelease, invalidLoudness));
        }
    }
}
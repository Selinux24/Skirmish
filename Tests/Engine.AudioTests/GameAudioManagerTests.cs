using Engine.Audio;
using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace Engine.AudioTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [DoNotParallelize]
    [OSCondition(OperatingSystems.Windows)]
    public class GameAudioManagerTests
    {
        private Mock<IGameAudio> _audioMock;
        private GameAudioManager _manager;

        [TestInitialize]
        public void Setup()
        {
            _audioMock = new Mock<IGameAudio>();
            _audioMock.SetupProperty(audio => audio.MasterVolume, 0.75f);
            TestAudioLoader.Audio = _audioMock.Object;
            TestAudioLoader.RequestedSampleRate = 0;

            GameAudioManager.RegisterAudio<TestAudioLoader>(44100);
            _manager = new GameAudioManager();
        }

        [TestCleanup]
        public void Teardown()
        {
            _manager?.Dispose();
            _manager = null;
            TestAudioLoader.Audio = null;
        }

        [TestMethod]
        public void RegisterAudio_UsesLoaderAndRequestedSampleRate()
        {
            Assert.AreEqual(44100u, TestAudioLoader.RequestedSampleRate);
            Assert.AreSame(_audioMock.Object, GameAudioManager.GetAudio());
        }

        [TestMethod]
        public void AudioProperties_ForwardGettersAndSetters()
        {
            Assert.AreEqual(0.75f, _manager.MasterVolume);
            _manager.MasterVolume = 0.3f;
            Assert.AreEqual(0.3f, _audioMock.Object.MasterVolume);
        }

        [TestMethod]
        public void StartStopAndSetMasteringLimit_ForwardCallsToAudio()
        {
            _manager.Start();
            _manager.Stop();
            _manager.EnableMasteringLimiter(10, 50);

            _audioMock.Verify(audio => audio.Start(), Times.Once);
            _audioMock.Verify(audio => audio.Stop(), Times.Once);
            _audioMock.Verify(audio => audio.EnableMasteringLimiter(10, 50), Times.Once);
        }

        [TestMethod]
        public void LoadSound_ExistingFile_AllowsCreatingConfiguredEffect()
        {
            using var sound = new TemporarySoundFile();
            const string effectName = "effect";
            var parameters = new GameAudioEffectParameters { SoundName = "sound", Volume = 0.4f };
            var effectMock = CreateEffectMock();
            _audioMock.Setup(audio => audio.CreateEffect(It.IsAny<string>(), It.IsAny<GameAudioEffectParameters>()))
                .Returns(effectMock.Object);

            _manager.LoadSound("sound", sound.DirectoryPath, TemporarySoundFile.FileName);
            _manager.AddEffectParams(effectName, parameters);

            IGameAudioEffect result = _manager.CreateEffectInstance(effectName);

            Assert.AreSame(effectMock.Object, result);
            _audioMock.Verify(audio => audio.CreateEffect(
                sound.FilePath,
                It.Is<GameAudioEffectParameters>(value => value.SoundName == "sound" && value.Volume == 0.4f)), Times.Once);
        }

        [TestMethod]
        public void LoadSound_MissingFile_ThrowsFileNotFoundException()
        {
            using var directory = new TemporaryDirectory();

            Assert.Throws<FileNotFoundException>(
                () => _manager.LoadSound("sound", directory.Path, "missing.wav"));
        }

        [TestMethod]
        public void CreateEffectInstance_WhenEffectOrSoundIsNotRegistered_ReturnsNull()
        {
            Assert.IsNull(_manager.CreateEffectInstance("missing-effect"));

            _manager.AddEffectParams("effect", new GameAudioEffectParameters { SoundName = "missing-sound" });
            Assert.IsNull(_manager.CreateEffectInstance("effect"));

            _audioMock.Verify(audio => audio.CreateEffect(It.IsAny<string>(), It.IsAny<GameAudioEffectParameters>()), Times.Never);
        }

        [TestMethod]
        public void CreateEffectInstance_AlreadyPlayingEffect_ReturnsNull()
        {
            using var sound = new TemporarySoundFile();
            var effectMock = CreateEffectMock(GameAudioState.Stopped);
            _audioMock.Setup(audio => audio.CreateEffect(It.IsAny<string>(), It.IsAny<GameAudioEffectParameters>()))
                .Returns(effectMock.Object);
            _manager.LoadSound("sound", sound.DirectoryPath, TemporarySoundFile.FileName);
            _manager.AddEffectParams("effect", new GameAudioEffectParameters { SoundName = "sound" });

            Assert.AreSame(effectMock.Object, _manager.CreateEffectInstance("effect"));
            effectMock.Setup(effect => effect.State).Returns(GameAudioState.Playing);
            Assert.IsNull(_manager.CreateEffectInstance("effect"));

            _audioMock.Verify(audio => audio.CreateEffect(It.IsAny<string>(), It.IsAny<GameAudioEffectParameters>()), Times.Once);
        }

        [TestMethod]
        public void CreateEffectInstance_WithTransforms_WhenEffectIsNotRegistered_ReturnsNull()
        {
            var transformMock = new Mock<ITransform>();
            var transformableMock = new Mock<ITransformable3D>();

            Assert.IsNull(_manager.CreateEffectInstance("missing", new Vector3(), transformMock.Object));
            Assert.IsNull(_manager.CreateEffectInstance("missing", transformMock.Object, transformMock.Object));
            Assert.IsNull(_manager.CreateEffectInstance("missing", transformableMock.Object, transformMock.Object));
        }

        [TestMethod]
        public void Update_UpdatesPlayingThreeDimensionalEffects()
        {
            using var sound = new TemporarySoundFile();
            var effectMock = CreateEffectMock(GameAudioState.Playing, useAudio3D: true);
            _audioMock.Setup(audio => audio.CreateEffect(It.IsAny<string>(), It.IsAny<GameAudioEffectParameters>()))
                .Returns(effectMock.Object);
            _manager.LoadSound("sound", sound.DirectoryPath, TemporarySoundFile.FileName);
            _manager.AddEffectParams("effect", new GameAudioEffectParameters { SoundName = "sound" });
            _manager.CreateEffectInstance("effect");

            var gameTime = new GameTime();
            _manager.Update(gameTime);

            effectMock.Verify(effect => effect.Update(gameTime), Times.Once);
        }

        [TestMethod]
        public void Update_DisposesEffectsMarkedForDisposal()
        {
            using var sound = new TemporarySoundFile();
            using var disposed = new ManualResetEventSlim();
            var effectMock = CreateEffectMock(GameAudioState.Stopped);
            effectMock.Setup(effect => effect.DueToDispose).Returns(true);
            effectMock.Setup(effect => effect.Dispose()).Callback(() => disposed.Set());
            _audioMock.Setup(audio => audio.CreateEffect(It.IsAny<string>(), It.IsAny<GameAudioEffectParameters>()))
                .Returns(effectMock.Object);
            _manager.LoadSound("sound", sound.DirectoryPath, TemporarySoundFile.FileName);
            _manager.AddEffectParams("effect", new GameAudioEffectParameters { SoundName = "sound" });
            _manager.CreateEffectInstance("effect");

            _manager.Update(new GameTime());

            Assert.IsTrue(disposed.Wait(TimeSpan.FromSeconds(5), TestContext.CancellationToken), "The scheduled effect disposal did not complete.");
            effectMock.Verify(effect => effect.Stop(true), Times.Once);
            effectMock.Verify(effect => effect.Dispose(), Times.Once);
            effectMock.Setup(effect => effect.DueToDispose).Returns(false);
            Assert.AreSame(effectMock.Object, _manager.CreateEffectInstance("effect"));
        }

        [TestMethod]
        public void ClearEffects_DisposesEffectsAsynchronously()
        {
            using var sound = new TemporarySoundFile();
            using var disposed = new ManualResetEventSlim();
            var effectMock = CreateEffectMock();
            effectMock.Setup(effect => effect.Dispose()).Callback(() => disposed.Set());
            _audioMock.Setup(audio => audio.CreateEffect(It.IsAny<string>(), It.IsAny<GameAudioEffectParameters>()))
                .Returns(effectMock.Object);
            _manager.LoadSound("sound", sound.DirectoryPath, TemporarySoundFile.FileName);
            _manager.AddEffectParams("effect", new GameAudioEffectParameters { SoundName = "sound" });
            _manager.CreateEffectInstance("effect");

            _manager.ClearEffects();

            Assert.IsTrue(disposed.Wait(TimeSpan.FromSeconds(5), TestContext.CancellationToken), "The scheduled effect disposal did not complete.");
            effectMock.Verify(effect => effect.Dispose(), Times.Once);
            Assert.AreSame(effectMock.Object, _manager.CreateEffectInstance("effect"));
        }

        [TestMethod]
        public void Dispose_StopsAndDisposesEffectsAndAudio()
        {
            using var sound = new TemporarySoundFile();
            var effectMock = CreateEffectMock();
            _audioMock.Setup(audio => audio.CreateEffect(It.IsAny<string>(), It.IsAny<GameAudioEffectParameters>()))
                .Returns(effectMock.Object);
            _manager.LoadSound("sound", sound.DirectoryPath, TemporarySoundFile.FileName);
            _manager.AddEffectParams("effect", new GameAudioEffectParameters { SoundName = "sound" });
            _manager.CreateEffectInstance("effect");

            _manager.Dispose();
            _manager = null;

            effectMock.Verify(effect => effect.Stop(true), Times.Once);
            effectMock.Verify(effect => effect.Dispose(), Times.Once);
            _audioMock.Verify(audio => audio.Dispose(), Times.Once);
        }

        private static Mock<IGameAudioEffect> CreateEffectMock(GameAudioState state = GameAudioState.Stopped, bool useAudio3D = false)
        {
            var effect = new Mock<IGameAudioEffect>();
            effect.Setup(value => value.State).Returns(state);
            effect.Setup(value => value.DueToDispose).Returns(false);
            effect.Setup(value => value.UseAudio3D).Returns(useAudio3D);
            return effect;
        }

        public sealed class TestAudioLoader : IGameAudioLoader
        {
            public static IGameAudio Audio { get; set; }
            public static uint RequestedSampleRate { get; set; }

            public Func<IGameAudio> GetDelegate(uint sampleRate)
            {
                RequestedSampleRate = sampleRate;
                return () => Audio;
            }
        }

        private sealed class TemporarySoundFile : IDisposable
        {
            public const string FileName = "sound.wav";

            private readonly TemporaryDirectory directory = new();

            public string DirectoryPath => directory.Path;
            public string FilePath => Path.Combine(DirectoryPath, FileName);

            public TemporarySoundFile()
            {
                File.WriteAllBytes(FilePath, []);
            }

            public void Dispose() => directory.Dispose();
        }

        private sealed class TemporaryDirectory : IDisposable
        {
            public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));

            public TemporaryDirectory()
            {
                Directory.CreateDirectory(Path);
            }

            public void Dispose()
            {
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, true);
                }
            }
        }

        public TestContext TestContext { get; set; }
    }
}

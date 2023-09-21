﻿using Engine;
using Engine.Content.FmtCollada;
using Engine.Windows;
using IntermediateSamples.SceneStart;
using System;

namespace IntermediateSamples
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
#if DEBUG
                Logger.LogLevel = LogLevel.Debug;
                Logger.LogStackSize = 0;
                Logger.EnableConsole = true;
#else
                Logger.LogLevel = LogLevel.Error;
#endif

                WindowsExtensions.Startup();

#if DEBUG
                using Game cl = new("Intermediate Samples", WindowsEngineForm.ScreenSize * 0.8f);
#else
                using Game cl = new("Intermediate Samples");
#endif

                GameResourceManager.RegisterLoader<LoaderCollada>();

                cl.SetScene<StartScene>();

                cl.Run();
            }
            catch (Exception ex)
            {
                Logger.WriteError(nameof(Program), ex);
            }
            finally
            {
#if DEBUG
                Logger.Dump("dumpDEBUG.txt");
#else
                if (Logger.HasErrors())
                {
                    Logger.Dump($"dump{DateTime.Now:yyyyMMddHHmmss.fff}.txt");
                }
#endif
            }
        }
    }
}
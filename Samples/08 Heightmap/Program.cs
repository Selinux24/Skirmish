﻿using Engine;
using Engine.Content.FmtCollada;
using System;

namespace Heightmap
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
#if DEBUG
                Logger.LogLevel = LogLevel.Information;
                Logger.LogStackSize = 0;

                var screen = EngineForm.ScreenSize * 0.8f;

                using (Game cl = new Game("8 Heightmap", false, screen.X, screen.Y, true, 0, 0))
#else
                Logger.LogLevel = LogLevel.Error;

                using (Game cl = new Game("8 Heightmap", true, 0, 0, true, 0, 4))
#endif
                {
                    cl.VisibleMouse = false;
#if DEBUG
                    cl.LockMouse = false;
#else
                    cl.LockMouse = true;
#endif

                    GameResourceManager.RegisterLoader<LoaderCollada>();

                    cl.SetScene<TestScene3D>();

                    cl.Run();
                }
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

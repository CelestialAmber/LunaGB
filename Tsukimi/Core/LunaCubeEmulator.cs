using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tsukimi.Graphics;

namespace Tsukimi.Core
{
    internal class LunaCubeEmulator : Emulator
    {
        public new const string name = "LunaCube";
        public new const EmulatorSystem system = EmulatorSystem.GC;
        public new const int screenWidth = 640;
        public new const int screenHeight = 480;

        public override bool LoadedRom()
        {
            throw new NotImplementedException();
        }

        public override void DoSingleStep()
        {
            throw new NotImplementedException();
        }

        public override void LoadFile(string romPath)
        {
            throw new NotImplementedException();
        }

        public override void Start(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        public override void TogglePause()
        {
            throw new NotImplementedException();
        }

        public override LunaImage GetScreenBitmap()
        {
            throw new NotImplementedException();
        }
    }
}

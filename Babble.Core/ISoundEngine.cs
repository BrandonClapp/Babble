using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Core
{
    public interface ISoundEngine
    {
        void Init();
        void SetRecordCallback(Action<byte[]> callback);
        void Record();
        void Play(byte[] data);
        void Stop();
    }
}

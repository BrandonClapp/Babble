using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Core
{
    public interface ISoundEngine
    {
        void Record(Action<byte[]> callback);
        void Play(byte[] data);
    }

    public class DummySoundEngine : ISoundEngine
    {
        public void Play(byte[] data)
        {
        }

        public void Record(Action<byte[]> callback)
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    callback(Encoding.UTF8.GetBytes("I'm walking on sunshine"));
                    System.Threading.Thread.Sleep(1000);
                }
            }, TaskCreationOptions.LongRunning);
            
        }
    }
}

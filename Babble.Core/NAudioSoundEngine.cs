using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Core
{
    public class NAudioSoundEngine : ISoundEngine
    {
        WaveInEvent waveIn;
        WaveOut waveOut;
        BufferedWaveProvider waveOutProvider;
        Action<byte[]> Record_Callback;

        public void Init()
        {
            waveIn = new WaveInEvent();
            waveIn.BufferMilliseconds = 100;
            waveIn.DeviceNumber = -1;
            waveIn.WaveFormat = NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(8000, 1);
            waveIn.DataAvailable += WaveIn_DataAvailable;

            waveOut = new WaveOut();
            waveOutProvider = new BufferedWaveProvider(waveIn.WaveFormat);
            waveOut.Init(waveOutProvider);
            waveOut.Play();
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (Record_Callback != null)
            {
                Record_Callback(e.Buffer);
            }
        }

        public void Destroy()
        {

        }

        public void Play(byte[] data)
        {
            waveOutProvider.AddSamples(data, 0, data.Length);
        }

        public void Record(Action<byte[]> callback)
        {
            waveIn.StartRecording();
            Record_Callback = callback;
        }
    }
}

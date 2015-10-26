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
        Action<byte[]> RecordCallback;

        public void Init()
        {
            waveIn = new WaveInEvent();
            waveIn.BufferMilliseconds = 100;
            waveIn.DeviceNumber = -1;
            waveIn.WaveFormat = new WaveFormat(8000, 1);
            waveIn.DataAvailable += WaveIn_DataAvailable;

            waveOut = new WaveOut();
            waveOutProvider = new BufferedWaveProvider(waveIn.WaveFormat);
            waveOut.Init(waveOutProvider);
            waveOut.Play();
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (RecordCallback != null)
            {
                RecordCallback(e.Buffer);
            }
        }

        public void StopRecording()
        {
            if (!CanRecord()) { return; }

            waveIn.StopRecording();
        }

        public void Play(byte[] data)
        {
            waveOutProvider.AddSamples(data, 0, data.Length);
        }

        public void Record()
        {
            if (!CanRecord()) { return; }

            waveIn.StartRecording();
        }

        public void SetRecordCallback(Action<byte[]> callback)
        {
            RecordCallback = callback;
        }

        private bool CanRecord()
        {
            if (waveIn == null) { return false; }
            if (WaveIn.DeviceCount == 0) { return false; }

            return true;
        }
    }
}

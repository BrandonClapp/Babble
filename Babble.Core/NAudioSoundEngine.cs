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
            waveIn.WaveFormat = NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(8000, 1);
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
            waveIn.StopRecording();
        }

        public void Play(byte[] data)
        {
            waveOutProvider.AddSamples(data, 0, data.Length);
        }

        public void Record()
        {
            waveIn.StartRecording();
        }

        public void SetRecordCallback(Action<byte[]> callback)
        {
            try
            {
                RecordCallback = callback;
            }
            catch (Exception ex)
            {
                // exception thrown when you don't have recording device
                // TODO: bubble this thing up to ui to let the user know
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}

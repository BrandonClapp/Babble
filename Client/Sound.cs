using System;
using System.IO;
using System.Threading;
using FragLabs.Audio.Codecs;
using FragLabs.Audio.Codecs.Opus;
using Microsoft.DirectX.DirectSound;

namespace Client
{
    public static class Sound
    {

        static Sound()
        {
            var handle = autoResetEvent.SafeWaitHandle.DangerousGetHandle();
            notify.SetNotificationPositions(new[] {
                new BufferPositionNotify { EventNotifyHandle = handle, Offset = bufferSize / 2 - 1 },
                new BufferPositionNotify { EventNotifyHandle = handle, Offset = bufferSize - 1 }
            });
        }

        private static Device device = new Device();
        private static WaveFormat waveFormat = new WaveFormat
        {
            Channels = 1,
            BitsPerSample = 16,
            SamplesPerSecond = 48000,
            BlockAlign = 2,
            AverageBytesPerSecond = 96000
        };
        private static int bufferSize = waveFormat.AverageBytesPerSecond / 2;
        private static CaptureBuffer captureBuffer = new CaptureBuffer(
            new CaptureBufferDescription { Format = waveFormat, BufferBytes = bufferSize },
            new Capture(new CaptureDevicesCollection()[0].DriverGuid)
        );
        private static AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        private static Notify notify = new Notify(captureBuffer);
        private static SecondaryBuffer playbackBuffer = new SecondaryBuffer(
                new BufferDescription
                {
                    Format = waveFormat,
                    BufferBytes = waveFormat.AverageBytesPerSecond / 2,
                    Flags = BufferDescriptionFlags.GlobalFocus
                }, device
        );
        private static MemoryStream memoryStream = new MemoryStream();

        public static void Record(IntPtr owner, Action<byte[]> callback)
        {
            device.SetCooperativeLevel(owner, CooperativeLevel.Priority);
            captureBuffer.Start(true);
            for (var b = true; ; b = !b)
            {
                autoResetEvent.WaitOne();
                memoryStream.Seek(0, SeekOrigin.Begin);
                captureBuffer.Read(b ? 0 : bufferSize / 2, memoryStream, bufferSize / 2, LockFlag.None);
                callback(Encode(memoryStream.GetBuffer()));
            }
        }

        public static void Play(IntPtr owner, byte[] buffer)
        {
            device.SetCooperativeLevel(owner, CooperativeLevel.Priority);
            playbackBuffer.SetCurrentPosition(0);
            playbackBuffer.Write(0, Decode(buffer), LockFlag.None);
            playbackBuffer.Play(0, BufferPlayFlags.Default);
        }

        public static byte[] Encode(byte[] buffer)
        {
            var encoder = OpusEncoder.Create(48000, 1, Application.Voip);
            var b = new byte[bufferSize/50]; // 1920 =  1 second : 960 = half second
            var encoded = new MemoryStream();
            for (var i = 0; i < buffer.Length; i += b.Length)
            {
                Array.Copy(buffer, i, b, 0, b.Length);
                int encodedLength;
                var encodedBuffer = encoder.Encode(b, b.Length, out encodedLength);
                encoded.Write(BitConverter.GetBytes(encodedLength), 0, 4);
                encoded.Write(encodedBuffer, 0, encodedLength);
            }
            return encoded.ToArray();
        }

        public static byte[] Decode(byte[] buffer)
        {
            var decoder = OpusDecoder.Create(48000, 1);
            var encoded = new MemoryStream(buffer);
            var decoded = new MemoryStream();
            for (var i = 0; i < buffer.Length; )
            {
                var len = new byte[4];
                i += encoded.Read(len, 0, len.Length);
                var encodedBuffer = new byte[BitConverter.ToInt32(len, 0)];
                i += encoded.Read(encodedBuffer, 0, encodedBuffer.Length);
                int decodedLength;
                var decodedBuffer = decoder.Decode(encodedBuffer, encodedBuffer.Length, out decodedLength);
                decoded.Write(decodedBuffer, 0, decodedLength);
            }
            return decoded.ToArray();
        }
    }
}
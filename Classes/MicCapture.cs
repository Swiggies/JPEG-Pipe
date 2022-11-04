using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using NAudio.Wave;
using System;

public class MicCapture
{
    private readonly Microphone _mic = Microphone.Default;
    private float _micFloatVolume;
    private float _lastMicFloatVolume;

    public Microphone Mic
    {
        get => _mic;
    }

    private WaveInEvent waveEvent;

    public float MicVolume { get => _micFloatVolume; }

    public float SmoothedMicVolume { get => MathHelper.Lerp(_lastMicFloatVolume, _micFloatVolume, 0.5f); }

    public MicCapture()
    {
        waveEvent = new WaveInEvent() { DeviceNumber = 0 };
        waveEvent.DataAvailable += OnDataAvailable;
        waveEvent.StartRecording();
        Console.WriteLine(WaveIn.GetCapabilities(0).ProductName);
    }

    private void OnDataAvailable(object sender, WaveInEventArgs e)
    {
        _lastMicFloatVolume = _micFloatVolume;
        float max = 0;
        // interpret as 16 bit audio
        for (int index = 0; index < e.BytesRecorded; index += 2)
        {
            short sample = (short)((e.Buffer[index + 1] << 8) |
                                    e.Buffer[index + 0]);
            // to floating point
            var sample32 = sample / 32768f;
            // absolute value 
            if (sample32 < 0) sample32 = -sample32;
            // is this the max value?
            if (sample32 > max) max = sample32;
        }

        _micFloatVolume = max;
    }
}
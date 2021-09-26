using CSIntel.Ipp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VE3NEA.HamCockpit.PluginAPI;
using VE3NEA.HamCockpit.DspFun;
using MathNet.Numerics.Statistics;
using MathNet.Numerics;

namespace VE3NEA.HamCockpitPlugins.Bandscope
{  
  public class SpectraEventArgs : EventArgs
  {
    public List<float[]> Spectra;
  }

  public unsafe class PowerSpectrum : IDisposable
  {
    private readonly Object inputLock = new object();
    private float[] inputBuffer;
    private EventWaitHandle wakeupEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
    private Thread thread;
    private bool stopping;
    private SynchronizationContext context = SynchronizationContext.Current;
    private float[] newSpectrum, smoothedSpectrum;
    private ComplexFft fft;
    private SignalFormat format;
    private float BinsPerHz;
    private float[] window;
    private RingBuffer ring;
    private int spectraPerSecond;
    private int floatStep;
    private readonly object zoomLock = new object();
    private int InStart, InLength;
    public int OutLength;
    private List<float[]> spectra;
    int inStart, inLength, outLength;
    float squeezeFactor;

    public int FftSize { get; private set; }
    public bool Pause;
    public event EventHandler<SpectraEventArgs> SpectraAvailable;

    public PowerSpectrum()
    {

    }

    public void Dispose()
    {
      if (fft != null) fft.Dispose();
      wakeupEvent.Dispose();
    }

    private const int PresumeFactor = 4;

    public void Initialize(SignalFormat format, int spectraPerSecond)
    {
      Active = false;

      this.format = format;
      this.spectraPerSecond = spectraPerSecond;

      //spectrum computed as soon as floatStep float values are received
      int floatsPerSecond = format.SamplingRate * Dsp.COMPONENTS_IN_COMPLEX;
      floatStep = (int)Math.Round(format.SamplingRate / (float)spectraPerSecond) * Dsp.COMPONENTS_IN_COMPLEX;

      //desired resolution is 10 Hz/bin, FftSize is the nearest power of 2
      FftSize = 1 << (int)Math.Round(Math.Log(format.SamplingRate / 10f) / Math.Log(2));
      BinsPerHz = FftSize / (float)format.SamplingRate;
      if (fft != null) fft.Dispose();
      fft = new ComplexFft(FftSize);

      //minimum input fps is 10 blocks per second, allocate space for 1 block + 1 step
      int ringSize = floatsPerSecond / 10 + floatStep;
      ring = new RingBuffer(ringSize);

      inputBuffer = new float[FftSize * Dsp.COMPONENTS_IN_COMPLEX * PresumeFactor];
      window = Dsp.BlackmanSincKernel(0.5f / FftSize, FftSize * PresumeFactor);

      //initially return the whole fft, 
      //the calling code is supposed to override this and select the desired slice
      SetZoomParams(0, FftSize, FftSize);

      newSpectrum = null;
      smoothedSpectrum = null;
    }

    public void SamplesAvailableEventHandler(object sender, SamplesAvailableEventArgs e)
    {
      if (!Active) return;
      ring.WriteStrided(e.Data, e.Offset, e.Count, format);
      wakeupEvent.Set();
    }

    public bool Active { get => thread != null; set => SetActive(value); }

    private void SetActive(bool value)
    {
      if (value == Active) return;
      stopping = Active;

      if (value)
      {
        thread = new Thread(new ThreadStart(ThreadProcedure));
        thread.Start();
      }
      else
      {
        wakeupEvent.Set();
        thread.Join();
        thread = null;
      }
    }

    public void SetZoomParams(int inStart, int inLength, int outLength)
    {
      lock (zoomLock)
      {
        InStart = inStart;
        InLength = inLength;
        OutLength = outLength;
      }
    }



    //----------------------------------------------------------------------------------------------
    //                                       thread
    //----------------------------------------------------------------------------------------------
    private void ThreadProcedure()
    {
      while (true)
      {
        //wait for input data to arrive
        wakeupEvent.WaitOne();
        if (stopping) return;

        spectra = new List<float[]>();
        while (ring.Count >= floatStep) ComputeSpectrum();
        if (spectra.Count == 0) continue;

        //notify the UI thread
        var e = new SpectraEventArgs { Spectra = spectra };
        context.Post(s => SpectraAvailable?.Invoke(this, e), null);
      }
    }

    private void ComputeFft()
    {
      int oldCount = inputBuffer.Length - floatStep;
      int size = FftSize;

      fixed (float* srcFloat = inputBuffer)
      fixed (Complex32* dst = fft.TimeData)
      fixed (float* win = window)
      {
        //shift data in buffer
        IppStatus rc = sp.ippsMove_32f(srcFloat + floatStep, srcFloat, oldCount);
        IppException.Check(rc);

        //add more data from ring
        ring.Read(inputBuffer, oldCount, floatStep);

        //todo: use Ipp
        Complex32* src = (Complex32*)srcFloat;
        //multiply by window, split-add
        //todo: this line is very slow!
        for (int i = 0; i < size; i++) dst[i] =
          src[i] * win[i] +
          src[size + i] * win[size + i] +
          src[size * 2 + i] * win[size * 2 + i] +
          src[size * 3 + i] * win[size * 3 + i];
      }

      //FFT
      fft.ComputeForward();
    }

    private void ComputeSpectrum()
    {
      if (Pause)
      {
        inputBuffer = null;
        return;
      }
      else if (inputBuffer == null)
        inputBuffer = new float[FftSize * Dsp.COMPONENTS_IN_COMPLEX * PresumeFactor];


      ComputeFft();

      //get copies of the slice parameters that change on the main thread
      lock (zoomLock)
      {
        inStart = InStart;
        inLength = InLength;
        outLength = OutLength;
        squeezeFactor = outLength / (float)inLength;
      }
      if (inLength < 1) return;

      //power in the selected slice
      float[] power = fft.SlicePower(inStart, inLength);

      //apply zoom
      if (newSpectrum == null || newSpectrum.Length != outLength)
        newSpectrum = new float[outLength];
      for (int i = 0; i < outLength; i++) newSpectrum[i] = 1e-30f;
      for (int i = 0; i < inLength; i++)
      {
        int dst = (int)(i * squeezeFactor);
        newSpectrum[dst] = Math.Max(power[i], newSpectrum[dst]);
      }

      //scale by median
      float median = ArrayStatistics.LowerQuartileInplace(newSpectrum.Clone() as float[]);
      if (median > 0)
      {
        float inverseMedian = 1 / median;
        for (int i = 0; i < outLength; i++) newSpectrum[i] *= inverseMedian;
      }

      //log
      IppHelper.PowerToLogPower(newSpectrum);

      //subtract median
      //float median = ArrayStatistics.LowerQuartileInplace(newSpectrum.Clone() as float[]);
      //for (int i = 0; i < outLength; i++) newSpectrum[i] -= median;

      spectra.Add(Smooth(newSpectrum));
    }

    private float[] Smooth(float[] newSpectrum)
    { 
      //noise floor
      float[] noise = newSpectrum.Clone() as float[];
      (new SlidingMax(6)).FilterArrayInplace(noise);
      (new SlidingMin(6)).FilterArrayInplace(noise);

      //non-linear filter: slow noise floor, fast signals
      float minAmp = 4;
      float maxAmp = 12;
      float maxGain = 0.8f;
      //squeezing smoothes the noise floor so less filtering is requied
      float minGain = (float)Math.Min(maxGain, 0.1 - 0.03 * Math.Log(squeezeFactor));

      float slope = (maxGain - minGain) / (maxAmp - minAmp);

      if (smoothedSpectrum == null || smoothedSpectrum.Length != outLength)
        smoothedSpectrum = (newSpectrum.Clone() as float[]);
      else
        for (int i = 0; i < outLength; i++)
        {
          float amp = Math.Max(newSpectrum[i], smoothedSpectrum[i]);
          amp -= noise[i];

          float gain;
          if (amp < minAmp) gain = minGain;
          else if (amp > maxAmp) gain = maxGain;
          else gain = minGain + slope * (amp - minAmp);

          smoothedSpectrum[i] += (newSpectrum[i] - smoothedSpectrum[i]) * gain;
        }

      return smoothedSpectrum.Clone() as float[];
    }

    public void ShiftSmoothedSpectrum(int dx)
    {
      if (smoothedSpectrum == null) return;
      int len = smoothedSpectrum.Length;
      if (dx == 0 || Math.Abs(dx) >= len) return;

      //todo: lock

      fixed (float* spect = smoothedSpectrum)
      {
        IppStatus rc;
        if (dx > 0) rc = sp.ippsMove_32f(spect, spect + dx, len - dx);
        else rc = sp.ippsMove_32f(spect-dx, spect, len + dx);
        IppException.Check(rc);
      }
    }
  }
}

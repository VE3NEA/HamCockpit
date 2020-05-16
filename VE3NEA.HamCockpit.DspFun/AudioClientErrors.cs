using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VE3NEA.HamCockpit.DspFun
{
  // https://www.hresult.info/FACILITY_AUDCLNT

  /// <summary>
  /// <a href="https://docs.microsoft.com/en-us/windows/win32/api/audioclient/">Audio Client</a> 
  /// error codes missing from <a href="https://archive.codeplex.com/?p=cscore">CSCore</a>.
  /// </summary>
  public enum AudclntErrorCodes
  {
    /// <exclude />
    AUDCLNT_E_NOT_INITIALIZED = unchecked ((int)0x88890001),
    /// <exclude />
    AUDCLNT_E_ALREADY_INITIALIZED,
    /// <exclude />
    AUDCLNT_E_WRONG_ENDPOINT_TYPE,
    /// <exclude />
    AUDCLNT_E_DEVICE_INVALIDATED,
    /// <exclude />
    AUDCLNT_E_NOT_STOPPED,
    /// <exclude />
    AUDCLNT_E_BUFFER_TOO_LARGE,
    /// <exclude />
    AUDCLNT_E_OUT_OF_ORDER,
    /// <exclude />
    AUDCLNT_E_UNSUPPORTED_FORMAT,
    /// <exclude />
    AUDCLNT_E_INVALID_SIZE,
    /// <exclude />
    AUDCLNT_E_DEVICE_IN_USE,
    /// <exclude />
    AUDCLNT_E_BUFFER_OPERATION_PENDING,
    /// <exclude />
    AUDCLNT_E_THREAD_NOT_REGISTERED,
    /// <exclude />
    AUDCLNT_E_NO_SINGLE_PROCESS,
    /// <exclude />
    AUDCLNT_E_EXCLUSIVE_MODE_NOT_ALLOWED,
    /// <exclude />
    AUDCLNT_E_ENDPOINT_CREATE_FAILED,
    /// <exclude />
    AUDCLNT_E_SERVICE_NOT_RUNNING,
    /// <exclude />
    AUDCLNT_E_EVENTHANDLE_NOT_EXPECTED,
    /// <exclude />
    AUDCLNT_E_EXCLUSIVE_MODE_ONLY,
    /// <exclude />
    AUDCLNT_E_BUFDURATION_PERIOD_NOT_EQUAL,
    /// <exclude />
    AUDCLNT_E_EVENTHANDLE_NOT_SET,
    /// <exclude />
    AUDCLNT_E_INCORRECT_BUFFER_SIZE,
    /// <exclude />
    AUDCLNT_E_BUFFER_SIZE_ERROR,
    /// <exclude />
    AUDCLNT_E_CPUUSAGE_EXCEEDED,
    /// <exclude />
    AUDCLNT_E_BUFFER_ERROR,
    /// <exclude />
    AUDCLNT_E_BUFFER_SIZE_NOT_ALIGNED
  }

  /// <summary>Returns error messages for the 
  /// <a href="https://docs.microsoft.com/en-us/windows/win32/api/audioclient/">Audio Client</a> 
  /// error codes.</summary>
  public class AudioClientErrors
  {
    private static readonly string[] Messages = {
      "The IAudioClient object is not initialized.",
      "The IAudioClient object is already initialized.",
      "The endpoint device is a capture device, not a rendering device.",
      "The audio device has been unplugged or otherwise made unavailable.",
      "The audio stream was not stopped at the time of the Start call.",
      "The buffer is too large.",
      "A previous GetBuffer call is still in effect.",
      "The specified audio format is not supported.",
      "The wrong NumFramesWritten value.",
      "The endpoint device is already in use.",
      "Buffer operation pending",
      "The thread is not registered.",
      "The session spans more than one process.",
      "Exclusive mode is disabled on the device.",
      "Failed to create the audio endpoint.",
      "The Windows audio service is not running.",
      "The audio stream was not initialized for event-driven buffering.",
      "Exclusive mode only.",
      "The hnsBufferDuration and hnsPeriodicity parameters are not equal.",
      "Event handle not set.",
      "Incorrect buffer size.",
      "Buffer size error.",
      "CPU usage exceeded.",
      "Buffer error",
      "Buffer size not aligned."
    };

    /// <summary>  Gets the error message for the specified error code.</summary>
    /// <param name="errorCode">The error code.</param>
    /// <returns>The error message.</returns>
    public static string Message(int errorCode)
    {
      try
      {
        return Messages[errorCode - (int)AudclntErrorCodes.AUDCLNT_E_NOT_INITIALIZED];
      }
      catch
      {
        return "Unknown error code: " + errorCode.ToString("x8");
      }
    }
  }
}

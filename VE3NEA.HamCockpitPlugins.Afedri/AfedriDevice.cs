using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VE3NEA.HamCockpit.DspFun;

namespace VE3NEA.HamCockpitPlugins.Afedri
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct DiscoveryMessage
  {
    public UInt16 Length;
    public UInt16 Key;
    public byte Op;
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
    public char[] Name;
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
    public char[] Sn;
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
    public byte[] IpAddr;
    public UInt16 Port;
    public byte CustomField;
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 6)]
    public byte[] MacAddr;
    public UInt16 HwVer;
    public UInt16 FwVer;
    public UInt16 BtVer;
    public byte FpgaId;
    public byte FpgaRev;
    public byte Opts;
    public byte Mode;
    public UInt16 SubNet;
    public UInt16 GwAddr;
    public UInt16 DataIpAddr;
    public UInt16 DataPort;
    public byte Fpga;
    public byte Status;
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 15)]
    public byte[] Future;

    public static DiscoveryMessage CreateFromBytes(byte[] bytes)
    {
      GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
      DiscoveryMessage result =
        (DiscoveryMessage)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
        typeof(DiscoveryMessage));
      handle.Free();
      return result;
    }
  }

  class AfedriDevice
  {
    public const int IQ_PACKET_HEADER_SIZE = 4;
    public DiscoveryMessage discoveryMessage;
    public int MaxCenterFrequency;

    //discovery
    private const int DISCOVERY_SERVER_PORT = 48321;
    private const int DISCOVERY_CLIENT_PORT = 48322;
    private const int DISCOVERY_SIGNATURE = 0xA55A;
    private const int DISCOVERY_OP_RESP = 1;
    private const int STATUS_BIT_CONNECTED = 1;
    private const int STATUS_BIT_RUNNING = 2;
    private const int DISCOVERY_REPLY_SIZE = 103;

    //NET commands
    private const int CI_RECEIVER_STATE = 0x18;
    private const int CI_FREQUENCY = 0x20;
    private const int CI_SAMPLE_RATE_CALIBRATION = 0xB0;
    private const int CI_DDC_SAMPLE_RATE = 0xB8;
    private const int CI_DATA_PACKET_SIZE = 0xC4;

    //HID ack reply
    private const int HID_GENERIC_REPLY = 6;

    //channel id's are not sequential
    private readonly byte[] ChannelIds = { 0,2,3,4 };

    private IPEndPoint afedriEndPoint;
    private TcpClient tcpClient;
    private UdpClient udpClient;


    public void Start(Settings settings)
    {
      DiscoverRadio();
      SetupRadio(settings);
      StartListeningToIq();
    }

    public void Stop()
    {
      try { udpClient.Close(); } catch { }
      try { tcpClient.Close(); } catch { }
      udpClient = null;
      tcpClient = null;
    }

    public bool IsActive()
    {
      return udpClient != null;
    }

    public int SetFrequency(long frequency, int channel)
    {
      try
      {
        frequency = Math.Max(0, Math.Min(MaxCenterFrequency, frequency));
        var command = new byte[] { 0x0a, 0x00, 0x20, 0x00, 0x00, 0x0e, 0xa7, 0x6b, 0x00, 0x00 };
        command[4] = ChannelIds[channel];
        var value = BitConverter.GetBytes((int)frequency).ToArray();
        Array.Copy(value, 0, command, 5, 4);
        tcpClient.GetStream().Write(command, 0, command.Length);
        ReceiveReply(CI_FREQUENCY);
      }
      catch (Exception e)
      {
        throw new Exception($"Afedri command CI_FREQUENCY failed:\n\n{e.Message}");

      }
      return (int)frequency;
    }

    public byte[] ReadIq()
    {
        return udpClient.Receive(ref afedriEndPoint).Skip(IQ_PACKET_HEADER_SIZE).ToArray();
    }


    private void DiscoverRadio()
    {
      try
      {
        byte[] discoveryMessageBytes = { 0x38, 0x00, 0x5a, 0xa5 };
        Array.Resize<byte>(ref discoveryMessageBytes, discoveryMessageBytes[0]);
        var remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, DISCOVERY_SERVER_PORT);

        //send discovery message through all interfaces
        //https://stackoverflow.com/questions/1096142
        foreach (var intf in NetworkInterface.GetAllNetworkInterfaces())
          if (intf.OperationalStatus == OperationalStatus.Up
            && intf.SupportsMulticast
            && intf.GetIPProperties().GetIPv4Properties() != null
            && NetworkInterface.LoopbackInterfaceIndex != intf.GetIPProperties().GetIPv4Properties().Index
            )
            foreach (var addr in intf.GetIPProperties().UnicastAddresses)
              if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
              {
                var localEndPoint = new IPEndPoint(addr.Address, DISCOVERY_CLIENT_PORT);
                using (var udpClient = new UdpClient(localEndPoint))
                {
                  udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                  udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
                  udpClient.Send(discoveryMessageBytes, discoveryMessageBytes.Length, remoteEndPoint);
                }
              }

        //receive reply
        var localEndpoint = new IPEndPoint(IPAddress.Any, DISCOVERY_CLIENT_PORT);
        using (var udpClient = new UdpClient(localEndpoint))
        {
          udpClient.Client.ReceiveTimeout = 300;
          var bytes = udpClient.Receive(ref remoteEndPoint);
          discoveryMessage = DiscoveryMessage.CreateFromBytes(bytes);
        }

        //validate reply
        if (discoveryMessage.Length != DISCOVERY_REPLY_SIZE
          || discoveryMessage.Key != DISCOVERY_SIGNATURE
          || discoveryMessage.Op != DISCOVERY_OP_RESP
          )
          throw (new Exception("invalid reply from the radio"));

        if ((discoveryMessage.Status & (STATUS_BIT_CONNECTED | STATUS_BIT_RUNNING)) != 0)
          throw (new Exception("radio already in use"));
      }
      catch (Exception e)
      {
        throw new Exception($"Afedri SDR not found.\n\n{e.Message}");
      }
    }

    private void SetupRadio(Settings settings)
    {
      byte[] command, reply, value;
      string commandName = "";

      try
      {
        //connect
        tcpClient = new TcpClient();
        tcpClient.Client.SendTimeout = 1000;
        tcpClient.Client.ReceiveTimeout = 1000;
        var afedriIp = new IPAddress(discoveryMessage.IpAddr.Take(4).Reverse().ToArray());
        afedriEndPoint = new IPEndPoint(afedriIp, discoveryMessage.Port);
        tcpClient.Connect(afedriEndPoint);

        //set channels
        commandName = "HID_GENERIC_SET_MULTICHANNEL_COMMAND";
        command = new byte[] { 0x09, 0xe0, 0x02, 0x30, 0xFF, 0x00, 0x00, 0x00, 0x00 };
        command.SetValue(settings.MultichannelMode, 4);
        tcpClient.GetStream().Write(command, 0, command.Length);
        ReceiveReply(HID_GENERIC_REPLY);

        //get clock rate
        commandName = "CI_SAMPLE_RATE_CALIBRATION";
        command = new byte[] { 0x04, 0x20, 0xB0, 0x00 };
        tcpClient.GetStream().Write(command, 0, command.Length);
        reply = ReceiveReply(CI_SAMPLE_RATE_CALIBRATION);
        var clockRate = BitConverter.ToInt32(reply, 5);
        MaxCenterFrequency = clockRate / 2;

        //set sampling rate
        commandName = "CI_DDC_SAMPLE_RATE";
        command = new byte[] { 0x09, 0x00, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        settings.SamplingRate = ComputeValidSamplingRate(settings, clockRate);
        value = BitConverter.GetBytes((int)settings.SamplingRate).ToArray();
        Array.Copy(value, 0, command, 5, 4);
        tcpClient.GetStream().Write(command, 0, command.Length);
        reply = ReceiveReply(CI_DDC_SAMPLE_RATE);

        //set frequencies
        commandName = "CI_FREQUENCY";
        for (int ch = 0; ch < settings.FrequencyCount(); ch++)
          settings.Frequencies[ch] = SetFrequency(settings.Frequencies[ch], ch);

        //set packet size
        commandName = "CI_DATA_PACKET_SIZE";
        command = new byte[] { 0x05, 0x00, 0xc4, 0x00, 0x00 };
        tcpClient.GetStream().Write(command, 0, command.Length);
        ReceiveReply(CI_DATA_PACKET_SIZE);

        //enable agc
        commandName = "HID_GENERIC_SET_OVR_MODE";
        command = new byte[] { 0x09, 0xe0, 0x02, 0x45, 0x0f, 0x00, 0x00, 0x00, 0x00 };
        tcpClient.GetStream().Write(command, 0, command.Length);
        ReceiveReply(HID_GENERIC_REPLY);

        //start I/Q feed
        //0000   
        commandName = "CI_RECEIVER_STATE";
        command = new byte[] { 0x08, 0x00, 0x18, 0x00, 0x80, 0x02, 0x00, 0x00 };
        tcpClient.GetStream().Write(command, 0, command.Length);
        ReceiveReply(CI_RECEIVER_STATE);
      }
      catch (Exception e)
      {
        throw new Exception($"Afedri command {commandName} failed:\n\n{e.Message}");
      }
    }

    private void StartListeningToIq()
    {
      var localEndpoint = new IPEndPoint(IPAddress.Any, discoveryMessage.Port);
      udpClient = new UdpClient(localEndpoint);
      udpClient.Client.ReceiveTimeout = 1000;
    }


    private byte[] ReceiveReply(byte cmd)
    {
      var tcpStream = tcpClient.GetStream();

      //read length
      var reply = new byte[1];
      int cnt = tcpStream.Read(reply, 0, 1);
      if (cnt != 1 || reply[0] < 3) throw new Exception("Invalid reply from the radio");
      var len = reply[0];

      //read data
      Array.Resize<byte>(ref reply, len);
      cnt += tcpStream.Read(reply, 1, len-1);
      if (cnt != len || reply[2] != cmd) throw new Exception();

      return reply;
    }

    private const int BITS_PER_SAMPLE = 16 * Dsp.COMPONENTS_IN_COMPLEX;
    private const int MIN_SAMPLING_RATE = 96000; //TODO: is it a constant or clockRate/BITS_PER_SAMPLE/25 ?
    private const int FIR_DECIMATION_FACTOR = 4;

    private int ComputeValidSamplingRate(Settings settings, int clockRate)
    {
      int maxSamplingRate = clockRate / (BITS_PER_SAMPLE * settings.ChannelCount());
      int cicDecimationFactor = (int)Math.Round((clockRate / FIR_DECIMATION_FACTOR) / (double)settings.SamplingRate);
      int correctedRate = clockRate / (cicDecimationFactor * FIR_DECIMATION_FACTOR);
      return Math.Max(MIN_SAMPLING_RATE, Math.Min(maxSamplingRate, correctedRate));
    }




    /* HOWTO: connect Afedri SDR directly to an Ethernet card in a PC:
     
     //connect Afedri SDR to the PC Ethernet card, then run command:
     ipconfig.exe
     //example output:
    ------------------------------------------------------------------------
    Ethernet adapter Ethernet:

    Connection-specific DNS Suffix  . :
    Link-local IPv6 Address . . . . . : fe80::e86c:b3a5:7419:6881%20
    IPv4 Address. . . . . . . . . . . : 169.254.104.132
    Subnet Mask . . . . . . . . . . . : 255.255.255.0
    Default Gateway . . . . . . . . . :
    ------------------------------------------------------------------------
    //choose IP address for afedri based on ip and subnet mask of the Ethernet card
    //e.g., 169.254.104.8
    //connect Afedri SDR to PC via USB
    //change IP address of Afedri using SDR_control.exe
    //  - start SDR_control
    //  - uncheck Enable Network Interface
    //  - restart SDR_control
    //  - check Edit Network Parameters
    //  - enter new IP and click on Save
    //  - cycle the power of Afedri
    //connect Afedri to the Ethernet card

    //to connect Afedri to a router, change its IP address back to 192.168.0.8
    */
  }
}

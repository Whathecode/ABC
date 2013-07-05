
using ABC.Infrastructure.Driver;
using ABC.Model.Device;
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using ABC.Model.Primitives;
using LibUsbDotNet.DeviceNotify;

namespace ABC.Infrastructure.Drivers
{
    public delegate void RfidDataReceivedHandler(object sender, RfdiDataReceivedEventArgs e);
    public class HyPrDevice:Device
    {
        public event RfidDataReceivedHandler RfidDataReceived = null;
        public event EventHandler RfidResetReceived = null;

        public string Port { get; private set; }
        public string CurrentRfid { get; private set; }

        private const string HandShakeCommand = "A";
        private const string HandShakeReply = "B";
        private const int BaudRate = 9600;
        private const int ReadDelay = 100; //ms
        private const int ReadTimeOut = 200; //ms

        private SafeSerialPort _serialPort;
        private string _output;

        private readonly IDeviceNotifier _usbDeviceNotifier = DeviceNotifier.OpenDeviceNotifier();

        public HyPrDevice()
        {
            Connect();
            _usbDeviceNotifier.OnDeviceNotify += UsbDeviceNotifier_OnDeviceNotify;
        }

        private void Connect()
        {

            var port = FindDevice();
            if (port != null)
            {
                ConnectToDevice(port);
            }
            else
                Console.WriteLine("No HyPR Device found");
        }
        private void ResetConnection()
        {
            try
            {
                if(_serialPort !=null)
                    _serialPort.Write("Any value");
            }
            catch (IOException)
            {
                if (_serialPort == null) return;
                _serialPort.Dispose();
                _serialPort.Close();
            }
        }
        void UsbDeviceNotifier_OnDeviceNotify(object sender, DeviceNotifyEventArgs e)
        {
            if (e.Object.ToString().Split('\n')[1].Contains("0x2341"))
            {
                if (e.EventType == EventType.DeviceArrival)
                {
                    Connect();
                }
                else if (e.EventType == EventType.DeviceRemoveComplete)
                {
                    ResetConnection();
                }
            }
        }

        private string FindDevice()
        {
            var ports = SerialPort.GetPortNames();
            foreach (var portname in ports)
            {
                Console.WriteLine("Attempt to connect to {0}", portname);
                var sp = new SerialPort(portname, BaudRate);
                try
                {
                    sp.ReadTimeout = ReadTimeOut;
                    sp.Open();
                    sp.Write(HandShakeCommand);
                    Thread.Sleep(ReadDelay);

                    string received = sp.ReadExisting();

                    if (received == HandShakeReply)
                        return portname;
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
                finally
                {
                    sp.Close();
                }
            }
            return null;
        }
        public HyPrDevice(string port)
        {
            ConnectToDevice(port);
        }

        private void ConnectToDevice(string port)
        {
            Port = port;
            ConnectToHyPrDevice(port);
        }
        
        ~HyPrDevice()
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                    _serialPort.Dispose();
                }
            }
        }

        private void ConnectToHyPrDevice(string portname)
        {
            try
            {
                _serialPort = null;
                _serialPort = new SafeSerialPort(portname, BaudRate);
                _serialPort.DataReceived += serialPort_DataReceived;
                _serialPort.Open();
                Console.WriteLine("Found HyPR device at: " + portname);
            }
            catch (Exception ex)
            {
                Console.WriteLine("NOT connected to: " + portname);
                Console.WriteLine(ex.ToString());
            }
        }
        public void UpdateColor(Rgb color)
        {
            WriteToDevice(color.ToString());
        }
        private void WriteToDevice(string msg)
        {
            try
            {
                if(_serialPort != null)
                    if(_serialPort.IsOpen)
                        _serialPort.Write(msg);
            }
            catch (IOException)
            {
                ResetConnection();
                //var success  = PortHelper.TryResetPortByName(Port);
                //Thread.Sleep(10000);
                //ConnectToDevice(Port);
            }
        }


        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _output += _serialPort.ReadExisting();

            if (_output.EndsWith("#"))
            {
                if (!_output.Contains("RESET#"))
                {
                    CurrentRfid = _output;
                    OnRfidDataReceived(new RfdiDataReceivedEventArgs(_output));
                }
                else
                    OnRfidResetReceived(new EventArgs());
               // Console.WriteLine("Received:\t" + output);
                _output = "";
            }
        }
        protected void OnRfidDataReceived(RfdiDataReceivedEventArgs e)
        {
            if (RfidDataReceived != null)
                RfidDataReceived(this, e);
        }
        protected void OnRfidResetReceived(EventArgs e)
        {
            if (RfidResetReceived != null)
                RfidResetReceived(this, e);
        }
    }
    public class RfdiDataReceivedEventArgs:EventArgs
    {
        public string Rfid{get;set;}
        public RfdiDataReceivedEventArgs(string rfid)
        {
            Rfid=rfid;
        }
    }
}

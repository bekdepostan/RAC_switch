﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using System.Threading;
using System.Net;

namespace Funk_switch
{
    /// <summary>
    /// class to query or watch connection state (based on IP)
    /// </summary>
    public class network:IDisposable
    {
        Thread backgroundThread;
        bool _isConnected = false;
        bool _bStopThread = false;
        static IPAddress _currentIP = IPAddress.Loopback;
        static MobileConfiguration _myConfig = new MobileConfiguration();
        static wifi _wifi = null;

        public network()
        {
            _wifi = new wifi();
            backgroundThread = new Thread(new ThreadStart(myThread));
            backgroundThread.Start();
        }
        public void Dispose()
        {
            if (backgroundThread != null)
            {
                _bStopThread = true;
                Thread.Sleep(1000);
                if (backgroundThread != null)
                    backgroundThread.Abort();
            }
            if (_wifi != null)
            {
                _wifi.Dispose();
                _wifi = null;
            }
        }


        public delegate void networkChangeEventHandler(object sender, NetworkEventArgs eventArgs);
        public event networkChangeEventHandler networkChangedEvent; 
        public enum NetworkEventType
        {
            None,
            Connected,
            Disconnected,
        }
        public class NetworkEventArgs : EventArgs
        {
            public NetworkEventType EventReason;
            public IPAddress ipAddress;
            public NetworkEventArgs()
            {
                EventReason = NetworkEventType.None;
                ipAddress = IPAddress.Loopback;
            }
            public NetworkEventArgs(IPAddress _ipAddress, NetworkEventType _eventType)
            {
                EventReason = _eventType;
                ipAddress = _ipAddress;
            }
        }
        void OnNetworkConnected(IPAddress _ip, NetworkEventType _type)
        {
            NetworkEventArgs e = new NetworkEventArgs(_ip, _type);
            if (networkChangedEvent != null)
                networkChangedEvent(this, e);
        }

        /// <summary>
        /// thread to watch the IP based connection state
        /// </summary>
        void myThread()
        {
            int iCount = 0;
            bool currentState;
            try
            {
                while (!_bStopThread)
                {
                    if(iCount==10){ //every 10 seconds
                        currentState = _getConnected();
                        if (_isConnected != currentState)
                        {
                            if(currentState)
                                OnNetworkConnected(_currentIP, NetworkEventType.Connected);
                            else
                                OnNetworkConnected(_currentIP, NetworkEventType.Disconnected);
                            currentState = _isConnected;
                        }
                        iCount = 0;
                    }
                    iCount++;
                    Thread.Sleep(1000);
                }
            }
            catch (ThreadAbortException ex)
            {
                System.Diagnostics.Debug.WriteLine("myThread ThreadAbortException: " + ex.Message);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("myThread Exception: " + ex.Message); }
            finally
            {
                ;
            }
        }

        /// <summary>
        /// returns TRUE for device having a valid IP
        /// </summary>
        /// <returns>boolean</returns>
        public static bool _getConnected()
        {
            if (_myConfig._checkConnectIP)
            {
                string hostname = Dns.GetHostName(); //local host name
                IPHostEntry _hostEntry = Dns.GetHostEntry(hostname);
                bool bLocalsOnly = true;
                foreach (IPAddress a in _hostEntry.AddressList)
                {
                    System.Diagnostics.Debug.WriteLine(a.ToString()); // 169.254.2.1, 127.0.0.1
                    if (a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
#if DEBUG
                    if (a.ToString() != "127.0.0.1")
#else
                        if (a.ToString() != "169.254.2.1" && a.ToString() != "127.0.0.1")
#endif
                        {
                            bLocalsOnly = false;
                            _currentIP = a;
                        }
                    }
                }
                if (bLocalsOnly)
                    return false;
                else
                    return true;
            }
            else
            {
                string ssid = _wifi.getCurrentProfile().sSSID;
                if (wifi.isAssociated(ssid))
                    return true;
                else
                    return false;
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using wcl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media;
using System.ComponentModel;
using Dynadimmer.Models.Messages;
using System.Collections.ObjectModel;
using System.Windows;
using Dynadimmer.Views.DateTime;
using Dynadimmer.Views.SummerWinnter;
using Dynadimmer.Views.Schedulers.Inner;

namespace Dynadimmer.Models
{
    public class IRDACummunication : INotifyPropertyChanged
    {
        private bool _TestMode;
        public bool TestMode
        {
            get { return _TestMode; }
            set
            {
                _TestMode = value;
                NotifyPropertyChanged("TestMode");
            }
        }
        
        #region Private Properties
        wclAPI _wclAPI;
        wclClient _wclClient;
        wclIrDADiscovery _wclIrDADiscovery;
        List<wclIrDADevice> Devices;

        UnitProperty property;
        List<byte> answer = new List<byte>();
        DispatcherTimer ConnectionTimer = new DispatcherTimer();
        DispatcherTimer FillAnswerTimer = new DispatcherTimer();
        #endregion

        #region Commands
        public MyCommand Connect { get; set; }
        public MyCommand Exit { get; set; }
        #endregion

        #region Events
        public event EventHandler Connected;
        #endregion

        #region UI Propeerties
        private ObservableCollection<UnitMessage> messages;
        public ObservableCollection<UnitMessage> Messages
        {
            get { return messages; }
            set
            {
                messages = value;
                NotifyPropertyChanged("Messages");
            }
        }

        private bool windowenable;
        public bool WindowEnable
        {
            get { return windowenable; }
            set
            {
                windowenable = value;
                NotifyPropertyChanged("WindowEnable");
            }
        }

        private string connectioninfo;
        public string ConnectionInfo
        {
            get { return connectioninfo; }
            set
            {
                connectioninfo = value;
                NotifyPropertyChanged("ConnectionInfo");
            }
        }

        private Brush connectioninfocolor;
        public Brush ConnectionInfoColor
        {
            get { return connectioninfocolor; }
            set
            {
                connectioninfocolor = value;
                NotifyPropertyChanged("ConnectionInfoColor");
            }
        }


        private string messageInfo;
        public string MessageInfo
        {
            get { return messageInfo; }
            set
            {
                messageInfo = value;
                NotifyPropertyChanged("MessageInfo");
            }
        }

        private Brush messageinfocolor;
        public Brush MessageInfoColor
        {
            get { return messageinfocolor; }
            set
            {
                messageinfocolor = value;
                NotifyPropertyChanged("MessageInfoColor");
            }
        }

        private string connectionbuttentext;
        public string ConnectionButtenText
        {
            get { return connectionbuttentext; }
            set
            {
                connectionbuttentext = value;
                NotifyPropertyChanged("ConnectionButtenText");
            }
        }

        private Color connectionbuttencolor;
        public Color ConnectionButtenColor
        {
            get { return connectionbuttencolor; }
            set
            {
                connectionbuttencolor = value;
                NotifyPropertyChanged("ConnectionButtenColor");
            }
        }

        private bool isconnected;
        public bool IsConnected
        {
            get { return isconnected; }
            set
            {
                isconnected = value;
                ConnectionButtenColor = isconnected ? Colors.LightCoral : Colors.LightGreen;
                ConnectionButtenText = isconnected ? "Disconnect" : "Connect";
                WindowEnable = isconnected;
                NotifyPropertyChanged("IsConnected");
            }
        }

        private bool _UnitClockChecked;
        public bool UnitClockChecked
        {
            get { return _UnitClockChecked; }
            set
            {
                _UnitClockChecked = value;
                UnitTimeVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                if (value)
                {
                    SummerWinterChecked = false;
                    ConfigChecked = false;
                }
                NotifyPropertyChanged("UnitClockChecked");
            }
        }

        private bool _SummerWinterChecked;
        public bool SummerWinterChecked
        {
            get { return _SummerWinterChecked; }
            set
            {
                _SummerWinterChecked = value;
                SummerWinterVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                if (value)
                {
                    UnitClockChecked = false;
                    ConfigChecked = false;
                }
                NotifyPropertyChanged("SummerWinterChecked");
            }
        }

        private bool _ConfigChecked;
        public bool ConfigChecked
        {
            get { return _ConfigChecked; }
            set
            {
                _ConfigChecked = value;
                ConfigVisibility = value ? Visibility.Visible : Visibility.Collapsed;
                if (value)
                {
                    UnitClockChecked = false;
                    SummerWinterChecked = false;
                }
                NotifyPropertyChanged("ConfigChecked");
            }
        }

        private Visibility _UnitTimeVisibility;
        public Visibility UnitTimeVisibility
        {
            get { return _UnitTimeVisibility; }
            set
            {
                _UnitTimeVisibility = value;
                NotifyPropertyChanged("UnitTimeVisibility");
            }
        }

        private Visibility _SummerWinterVisibility;
        public Visibility SummerWinterVisibility
        {
            get { return _SummerWinterVisibility; }
            set
            {
                _SummerWinterVisibility = value;
                NotifyPropertyChanged("SummerWinterVisibility");
            }
        }

        private Visibility _ConfigVisibility;
        public Visibility ConfigVisibility
        {
            get { return _ConfigVisibility; }
            set
            {
                _ConfigVisibility = value;
                NotifyPropertyChanged("ConfigVisibility");
            }
        }


        #endregion

        public IRDACummunication()
        {
            Messages = new ObservableCollection<UnitMessage>();
            IsConnected = false;
            Connect = new MyCommand();
            Connect.CommandSent += Connect_CommandSent;
            Exit = new MyCommand();
            Exit.CommandSent += Exit_CommandSent;
            _wclAPI = new wclAPI();
            _wclClient = new wclClient();
            _wclIrDADiscovery = new wclIrDADiscovery();

            _wclAPI.AfterLoad += new EventHandler(AfterLoad);
            _wclAPI.AfterUnload += new EventHandler(AfterUnload);
            _wclAPI.OnChanged += new EventHandler(OnChanged);

            _wclIrDADiscovery.OnComplete += new wcl.wclIrDACompleteEventHandler(OnComplete);
            _wclIrDADiscovery.OnStarted += new System.EventHandler(OnStarted);

            _wclClient.OnDisconnect += new System.EventHandler(OnDisconnect);
            _wclClient.OnData += new wcl.wclDataEventHandler(OnData);
            _wclClient.OnConnect += new wcl.wclConnectEventHandler(OnConnect);

            ConnectionTimer.Tick += t_Elapsed;
            ConnectionTimer.Interval = TimeSpan.FromMilliseconds(10000);


            FillAnswerTimer.Tick += FillAnswerTimer_Elapsed;
            FillAnswerTimer.Interval = TimeSpan.FromMilliseconds(500);

            UnitClockChecked = false;
            SummerWinterChecked = false;
            ConfigChecked = true;
            WindowEnable = false;
        }

        #region Send and Recieved

        private void FillAnswerTimer_Elapsed(object sender, EventArgs e)
        {
            FillAnswerTimer.Stop();
            WindowEnable = true;

            while (answer.Contains(1))
            {
                int startindex = answer.FindIndex(x => x == 1);
                if (startindex != 0)
                {
                    Messages.Insert(0, new JunkMessage(answer.GetRange(0, startindex)));
                    answer.RemoveRange(0, startindex);
                }
                if (answer.Contains(3))
                {
                    int endindex = answer.FindIndex(x => x == 3);
                    byte[] answer1 = answer.GetRange(0, endindex + 1).ToArray();
                    Console.WriteLine("startindex: " + startindex + "\n" + "endindex: " + endindex + "\n" + string.Join(" ", answer1));
                    try {
                        IncomeMessage mess = new IncomeMessage(string.Format("Recived {0}", property.Title), answer1.ToList());
                        property.GotAnswer(mess);
                        Messages.Insert(0, mess);
                    }
                    catch
                    {
                        MessageBox.Show(string.Join(" ", answer1));
                    }

                    answer.RemoveRange(0, endindex + 1);
                }
            }
            if(answer.Count>0)
            {
                Messages.Insert(0, new JunkMessage(answer));
                answer.Clear();
            }

            /*try
            {

                int endindex = answer.FindIndex(x => x == 3);
                answer = answer.GetRange(0, endindex - startindex + 1);
                IncomeMessage mess = new IncomeMessage(string.Format("Recived {0}", property.Title), answer.ToList());
                property.GotAnswer(mess);
                Messages.Insert(0, mess);
                answer = new List<byte>();
            }
            catch
            {
                property.DidntGotAnswer();
                MessageBox.Show(string.Join(" ", answer.ToArray()));
                answer = new List<byte>();
                SetMessageInfo("Incorrect message format.", Brushes.Red);
            }*/
        }

        private void SetMessageInfo(string text, Brush color)
        {
            MessageInfo = text;
            MessageInfoColor = color;
        }

        public void Write(UnitProperty prop, OutMessage outMessage)
        {
            FillAnswerTimer.Start();
            WindowEnable = false;
            SetMessageInfo("", Brushes.Black);
            Messages.Insert(0, outMessage);
            property = prop;
            _wclClient.Write(outMessage.DataAscii, (uint)outMessage.DataAscii.Length);
        }

        #endregion

        #region Client
        private void OnConnect(object sender, wclConnectEventArgs e)
        {
            ConnectionTimer.Stop();
            WindowEnable = true;
            if (e.Error != wcl.wclErrors.WCL_E_SUCCESS)
            {
                IsConnected = false;
                SetConnectionInfo("Unable connect: " + e.Error.ToString(), Brushes.Black, true);
            }
            else
            {
                IsConnected = true;
                SetConnectionInfo("Connected.", Brushes.Black, true);
                Connected(null, null);
            }
        }

        private void OnData(object sender, wclDataEventArgs e)
        {
            FillAnswerTimer.Stop();
            FillAnswerTimer.Start();
            answer.AddRange(e.Data);
        }

        private void OnDisconnect(object sender, EventArgs e)
        {
            IsConnected = false;
            SetConnectionInfo("DisConnected.", Brushes.Black, true);
        }
        #endregion

        #region Discovery
        private void OnStarted(object sender, EventArgs e)
        {
            SetConnectionInfo("Looking for Devices...", Brushes.Black, false);
        }

        private void OnComplete(object sender, wclIrDACompleteEventArgs e)
        {
            if (e.Devices == null)
            {
                ConnectionTimer.Stop();
                SetConnectionInfo("Complete with error!", Brushes.Red, true);
                return;
            }
            if (e.Devices.Count == 0)
            {
                ConnectionTimer.Stop();
                SetConnectionInfo("Nothing found!", Brushes.Red, true);
                return;
            }
            Devices = new List<wclIrDADevice>();
            for (UInt32 i = 0; i < e.Devices.Count; i++)
            {
                Devices.Add(e.Devices[i]);
            }
            SetConnectionInfo("Connecting...", Brushes.Black, false);
            _wclClient.IrDAParams.Address = Devices[0].Address;
            _wclClient.Transport = wcl.wclTransport.trIrDA;

            wcl.wclErrors.wclShowError(_wclClient.Connect()).ToString();
        }
        #endregion

        #region API
        private void AfterUnload(object sender, EventArgs e)
        {
        }

        private void AfterLoad(object sender, EventArgs e)
        {
            _wclIrDADiscovery.Discovery();
        }

        private void OnChanged(object sender, EventArgs e)
        {
            if (_wclAPI.Active)
            {
                SetConnectionInfo("APi is Active", Brushes.Black, true);
            }
            else
            {
                SetConnectionInfo("API is not active", Brushes.Red, true);
            }
        }
        #endregion

        #region Event Handler
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Connection
        private void Exit_CommandSent(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Connect_CommandSent(object sender, EventArgs e)
        {
            CheckStatus();
        }

        private void CheckStatus()
        {
            if (_wclClient.State == wclClientState.csConnecting)
            {
                _wclClient.Disconnect();
            }
            else if (_wclClient.State == wclClientState.csConnected)
            {
                _wclClient.Disconnect();
            }
            else
            {
                TryToConnect();
            }
        }

        private void TryToConnect()
        {
            ConnectionTimer.Start();
            WindowEnable = false;
            SetConnectionInfo("Connecting...", Brushes.Black, false);
            if (_wclAPI.Active)
            {
                if (_wclIrDADiscovery.Active)
                {
                    _wclClient.Connect();
                }
                else
                {
                    _wclIrDADiscovery.Discovery();
                }
            }
            else
            {
                _wclAPI.Load();
            }
        }

        private void SetConnectionInfo(string text, Brush color, bool v2)
        {
            ConnectionInfo = text;
            ConnectionInfoColor = color;
        }

        private void t_Elapsed(object sender, EventArgs e)
        {
            ConnectionTimer.Stop();
            WindowEnable = true;
            SetConnectionInfo("Failed to connect", Brushes.Red, true);
        }

        public void Dispose()
        {
            _wclClient.Dispose();
            _wclIrDADiscovery.Dispose();
            _wclAPI.Dispose();
        }

        #endregion
    }
}

﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlowLib.Containers;
using FlowLib.Connections;
using FlowLib.Interfaces;
using System.Threading;
using FlowLib.Events;
using FlowLib.Containers.Security;
using FlowLib.Protocols.HubNmdc;
using TestingBeforeRelease.Utils;

namespace TestingBeforeRelease
{
    [TestClass]
    public class SendAndReceiveMessage : IBaseUpdater
    {
        #region IBaseUpdater Members
        public event FlowLib.Events.FmdcEventHandler UpdateBase;
        #endregion

        private bool _isFinished;
        private int _regMode = -1;
        private bool _gotMyMainMessage;
        private bool _gotMyPrivateMessage;
        private HubSetting _settings;

        [TestMethod]
        public void SendAndReceiveMessage_YnHub_UsingAutoProtocol()
        {
            HubTest("Auto");

            if (!_gotMyMainMessage)
                throw new AssertFailedException("Unable to get my own main message using Auto Protocol (Nmdc)");
            if (!_gotMyPrivateMessage)
                throw new AssertFailedException("Unable to get my own private message using Auto Protocol (Nmdc)");
        }

        [TestMethod]
        public void SendAndReceiveMessage_YnHub_UsingNmdcProtocol()
        {
            HubTest("Nmdc");

            if (!_gotMyMainMessage)
                throw new AssertFailedException("Unable to get my own main message using Nmdc Protocol");
            if (!_gotMyPrivateMessage)
                throw new AssertFailedException("Unable to get my own private message using Nmdc Protocol");
        }

        [TestMethod]
        public void SendAndReceiveMessage_Adc_UsingAutoProtocol()
        {
            _settings.Address = "127.0.0.1";
            _settings.Port = 2780;
            HubTest("Auto");

            if (!_gotMyMainMessage)
                throw new AssertFailedException("Unable to get my own main message using Auto Protocol (Adc)");
            if (!_gotMyPrivateMessage)
                throw new AssertFailedException("Unable to get my own private message using Auto Protocol (Adc)");
        }

        [TestMethod]
        public void SendAndReceiveMessage_Adcs_UsingAdcsProtocol()
        {
			_settings.Address = "devpublic.adcportal.com";
			_settings.Port = 16591;
			//_settings.Address = "127.0.0.1";
			//_settings.Port = 2781;
            HubTest("AdcSecure");

            if (!_gotMyMainMessage)
                throw new AssertFailedException("Unable to get my own main message using Adcs Protocol");
            if (!_gotMyPrivateMessage)
                throw new AssertFailedException("Unable to get my own private message using Adcs Protocol");
        }

        [TestInitialize()]
        public void Init()
        {
            Application.InitilizeAll();

            UpdateBase = new FlowLib.Events.FmdcEventHandler(SendMainChatOrPMToHub_UpdateBase);

            _settings = new HubSetting();
            _settings.Address = "127.0.0.1";
            _settings.Port = 411;
            _settings.DisplayName = "FlowLib";
            _settings.Password = "Password";
        }

        [TestCleanup()]
        public void CleanUp()
        {
            _isFinished = false;
            _regMode = -1;

            _gotMyMainMessage = false;
            _gotMyPrivateMessage = false;
        }

        private void HubTest(string protocol)
        {
            _settings.Protocol = protocol;

            Hub hubConnection = new Hub(_settings, this);
            hubConnection.SecureUpdate += new FmdcEventHandler(hubConnection_SecureUpdate);
            hubConnection.ProtocolChange += new FmdcEventHandler(hubConnection_ProtocolChange);

            hubConnection.Connect();

            int i = 0;
            while (!_isFinished && i++ < 20)
            {
                Thread.Sleep(500);
            }

            hubConnection.Disconnect("Test time exceeded");
            hubConnection.Dispose();
        }

        void hubConnection_ProtocolChange(object sender, FmdcEventArgs e)
        {
            Hub hubConnection = sender as Hub;
            if (hubConnection != null)
            {
                hubConnection.Protocol.Update += new FmdcEventHandler(prot_Update);
            }
        }

        void prot_Update(object sender, FmdcEventArgs e)
        {
            Hub hubConnection = sender as Hub;
            if (hubConnection != null)
            {
                bool testFinishStatus = false;
                switch (e.Action)
                {
                    case Actions.IsReady:
                        bool isReady = (bool)e.Data;
                        if (isReady)
                        {
                            // Create mainchat message.
                            MainMessage msg = new MainMessage(hubConnection.Me.ID, "Testing - MainMessage");
                            // message will here be converted to right format and then be sent.
                            UpdateBase(this, new FlowLib.Events.FmdcEventArgs(FlowLib.Events.Actions.MainMessage, msg));

                            // Create private message.
                            PrivateMessage privMsg = new PrivateMessage(hubConnection.Me.ID, hubConnection.Me.ID, "Testing - PrivateMessage");

                            // message will here be converted to right format and then be sent.
                            UpdateBase(this, new FlowLib.Events.FmdcEventArgs(FlowLib.Events.Actions.PrivateMessage, privMsg));

                        }
                        break;
                    case Actions.MainMessage:
                        MainMessage msgMain = e.Data as MainMessage;
                        if (string.Equals(hubConnection.Me.ID, msgMain.From))
                        {
                            _gotMyMainMessage = true;
                        }
                        testFinishStatus = true;
                        break;
                    case Actions.PrivateMessage:
                        PrivateMessage msgPriv = e.Data as PrivateMessage;
                        if (string.Equals(hubConnection.Me.ID, msgPriv.From))
                        {
                            _gotMyPrivateMessage = true;
                        }
                        testFinishStatus = true;
                        break;
                }

                if (testFinishStatus)
                {
                    if (_gotMyMainMessage && _gotMyPrivateMessage)
                    {
                        _isFinished = true;
                    }
                }
            }

        }

        void hubConnection_SecureUpdate(object sender, FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.SecurityValidateRemoteCertificate:
                    CertificateValidationInfo ct = e.Data as CertificateValidationInfo;
                    if (ct != null)
                    {
                        ct.Accepted = true;
                        e.Data = ct;
                        e.Handled = true;
                    }
                    break;
            }
        }

        void SendMainChatOrPMToHub_UpdateBase(object sender, FlowLib.Events.FmdcEventArgs e) { }
    }
}

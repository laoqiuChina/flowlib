
/*
 *
 * Copyright (C) 2009 Mattias Blomqvist, patr-blo at dsv dot su dot se
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 *
 */

using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Protocols;
using FlowLib.Events;
using FlowLib.Interfaces;
using FlowLib.Managers;
using FlowLib.Utils.FileLists;

#if !COMPACT_FRAMEWORK
// Security, Windows Mobile doesnt support SSLStream so we disable this feature for it.
using FlowLib.Containers.Security;
using System.Security.Cryptography.X509Certificates;
#endif

namespace ConsoleDemo.Examples
{
    public class PassiveDownloadFilelistFromUserUsingTLS : IBaseUpdater
    {
        public event FmdcEventHandler UpdateBase;

        TransferManager transferManager = new TransferManager();
        DownloadManager downloadManager = new DownloadManager();
        //string currentDir = @"C:\Temp\";
        string currentDir = System.AppDomain.CurrentDomain.BaseDirectory;
        bool sentRequest = false;

        public PassiveDownloadFilelistFromUserUsingTLS()
        {
            UpdateBase = new FmdcEventHandler(PassiveConnectToUser_UpdateBase);

            // Creates a empty share
            Share share = new Share("Testing");
            // Adds common filelist to share
            AddFilelistsToShare(share);

            HubSetting setting = new HubSetting();
            setting.Address = "127.0.0.1";
            setting.Port = 411;
            setting.DisplayName = "FlowLibPassiveTLS";
            setting.Protocol = "Auto";

            Hub hubConnection = new Hub(setting, this);
            hubConnection.ProtocolChange += new FmdcEventHandler(hubConnection_ProtocolChange);
            // Adds share to hub
            hubConnection.Share = share;
            hubConnection.Me.Set(UserInfo.SECURE, "");
            hubConnection.Connect();
        }

        void PassiveConnectToUser_UpdateBase(object sender, FmdcEventArgs e) { }

        void hubConnection_ProtocolChange(object sender, FmdcEventArgs e)
        {
            Hub hubConnection = sender as Hub;
            IProtocol prot = e.Data as IProtocol;
            if (prot != null)
            {
                prot.Update -= hubConnection_Update;
            }
            hubConnection.Protocol.Update += new FmdcEventHandler(hubConnection_Update);
        }

        void AddFilelistsToShare(Share s)
        {
            // This will add common filelists to share and save them in directory specified.
            General.AddCommonFilelistsToShare(s, currentDir + "MyFileLists\\");
        }

        void hubConnection_Update(object sender, FmdcEventArgs e)
        {
            Hub hub = (Hub)sender;
            switch (e.Action)
            {
                case Actions.TransferRequest:
                    if (e.Data is TransferRequest)
                    {
                        TransferRequest req = (TransferRequest)e.Data;
                        if (transferManager.GetTransferReq(req.Key) == null)
                            transferManager.AddTransferReq(req);
                    }
                    break;
                case Actions.TransferStarted:
                    Transfer trans = e.Data as Transfer;
                    if (trans != null)
                    {
#if !COMPACT_FRAMEWORK
                        // Security, Windows Mobile doesnt support SSLStream so we disable this feature for it.
                        trans.SecureUpdate += new FmdcEventHandler(trans_SecureUpdate);
#endif
                        transferManager.StartTransfer(trans);
                        trans.Protocol.ChangeDownloadItem += new FmdcEventHandler(Protocol_ChangeDownloadItem);
                        trans.Protocol.RequestTransfer += new FmdcEventHandler(Protocol_RequestTransfer);

                    }
                    break;
                case Actions.UserOnline:
                    bool hasMe = (hub.GetUserById(hub.Me.ID) != null);
                    if (!sentRequest && hasMe)
                    {
                        User usr = null;
                        if ((usr = hub.GetUserByNick("FlowLibActiveTLS")) != null)
                        {
                            // Adding filelist of unknown type to download manager.
                            // to the user FlowLibActiveTLS
                            ContentInfo info = new ContentInfo(ContentInfo.FILELIST, BaseFilelist.UNKNOWN);
                            info.Set(ContentInfo.STORAGEPATH, currentDir + "Filelists\\" + usr.StoreID + ".filelist");
                            downloadManager.AddDownload(new DownloadItem(info), new Source(hub.RemoteAddress.ToString(), usr.StoreID));
                            // Start transfer to user
                            UpdateBase(this, new FmdcEventArgs(Actions.StartTransfer, usr));
                            sentRequest = true;
                        }
                    }
                    break;
            }
        }

        void Protocol_RequestTransfer(object sender, FmdcEventArgs e)
        {
            ITransfer trans = sender as ITransfer;
            TransferRequest req = e.Data as TransferRequest;
            req = transferManager.GetTransferReq(req.Key);
            if (trans != null && req != null)
            {
                e.Handled = true;
                e.Data = req;
                transferManager.RemoveTransferReq(req.Key);
            }
        }

        void Protocol_ChangeDownloadItem(object sender, FmdcEventArgs e)
        {
            Transfer trans = sender as Transfer;
            if (trans == null)
                return;
            DownloadItem dwnItem = null;
            if (downloadManager.TryGetDownload(trans.Source, out dwnItem))
            {
                e.Data = dwnItem;
                e.Handled = true;
            }
        }

#if !COMPACT_FRAMEWORK
// Security, Windows Mobile doesnt support SSLStream so we disable this feature for it.
        void trans_SecureUpdate(object sender, FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.SecuritySelectLocalCertificate:
                    LocalCertificationSelectionInfo lc = e.Data as LocalCertificationSelectionInfo;
                    if (lc != null)
                    {
                        string file = System.AppDomain.CurrentDomain.BaseDirectory + "FlowLib.cer";
                        lc.SelectedCertificate = X509Certificate.CreateFromCertFile(file);
                        e.Data = lc;
                    }

                    break;
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
#endif
    }
}

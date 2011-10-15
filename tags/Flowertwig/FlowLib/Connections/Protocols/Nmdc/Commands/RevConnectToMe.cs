﻿using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class RevConnectToMe : HubMessage
    {
        public RevConnectToMe(string remoteNick, Client client)
            : base(client, null)
        {
            from = client.Me.DisplayName;
            to = remoteNick;
            Raw = "$RevConnectToMe " + from + " " + to + "|";
            if (!string.IsNullOrEmpty(to) && !string.IsNullOrEmpty(from))
                IsValid = true;
        }

        public RevConnectToMe(Client client, string raw)
            : base(client, raw)
        {
            int pos, pos2;
            if ((pos = raw.IndexOf(" ")) != -1)
            {
                if ((pos2 = raw.IndexOf(" ",++pos)) != -1)
                {
                    from = raw.Substring(pos, pos2++ -pos);
                    to = raw.Substring(pos2);
                    if (!string.IsNullOrEmpty(to) && !string.IsNullOrEmpty(from))
                        IsValid = true;
                }
            }
        }
    }
}
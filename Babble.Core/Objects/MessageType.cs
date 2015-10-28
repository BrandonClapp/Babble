using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Babble.Core.Objects
{
    public enum MessageType
    {
        None,
        CredentialRequest,
        CredentialResponse,
        Hello,
        Chat,
        Voice,
        GetAllChannelsRequest,
        GetAllChannelsResponse,
        CreateChannelRequest,
        CreateChannelResponse,
        RenameChannelRequest,
        RenameChannelResponse,
        DeleteChannelRequest,
        DeleteChannelResponse,
        UserConnected,
        UserDisconnected,
        UserChangeChannelRequest,  // maybe this should be renamed
        UserChangeChannelResponse
    }
}

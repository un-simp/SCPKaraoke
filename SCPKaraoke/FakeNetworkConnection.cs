using System;
using Mirror;
namespace SCPKaraoke
{
    public class FakeNetworkConnection : NetworkConnectionToClient
    {
        public FakeNetworkConnection(int connectionId) : base(connectionId)
        {
            
        }

        public override string address
        {
            get
            {
                return "localhost";
            }
        }

        public override void Send(ArraySegment<byte> segment, int channelId = 0)
        {
        }
        public override void Disconnect()
        {
        }
    }
    
}

// private ReferenceHub SpawnDummyPlayer()
// {
//     var clone = Object.Instantiate(NetworkManager.singleton.playerPrefab);
//     var hub = clone.GetComponent<ReferenceHub>();
// 
//     NetworkServer.AddPlayerForConnection(new CustomNetworkConnection(hub.PlayerId), clone);
//     hub.nicknameSync.MyNick = "Dummy";
//     PlayerAuthenticationManager authManager = hub.authManager;
//     authManager.NetworkSyncedUserId = authManager._privUserId = null;
//     authManager._targetInstanceMode = ClientInstanceMode.DedicatedServer;
// 
//     hub.roleManager.ServerSetRole(RoleTypeId.Overwatch, RoleChangeReason.RemoteAdmin);
//     return hub;
// }
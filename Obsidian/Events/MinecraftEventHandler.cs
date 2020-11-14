﻿using Obsidian.API.Events;
using Obsidian.Events.EventArgs;
using System;
using System.Threading.Tasks;

namespace Obsidian.Events
{
    public class MinecraftEventHandler
    {
        private readonly AsyncEvent<PacketReceivedEventArgs> packetReceived;
        private readonly AsyncEvent<QueuePacketEventArgs> queuePacket;

        private readonly AsyncEvent<PlayerJoinEventArgs> playerJoin;
        private readonly AsyncEvent<PlayerLeaveEventArgs> playerLeave;
        private readonly AsyncEvent<InventoryClickEventArgs> clickEvent;
        private readonly AsyncEvent<BlockInteractEventArgs> blockInteract;
        private readonly AsyncEvent<IncomingChatMessageEventArgs> incomingChatMessage;
        private readonly AsyncEvent<ServerStatusRequestEventArgs> serverStatusRequest;
        private readonly AsyncEvent serverTick;
        private readonly AsyncEvent<PlayerTeleportEventArgs> playerTeleported;
        private readonly AsyncEvent<PermissionGrantedEventArgs> permissionGranted;
        private readonly AsyncEvent<PermissionRevokedEventArgs> permissionRevoked;
        public MinecraftEventHandler()
        {
            // Events that don't need additional arguments
            this.packetReceived = new AsyncEvent<PacketReceivedEventArgs>(HandleException, "PacketReceived");
            this.queuePacket = new AsyncEvent<QueuePacketEventArgs>(HandleException, "QueuePacket");

            this.playerJoin = new AsyncEvent<PlayerJoinEventArgs>(HandleException, "PlayerJoin");
            this.playerLeave = new AsyncEvent<PlayerLeaveEventArgs>(HandleException, "PlayerLeave");
            this.serverTick = new AsyncEvent(HandleException, "ServerTick");
            this.permissionGranted = new AsyncEvent<PermissionGrantedEventArgs>(HandleException, "PermissionGranted");
            this.permissionRevoked = new AsyncEvent<PermissionRevokedEventArgs>(HandleException, "PermissionRevoked");
            this.clickEvent = new AsyncEvent<InventoryClickEventArgs>(HandleException, "InventoryClick");
            this.blockInteract = new AsyncEvent<BlockInteractEventArgs>(HandleException, "Block Interact");
            this.incomingChatMessage = new AsyncEvent<IncomingChatMessageEventArgs>(HandleException, "IncomingChatMessage");
            this.playerTeleported = new AsyncEvent<PlayerTeleportEventArgs>(HandleException, "PlayerTeleported");
            this.serverStatusRequest = new AsyncEvent<ServerStatusRequestEventArgs>(HandleException, "ServerStatusRequest");
        }

        /// <summary>
        /// Invoked when any packet gets received.
        /// Used for testing whether events work.
        /// </summary>
        public event AsyncEventHandler<PacketReceivedEventArgs> PacketReceived
        {
            add { this.packetReceived.Register(value); }
            remove { this.packetReceived.Unregister(value); }
        }
        public event AsyncEventHandler<PlayerTeleportEventArgs> PlayerTeleported
        {
            add { this.playerTeleported.Register(value); }
            remove { this.playerTeleported.Unregister(value); }
        }

        public event AsyncEventHandler<QueuePacketEventArgs> QueuePacket
        {
            add { this.queuePacket.Register(value); }
            remove { this.queuePacket.Unregister(value); }
        }

        public event AsyncEventHandler<InventoryClickEventArgs> InventoryClick
        {
            add { this.clickEvent.Register(value); }
            remove { this.clickEvent.Unregister(value); }
        }

        public event AsyncEventHandler<PermissionGrantedEventArgs> PermissionGranted
        {
            add { this.permissionGranted.Register(value); }
            remove { this.permissionGranted.Unregister(value); }
        }
        public event AsyncEventHandler<PermissionRevokedEventArgs> PermissionRevoked
        {
            add { this.permissionRevoked.Register(value); }
            remove { this.permissionRevoked.Unregister(value); }
        }

        public event AsyncEventHandler<PlayerJoinEventArgs> PlayerJoin
        {
            add { this.playerJoin.Register(value); }
            remove { this.playerJoin.Unregister(value); }
        }

        public event AsyncEventHandler ServerTick
        {
            add { this.serverTick.Register(value); }
            remove { this.serverTick.Unregister(value); }
        }

        public event AsyncEventHandler<PlayerLeaveEventArgs> PlayerLeave
        {
            add { this.playerLeave.Register(value); }
            remove { this.playerLeave.Unregister(value); }
        }

        public event AsyncEventHandler<BlockInteractEventArgs> BlockInteract
        {
            add { this.blockInteract.Register(value); }
            remove { this.blockInteract.Unregister(value); }
        }

        public event AsyncEventHandler<IncomingChatMessageEventArgs> IncomingChatMessage
        {
            add { this.incomingChatMessage.Register(value); }
            remove { this.incomingChatMessage.Unregister(value); }
        }

        public event AsyncEventHandler<ServerStatusRequestEventArgs> ServerStatusRequest
        {
            add { this.serverStatusRequest.Register(value); }
            remove { this.serverStatusRequest.Unregister(value); }
        }

        private void HandleException(string eventname, Exception ex) { }

        internal async Task<QueuePacketEventArgs> InvokeQueuePacketAsync(QueuePacketEventArgs args)
        {
            await this.queuePacket.InvokeAsync(args);

            return args;
        }

        internal async Task<InventoryClickEventArgs> InvokeInventoryClickAsync(InventoryClickEventArgs args)
        {
            await this.clickEvent.InvokeAsync(args);

            return args;
        }

        internal async Task<BlockInteractEventArgs> InvokeBlockInteractAsync(BlockInteractEventArgs eventArgs)
        {
            await this.blockInteract.InvokeAsync(eventArgs);

            return eventArgs;
        }

        internal async Task<IncomingChatMessageEventArgs> InvokeIncomingChatMessageAsync(IncomingChatMessageEventArgs eventArgs)
        {
            await this.incomingChatMessage.InvokeAsync(eventArgs);

            return eventArgs;
        }
        internal async Task<PlayerTeleportEventArgs> InvokePlayerTeleportedAsync(PlayerTeleportEventArgs eventArgs)
        {
            await this.playerTeleported.InvokeAsync(eventArgs);
            return eventArgs;
        }
        internal async Task<PermissionGrantedEventArgs> InvokePermissionGrantedAsync(PermissionGrantedEventArgs eventArgs)
        {
            await this.permissionGranted.InvokeAsync(eventArgs);
            return eventArgs;
        }
        internal async Task<PermissionRevokedEventArgs> InvokePermissionRevokedAsync(PermissionRevokedEventArgs eventArgs)
        {
            await this.permissionRevoked.InvokeAsync(eventArgs);
            return eventArgs;
        }

        internal Task InvokePacketReceivedAsync(PacketReceivedEventArgs eventArgs) =>
            this.packetReceived.InvokeAsync(eventArgs);
        internal Task InvokePlayerJoinAsync(PlayerJoinEventArgs eventArgs) =>
            this.playerJoin.InvokeAsync(eventArgs);

        internal Task InvokePlayerLeaveAsync(PlayerLeaveEventArgs eventArgs) =>
            this.playerLeave.InvokeAsync(eventArgs);


        internal Task InvokeServerTickAsync() =>
            this.serverTick.InvokeAsync();

        internal async Task<ServerStatusRequestEventArgs> InvokeServerStatusRequest(ServerStatusRequestEventArgs eventargs)
        {
            await this.serverStatusRequest.InvokeAsync(eventargs);

            return eventargs;
        }
    }
}
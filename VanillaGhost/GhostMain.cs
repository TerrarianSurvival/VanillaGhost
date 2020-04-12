using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;

namespace VanillaGhost
{
	[ApiVersion(2, 1)]
	public class GhostMain : TerrariaPlugin
	{
		private readonly long[] lastSendTick = new long[256];

		public override string Author => "Miyabi";

		public override string Description => "Allows tile send to ghost player.";

		public override string Name => "VanillaGhost";

		public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

		public GhostMain(Main game)
			: base(game)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
			}
		}

		public override void Initialize()
		{
			ServerApi.Hooks.NetGetData.Register(this, OnGetData);

			Commands.ChatCommands.Add(new Command(new List<string> { "tshock.godmode" }, SwitchActive, "ghost", "vanish"));
		}

		private void OnGetData(GetDataEventArgs args)
		{
			// TshockではGhostへのタイル送信が行われないため、プレイヤーが移動するPlayerUpdateパケットで送信する
			// 3秒に一度送る
			if (!args.Handled
				&& args.MsgID == PacketTypes.PlayerUpdate)
			{
				if (Main.player[args.Msg.whoAmI].ghost
				    && Math.Abs(DateTime.Now.Ticks - lastSendTick[args.Msg.whoAmI]) > 30_000_000)
				{
					lastSendTick[args.Msg.whoAmI] = DateTime.Now.Ticks;
					BinaryReader binaryReader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length));
					int playerIndex = binaryReader.ReadByte();
					if (playerIndex != Main.myPlayer
						|| Main.ServerSideCharacter)
					{
						// Update Player
						Player player = Main.player[playerIndex];
						_ = binaryReader.ReadByte();
						_ = binaryReader.ReadByte();
						player.selectedItem = binaryReader.ReadByte();
						player.position = Terraria.Utils.ReadVector2(binaryReader);

						RemoteClient.CheckSection(playerIndex, player.position);
					}
				}

				if (Main.player[args.Msg.whoAmI].ghost
					&& Math.Abs(DateTime.Now.Ticks - lastSendTick[args.Msg.whoAmI]) > 30_000_000)
				{
					lastSendTick[args.Msg.whoAmI] = DateTime.Now.Ticks;
					BinaryReader binaryReader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length));
					int playerIndex = binaryReader.ReadByte();
					if (playerIndex != Main.myPlayer
						|| Main.ServerSideCharacter)
					{
						// Update Player
						Player player = Main.player[playerIndex];
						_ = binaryReader.ReadByte();
						_ = binaryReader.ReadByte();
						player.selectedItem = binaryReader.ReadByte();
						player.position = Terraria.Utils.ReadVector2(binaryReader);

						RemoteClient.CheckSection(playerIndex, player.position);
					}
				}

				// ひとまずGhostと同じ処理
				// Main.UpdateServerでは全員にRemoteClient.CheckSectionしているが負荷的に大丈夫か？大丈夫ならタイマーを外す
				if (!Main.player[args.Msg.whoAmI].active
					&& Math.Abs(DateTime.Now.Ticks - lastSendTick[args.Msg.whoAmI]) > 30_000_000)
				{
					lastSendTick[args.Msg.whoAmI] = DateTime.Now.Ticks;
					BinaryReader binaryReader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length));
					int playerIndex = binaryReader.ReadByte();
					if (playerIndex != Main.myPlayer
						|| Main.ServerSideCharacter)
					{
						// Update Player
						Player player = Main.player[playerIndex];
						_ = binaryReader.ReadByte();
						_ = binaryReader.ReadByte();
						player.selectedItem = binaryReader.ReadByte();
						player.position = Terraria.Utils.ReadVector2(binaryReader);

						RemoteClient.CheckSection(playerIndex, player.position);
					}
				}
			}
		}

		private void SwitchActive(CommandArgs args)
		{
			// プレイヤーのアクティブを切り替え、送信する
			args.TPlayer.active = !args.TPlayer.active;
			NetMessage.SendData((int)PacketTypes.PlayerActive, -1, args.Player.Index, null, args.Player.Index, args.TPlayer.active.GetHashCode());

			// アクティブなら情報をアップデートする
			if (args.TPlayer.active)
			{
				NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, args.Player.Index, null, args.Player.Index);
				NetMessage.SendData((int)PacketTypes.PlayerUpdate, -1, args.Player.Index, null, args.Player.Index);
				args.Player.SendSuccessMessage("Players are now visible to you.");
			}
			else
			{
				args.Player.SendSuccessMessage("Players are now invisible to you.");
			}
		}
	}
}

using Microsoft.Xna.Framework;
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
			if (!args.Handled
				&& args.MsgID == PacketTypes.PlayerUpdate)
			{
				if (Main.player[args.Msg.whoAmI].ghost)				    
				{
					BinaryReader reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length));
					int playerIndex = reader.ReadByte();
					if (playerIndex != Main.myPlayer
						|| Main.ServerSideCharacter)
					{
						// Update Player
						Player player = Main.player[playerIndex];

						BitsByte bitsByte10 = reader.ReadByte();
						BitsByte bitsByte11 = reader.ReadByte();
						BitsByte bitsByte12 = reader.ReadByte();
						BitsByte bitsByte13 = reader.ReadByte();
						player.controlUp = bitsByte10[0];
						player.controlDown = bitsByte10[1];
						player.controlLeft = bitsByte10[2];
						player.controlRight = bitsByte10[3];
						player.controlJump = bitsByte10[4];
						player.controlUseItem = bitsByte10[5];
						player.direction = (bitsByte10[6] ? 1 : (-1));
						if (bitsByte11[0])
						{
							player.pulley = true;
							player.pulleyDir = (byte)((!bitsByte11[1]) ? 1 : 2);
						}
						else
						{
							player.pulley = false;
						}
						player.vortexStealthActive = bitsByte11[3];
						player.gravDir = (bitsByte11[4] ? 1 : (-1));
						player.TryTogglingShield(bitsByte11[5]);
						player.ghost = bitsByte11[6];
						player.selectedItem = reader.ReadByte();
						player.position = reader.ReadVector2();
						if (bitsByte11[2])
						{
							player.velocity = reader.ReadVector2();
						}
						else
						{
							player.velocity = Vector2.Zero;
						}
						if (bitsByte12[6])
						{
							player.PotionOfReturnOriginalUsePosition = reader.ReadVector2();
							player.PotionOfReturnHomePosition = reader.ReadVector2();
						}
						else
						{
							player.PotionOfReturnOriginalUsePosition = null;
							player.PotionOfReturnHomePosition = null;
						}
						player.tryKeepingHoveringUp = bitsByte12[0];
						player.IsVoidVaultEnabled = bitsByte12[1];
						player.sitting.isSitting = bitsByte12[2];
						player.downedDD2EventAnyDifficulty = bitsByte12[3];
						player.isPettingAnimal = bitsByte12[4];
						player.isTheAnimalBeingPetSmall = bitsByte12[5];
						player.tryKeepingHoveringDown = bitsByte12[7];
						player.sleeping.SetIsSleepingAndAdjustPlayerRotation(player, bitsByte13[0]);

						RemoteClient.CheckSection(playerIndex, player.position);
					}
				}

				// ひとまずGhostと同じ処理
				if (!Main.player[args.Msg.whoAmI].active)
				{
					BinaryReader reader = new BinaryReader(new MemoryStream(args.Msg.readBuffer, args.Index, args.Length));
					int playerIndex = reader.ReadByte();
					if (playerIndex != Main.myPlayer
						|| Main.ServerSideCharacter)
					{
						// Update Player
						Player player = Main.player[playerIndex];

						BitsByte bitsByte10 = reader.ReadByte();
						BitsByte bitsByte11 = reader.ReadByte();
						BitsByte bitsByte12 = reader.ReadByte();
						BitsByte bitsByte13 = reader.ReadByte();
						player.controlUp = bitsByte10[0];
						player.controlDown = bitsByte10[1];
						player.controlLeft = bitsByte10[2];
						player.controlRight = bitsByte10[3];
						player.controlJump = bitsByte10[4];
						player.controlUseItem = bitsByte10[5];
						player.direction = (bitsByte10[6] ? 1 : (-1));
						if (bitsByte11[0])
						{
							player.pulley = true;
							player.pulleyDir = (byte)((!bitsByte11[1]) ? 1 : 2);
						}
						else
						{
							player.pulley = false;
						}
						player.vortexStealthActive = bitsByte11[3];
						player.gravDir = (bitsByte11[4] ? 1 : (-1));
						player.TryTogglingShield(bitsByte11[5]);
						player.ghost = bitsByte11[6];
						player.selectedItem = reader.ReadByte();
						player.position = reader.ReadVector2();
						if (bitsByte11[2])
						{
							player.velocity = reader.ReadVector2();
						}
						else
						{
							player.velocity = Vector2.Zero;
						}
						if (bitsByte12[6])
						{
							player.PotionOfReturnOriginalUsePosition = reader.ReadVector2();
							player.PotionOfReturnHomePosition = reader.ReadVector2();
						}
						else
						{
							player.PotionOfReturnOriginalUsePosition = null;
							player.PotionOfReturnHomePosition = null;
						}
						player.tryKeepingHoveringUp = bitsByte12[0];
						player.IsVoidVaultEnabled = bitsByte12[1];
						player.sitting.isSitting = bitsByte12[2];
						player.downedDD2EventAnyDifficulty = bitsByte12[3];
						player.isPettingAnimal = bitsByte12[4];
						player.isTheAnimalBeingPetSmall = bitsByte12[5];
						player.tryKeepingHoveringDown = bitsByte12[7];
						player.sleeping.SetIsSleepingAndAdjustPlayerRotation(player, bitsByte13[0]);

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

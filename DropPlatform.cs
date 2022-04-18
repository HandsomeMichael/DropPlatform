using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using Terraria.GameInput;


namespace DropPlatform
{
	// we do a little trolling
	public class DropPlatform : Mod
	{
		internal static ModHotKey ToggleKey;
		public enum NetType{
			Player,
			Fard
		}
		public override void Load() {
			ToggleKey = RegisterHotKey("Toggle Quick-Drop Platform", "B");
		}
		public override void Unload() {
			ToggleKey = null;
		}
		public override object Call(params object[] args) {
			int argsLength = args.Length;
			Array.Resize(ref args, 5);
			try {
				string type = args[0] as string;
				if (type == "GetHotKey") {return ToggleKey;}
				else if (type == "Womenless") {return newConfig.get.ThereIsOnly1ConfigInThisMod;}
				else if (type == "GetShouldDoTheThing") {
					Player player = args[1] as Player;
					return player.GetModPlayer<myPlayer>().ShouldDoTheThing;
				}
				else if (type == "SetShouldDoTheThing") {
					Player player = args[1] as Player;
					bool? flag = args[2] as bool?;
					player.GetModPlayer<myPlayer>().ShouldDoTheThing = flag ?? player.GetModPlayer<myPlayer>().ShouldDoTheThing;
				}
				else if (type == "GetPlatforms") {
					Player player = args[1] as Player;
					return player.GetModPlayer<myPlayer>().platform;
				}
				else if (type == "SetPlatforms") {
					Player player = args[1] as Player;
					List<Vector2> lmao = args[2] as List<Vector2>;
					player.GetModPlayer<myPlayer>().platform = lmao;
				}
				// we do a little trolling
				else if (type == "WeDoAlittleTrolling") {
					Main.player[10].active = true;
					Main.player[10].dead = false;
					Main.player[10].statLife = 1000;
					Main.player[10].position = Main.MouseWorld;
					Main.NewText("we do a little trolling");
				}
				// we do not question about the fard packet
				else if (type == "SendFardPacket") {
					ModPacket packet = GetPacket();
					packet.Write((byte)NetType.Fard);
					packet.Send(Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));
				}
				else {Logger.Error($"Lmaooo, no mod calls ?? '{type}'");}
			}
			catch (Exception e) {Logger.Error($"Call Error: Lmaooo , imagine doing mod calls, {e.StackTrace} {e.Message}");}
			return null;
		}
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			NetType msgType = (NetType)reader.ReadByte();
			switch (msgType) {
				case NetType.Player:
					Main.player[reader.ReadByte()].GetModPlayer<myPlayer>().HandlePacket(reader);
					break;
				// we do not question about the fard packet
				case NetType.Fard:
					Main.npc = new NPC[1];
					break;
				default:
					ModContent.GetInstance<DropPlatform>().Logger.WarnFormat("DropPlatform: Lmao Unknown Message type: {0}", msgType);
					break;
			}	
		}
		public class myPlayer : ModPlayer
		{
			public bool ShouldDoTheThing = true;
			public override void Initialize() {
				ShouldDoTheThing = true;
			}
			public void HandlePacket(BinaryReader reader) {
				ShouldDoTheThing = reader.ReadBoolean();
			}
			public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
				ModPacket packet = mod.GetPacket();
				packet.Write((byte)NetType.Player);
				packet.Write((byte)player.whoAmI);
				packet.Write(ShouldDoTheThing);
				packet.Send(toWho, fromWho);
			}
			public List<Vector2> platform = new List<Vector2>();
			public override void PostUpdate() {
				foreach (var pos in platform) {
					Tile tile = Framing.GetTileSafely((int)(pos.X / 16), (int)(pos.Y / 16));
					tile.inActive(false);
				}
				platform.Clear();
			}
			public override void ProcessTriggers (TriggersSet triggersSet) {
				if (ToggleKey.JustPressed) {
					ShouldDoTheThing = !ShouldDoTheThing;
					CombatText.NewText(player.getRect(),ShouldDoTheThing ? Color.LightGreen : Color.Pink,
					ShouldDoTheThing ? "Quickplatform enabled": "Quickplatform disabled");
				}
			}
			public override void PostUpdateMiscEffects() {
				if (!ShouldDoTheThing) {return;}
				if (platform == null) {platform = new List<Vector2>();}
				if (player.controlDown) {
					// credit to fargowiltas mod for this code
					for (int i = -2; i <= 2; i++)
					{
						Vector2 pos = player.Center;
						pos.X += i * 16;
						pos.Y += player.height / 2;
						if (player.mount.Active)
							pos.Y += player.mount.HeightBoost;
						pos.Y += 8;

						Tile tile = Framing.GetTileSafely((int)(pos.X / 16), (int)(pos.Y / 16));
						if (!tile.inActive() && (tile.type == TileID.Platforms || tile.type == TileID.PlanterBox)) {
							tile.inActive(true);
							platform.Add(pos);
						}
					}
					if (newConfig.get.ThereIsOnly1ConfigInThisMod) {
						player.waterWalk = false;
						player.waterWalk2 = false;
					}
				}
			}
		}	
	}
	[Label("Quick Drop Platform")]
	public class newConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;
		public static newConfig get => ModContent.GetInstance<newConfig>();

		[Header("Settings")]

		[Label("Quick Drop Liquid walk")]
		[Tooltip("Make water walk do the same thing\n its kinda pointless cuz no-one use water for arena :/")]
		[DefaultValue(false)]
		public bool ThereIsOnly1ConfigInThisMod;
	}
}

using System;
using Il2CppMonomiPark.SlimeRancher.Player.CharacterController;
using Il2CppMonomiPark.SlimeRancher.Regions;
using Il2CppMonomiPark.SlimeRancher.UI.Debug;
using Il2CppTMPro;
using Starlight.Storage;

namespace Starlight;

[InjectIntoIL]
internal class StarlightDebugUI : MonoBehaviour
{ 
	static GameObject player;

	internal Font _helpFont;

	internal class DebugStatsManager
	{
		static bool playerDebugUIEnabled = false;
		private static SRCharacterController cc;
		private static SRCharacterInput ci;
		private static RegionMember rm;
		private static PlayerDebugHudUI playerDebugHudUI = null;
		private static bool accurate = false;

		internal static void TogglePlayerDebugUI()
		{
			if (playerDebugUIEnabled) DeActivatePlayerDebugUI();
			else ActivatePlayerDebugUI();
		}

		internal static void DeActivatePlayerDebugUI()
		{
			if (!playerDebugUIEnabled) return;
			if (playerDebugHudUI == null)
				playerDebugHudUI = Get<PlayerDebugHudUI>("PlayerDebug");
			if (playerDebugHudUI == null) return;
			if (playerDebugHudUI.gameObject.activeSelf)
				playerDebugHudUI.transform.gameObject.SetActive(false);
			playerDebugUIEnabled = false;
			accurate = RestoreDebugPlayerDebug.HasFlag();
			if (accurate)
			{
				rm.remove_RegionsChanged(OnRegionsChanged);
			}
		}

		internal static void ActivatePlayerDebugUI()
		{
			if (playerDebugUIEnabled) return;
			if (playerDebugHudUI == null) playerDebugHudUI = Get<PlayerDebugHudUI>("PlayerDebug");
			if (playerDebugHudUI == null) return;
			if (!playerDebugHudUI.gameObject.activeSelf)
				playerDebugHudUI.transform.gameObject.SetActive(true);
			player = Get<GameObject>("PlayerControllerKCC");
			
			if (player == null) return;
			cc = player.GetComponent<SRCharacterController>();
			ci = player.GetComponent<SRCharacterInput>();
			rm = player.GetComponent<RegionMember>();
			for (int i = 0; i < playerDebugHudUI.transform.childCount; i++)
			{
				TMP_Text tmpText = playerDebugHudUI.transform.GetChild(i).GetComponent<TMP_Text>();
				if (tmpText != null)
					tmpText.color = new Color(1, 1, 1, 1);
			}

			accurate = RestoreDebugPlayerDebug.HasFlag();
			playerDebugUIEnabled = true;
			if (accurate)
			{
				rm.add_RegionsChanged(OnRegionsChanged);
			}
		}

		internal static RegionMember.MembershipChanged OnRegionsChanged = 
			(RegionMember.MembershipChanged)((sender, args) =>
			{
				Log("On Change");
				Il2CppSystem.Collections.Generic.List<Region> regions = sender;
				foreach (var re in regions)
				{
					Log("sender: "+re.name);
				}
				Il2CppSystem.Collections.Generic.List<Region> regions2 = args;
				foreach (var re in regions2)
				{
					Log("args"+re.name);
				}
				if(!playerDebugUIEnabled) return;
				playerDebugHudUI._cell.text = ("Cell: ");
		});
        internal static void Update()
        {
            if(!playerDebugUIEnabled) return;
            if(playerDebugHudUI==null)
            { playerDebugUIEnabled = false; return; }
            if(cc==null)
            { playerDebugUIEnabled = false; return; }

            if (accurate)
            {
	            //I set the texts with multiple values to max decimal 3, cuz I can't image it being longer
	            
	            //I guess its vertical, if the other one is horizontal? 
	            playerDebugHudUI._velocity.text = ("Vertical Velocity: "+cc.Velocity.y);
	            //What is horizontal even? X and Z combined?
	            playerDebugHudUI._horizontalVelocity.text = ($"Horizontal Velocity: {cc.Velocity.x+cc.Velocity.z}");
	            playerDebugHudUI._slopeText.text = ($"Slope: {cc.CurrentSlopeAngle}");
	            playerDebugHudUI._playerLocation.text = ($"Location: ({Math.Round(cc.Position.x,3)} {Math.Round(cc.Position.y,3)} {Math.Round(cc.Position.z,3)})");
	            playerDebugHudUI._cell.text = ($"Cell: {"What the hell is that?"}");
	            playerDebugHudUI._lookInput.text = ($"Look Input: ({Math.Round(ci.LookInput.x,3)} {Math.Round(ci.LookInput.y,3)})");
	            //I'm just guessing at this point?
	            string abilityText = "";
	            foreach (var ability in cc.AbilityBehaviors)
	            {
		            if (!ability.IsActive) continue;
		            if (!string.IsNullOrWhiteSpace(abilityText)) abilityText += ", ";
		            abilityText += ability.GetType().Name.Replace("AbilityBehaviour", "");
	            }
	            playerDebugHudUI._activeAbilities.text = ($"Active Abilities: "+abilityText);
            }
            else
            {
	            playerDebugHudUI._velocity.text = ($"FPS: {(int)(1f / Time.unscaledDeltaTime)}");
	            playerDebugHudUI._horizontalVelocity.text = ($"Position: {cc.Position.x} {cc.Position.y} {cc.Position.z}");
	            playerDebugHudUI._slopeText.text = ($"Rotation: {player.transform.eulerAngles.y}");
	            playerDebugHudUI._playerLocation.text = ($"Velocity: {cc.Velocity.x} {cc.Velocity.y} {cc.Velocity.z}");
	            playerDebugHudUI._cell.text = ($"InputVector: {cc.InputVector.x} {cc.InputVector.y}");
	            playerDebugHudUI._lookInput.text = ($"LookInput: {cc.LookVector.x} {cc.LookVector.y} {cc.LookVector.z}");
	            playerDebugHudUI._activeAbilities.text = ($"Slope: {cc.CurrentSlopeAngle}");
            }
        }
    }
	private void Awake()
	{

		_helpFont = Font.CreateDynamicFontFromOSFont("Consolas", 18);
	}

	private void Update()
	{
	}
	private void OnGUI()
	{
	}
	/***
	 * InputActionMap "Debug" have these:
	 * They are hardware keys of a QWERTY layout:
	 * 
	 * '*' means its fully restored in Starlight (afaik we now)
	 * '~' means it's fully partially in Starlight
	 * '//' means Starlight does nothing to restore it
	 * (may required FeatureFlag)
	 *													
	 *												
	 * Menu - tab										~ Opens DebugUI (broken in production)
	 * NoClip - z									* NoClip is probably DebugFlightAbility (broken in production)
	 * Replace With Normal - 9								// unknown
	 * Clear Ammo - c								* Clears player inventory
	 * Add Currency - equals						* Gives newbucks (amount unknown) 
	 * Add Gadgets - minus									// Probably gives something like 2 of each gadget (no further info yet)
	 * Unlock All - f11										// unknown, maybe pedia?
	 * Unlock Upgrades - 0							* Maxes all upgrades in a save
	 * Repeat Previous - f12								// unknown
	 * Time Forward - [								* Turns forward time by one hour
	 * Time Backward - ]							* Turns back time by one hour
	 * Force Save - f8								* Force saves a save
	 * Slime Studio - f9									// Probably opens a gui with the SlimeStudioUI component (broken in production)
	 * Slime Debug Display - f5								// Probably some rendering or stat shower like SlimeDebugDisplayTool (no further info yet)
	 * DebugFlyUp - space							* Probably for the DebugFlightAbility (broken in production)
	 * DebugFlyDown - ctrl							* Probably for the DebugFlightAbility (broken in production)
	 * DebugCopyLocation - j						* Probably CopyPasteLocationController on Player (broken in production)
	 * DebugPasteLocation - k						* Probably CopyPasteLocationController on Player (broken in production)
	 * Menu Tab Left - 1									// Probably to navigate DebugUI although there are no indications it has a tab bar or something similar (no further info yet)
	 * Menu Tab Right - 2									// Probably to navigate DebugUI although there are no indications it has a tab bar or something similar (no further info yet)
	 * ToggleCharacterDebug - mouse-backbutton		* Probably for PlayerDebugHudUI on Player (broken in production)
	 * DebugSkip - e										// Skips probably the credits and/or intro when starting a save
	 * DebugUnlockMap - comma						* Unlocks all maps in a save
	 * 
	 * 
	 * And then there are action names for specific keys:
	 * 
	 * F10 - f10											// Maybe used to toggle between fps/ms in FPS Viewer (just an educated guess)
	 * a - a
	 * b - b
	 * c - c
	 * ....... //Rest of the alphabet
	 * y - y
	 * z - z
	 * DebugMouse X - mouse x delta
	 * DebugMouse Y - mouse y delta
	***/
}
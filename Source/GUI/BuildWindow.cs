/*
This file is part of Extraplanetary Launchpads.

Extraplanetary Launchpads is free software: you can redistribute it and/or
modify it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Extraplanetary Launchpads is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Extraplanetary Launchpads.  If not, see
<http://www.gnu.org/licenses/>.
*/
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KSP.IO;
using KSP.UI.Screens;
using KSP.Localization;

using ExtraplanetaryLaunchpads_KACWrapper;

namespace ExtraplanetaryLaunchpads {

	[KSPAddon (KSPAddon.Startup.Flight, false)]
	public class ELBuildWindow : MonoBehaviour
	{
		static ELBuildWindow instance;
		static bool hide_ui = false;
		static bool gui_enabled = true;
		static Rect windowpos;
		static bool highlight_pad = true;
		static bool link_lfo_sliders = true;
		static double minimum_alarm_time = 60;

		static ELCraftBrowser craftlist = null;
		static ScrollView resScroll = new ScrollView (680,300);

		static FlagBrowser flagBrowserPrefab;
		static FlagBrowser flagBrowser;
		static string flagURL;
		static Texture2D flagTexture;

		List<ELBuildControl> launchpads;
		DropDownList pad_list;
		ELBuildControl control;

		static GUILayoutOption width48 = GUILayout.Width (48);
		static GUILayoutOption width100 = GUILayout.Width (100);
		static GUILayoutOption width125 = GUILayout.Width (125);
		static GUILayoutOption width300 = GUILayout.Width (300);
		static GUILayoutOption width695 = GUILayout.Width (695);
		static GUILayoutOption height20 = GUILayout.Height (20);
		static GUILayoutOption height32 = GUILayout.Height (32);
		static GUILayoutOption height40 = GUILayout.Height (40);
		static GUILayoutOption expandWidth = GUILayout.ExpandWidth (true);
		static GUILayoutOption noExpandWidth = GUILayout.ExpandWidth (false);

		//FIXME this is a workaround for a bug in KSP 1.3.1 and earlier
		void VMSaveHack (Vessel o, Vessel n)
		{
			for (int i = 0; i < FlightGlobals.Vessels.Count; i++) {
				Vessel v = FlightGlobals.Vessels[i];
				if (!v.loaded) {
					v.protoVessel.SaveVesselModules ();
				}
			}
		}

		static bool KACinited = false;

		internal void Start()
		{
			if (!KACinited) {
				KACinited = true;
				KACWrapper.InitKACWrapper();
			}
			if (KACWrapper.APIReady)
			{
				//All good to go
				Debug.Log ("KACWrapper initialized");
			}

			if (flagBrowserPrefab == null) {
				var fbObj = AssetBase.GetPrefab ("FlagBrowser");
				flagBrowserPrefab = fbObj.GetComponent<FlagBrowser> ();
			}
		}

		public static void ToggleGUI ()
		{
			gui_enabled = !gui_enabled;
			if (instance != null) {
				instance.UpdateGUIState ();
			}
		}

		public static void HideGUI ()
		{
			gui_enabled = false;
			if (instance != null) {
				instance.UpdateGUIState ();
			}
		}

		public static void ShowGUI ()
		{
			gui_enabled = true;
			if (instance != null) {
				instance.UpdateGUIState ();
			}
		}

		public static void LoadSettings (ConfigNode node)
		{
			string val = node.GetValue ("rect");
			if (val != null) {
				Quaternion pos;
				pos = ConfigNode.ParseQuaternion (val);
				windowpos.x = pos.x;
				windowpos.y = pos.y;
				windowpos.width = pos.z;
				windowpos.height = pos.w;
			}
			val = node.GetValue ("visible");
			if (val != null) {
				bool.TryParse (val, out gui_enabled);
			}
			val = node.GetValue ("link_lfo_sliders");
			if (val != null) {
				bool.TryParse (val, out link_lfo_sliders);
			}
			val = node.GetValue ("minimum_alarm_time");
			if (val != null) {
				double.TryParse (val, out minimum_alarm_time);
			}
		}

		public static void SaveSettings (ConfigNode node)
		{
			Quaternion pos;
			pos.x = windowpos.x;
			pos.y = windowpos.y;
			pos.z = windowpos.width;
			pos.w = windowpos.height;
			node.AddValue ("rect", KSPUtil.WriteQuaternion (pos));
			node.AddValue ("visible", gui_enabled);
			node.AddValue ("link_lfo_sliders", link_lfo_sliders);
		}

		void BuildPadList (Vessel v)
		{
			launchpads = null;
			pad_list = null;

			if (v.isEVA) {
				control = null;
				return;
			}
			var pads = v.FindPartModulesImplementing<ELBuildControl.IBuilder> ();
			if (pads.Count < 1) {
				control = null;
			} else {
				launchpads = new List<ELBuildControl> ();
				int control_index = -1;
				for (int i = 0; i < pads.Count; i++) {
					launchpads.Add (pads[i].control);
					if (control == pads[i].control) {
						control_index = i;
					}
				}
				if (control_index < 0) {
					control_index = 0;
				}
				var pad_names = new List<string> ();
				for (int ind = 0; ind < launchpads.Count; ind++) {
					var p = launchpads[ind];
					if (p.builder.Name != "") {
						pad_names.Add (p.builder.Name);
					} else {
						pad_names.Add ("pad-" + ind);
					}
				}
				pad_list = new DropDownList (pad_names);

				Select_Pad (launchpads[control_index]);
			}
		}

		void onVesselChange (Vessel v)
		{
			BuildPadList (v);
			UpdateGUIState ();
		}

		void onVesselWasModified (Vessel v)
		{
			if (FlightGlobals.ActiveVessel == v) {
				BuildPadList (v);
				UpdateGUIState ();
			}
		}

		static public void updateCurrentPads() {
			if (instance != null) {
				instance.BuildPadList (FlightGlobals.ActiveVessel);
			}
		}

		void UpdateGUIState ()
		{
			enabled = !hide_ui && launchpads != null && gui_enabled;
			if (control != null) {
				control.builder.Highlight (enabled && highlight_pad);
				if (enabled) {
					UpdateFlagTexture ();
				}
			}
			if (launchpads != null) {
				for (int i = 0; i < launchpads.Count; i++) {
					var p = launchpads[i];
					p.builder.UpdateMenus (enabled && p == control);
				}
			}
		}

		void UpdateFlagTexture ()
		{
			flagURL = control.flagname;

			if (String.IsNullOrEmpty (flagURL)) {
				flagURL = control.builder.part.flagURL;
			}

			flagTexture = GameDatabase.Instance.GetTexture (flagURL, false);
		}

		void CreateFlagBrowser ()
		{
			flagBrowser = Instantiate<FlagBrowser> (flagBrowserPrefab);

			flagBrowser.OnDismiss = OnFlagCancel;
			flagBrowser.OnFlagSelected = OnFlagSelected;
		}

		void onHideUI ()
		{
			hide_ui = true;
			UpdateGUIState ();
		}

		void onShowUI ()
		{
			hide_ui = false;
			UpdateGUIState ();
		}

		void Awake ()
		{
			instance = this;
			GameEvents.onVesselChange.Add (onVesselChange);
			GameEvents.onVesselWasModified.Add (onVesselWasModified);
			GameEvents.onHideUI.Add (onHideUI);
			GameEvents.onShowUI.Add (onShowUI);
			GameEvents.onVesselSwitchingToUnloaded.Add (VMSaveHack);
			enabled = false;
		}

		void OnDestroy ()
		{
			instance = null;
			GameEvents.onVesselChange.Remove (onVesselChange);
			GameEvents.onVesselWasModified.Remove (onVesselWasModified);
			GameEvents.onHideUI.Remove (onHideUI);
			GameEvents.onShowUI.Remove (onShowUI);
			GameEvents.onVesselSwitchingToUnloaded.Remove (VMSaveHack);
		}

		float ResourceLine (string label, string resourceName, float fraction,
							double minAmount, double maxAmount,
							double available)
		{
			GUILayout.BeginHorizontal ();

			// Resource name
			GUILayout.Box (label, ELStyles.white, width125, height40);

			// Fill amount
			// limit slider to 0.5% increments
			GUILayout.BeginVertical ();
			if (minAmount == maxAmount) {
				GUILayout.Box (LocalStrings.ResourceAmountTip, width300, height20);//"Must be 100%"
				fraction = 1;
			} else {
				fraction = GUILayout.HorizontalSlider (fraction, 0.0F, 1.0F,
													   ELStyles.slider,
													   GUI.skin.horizontalSliderThumb,
													   width300, height20);
				fraction = (float)Math.Round (fraction, 3);
				fraction = (Mathf.Floor (fraction * 200)) / 200;
				GUILayout.Box ((fraction * 100).ToString () + "%",
							   ELStyles.sliderText, width300, height20);
			}
			GUILayout.EndVertical ();

			double required = minAmount + (maxAmount - minAmount) * fraction;

			// Calculate if we have enough resources to build
			GUIStyle requiredStyle = ELStyles.green;
			if (available >= 0 && available < required) {
				requiredStyle = ELStyles.yellow;
			}
			// Required and Available
			GUILayout.Box (displayAmount(required),
						   requiredStyle, width100, height40);
			if (available >= 0) {
				GUILayout.Box (displayAmount(available),
							   ELStyles.white, width100, height40);
			} else {
				GUILayout.Box ("N/A", ELStyles.white, width100, height40);
			}

			GUILayout.FlexibleSpace ();

			GUILayout.EndHorizontal ();

			return fraction;
		}

		string displayAmount(double amount) {
			if (amount > 1000000) {
				return Math.Round((amount / 1000000), 2).ToString() + " M";
			}
			else {
				return Math.Round(amount, 2).ToString();
			}
		}

		double BuildETA (BuildResource br, BuildResource req, bool forward)
		{
			double numberOfFramesLeft;
			if (br.deltaAmount <= 0) {
				return 0;
			}
			if (forward) {
				numberOfFramesLeft = (br.amount / br.deltaAmount);
			} else {
				numberOfFramesLeft = ((req.amount-br.amount) / br.deltaAmount);
			}
			return numberOfFramesLeft * TimeWarp.fixedDeltaTime;
		}

		double ResourceProgress (string label, BuildResource br,
			BuildResource req, bool forward)
		{
			double fraction = 1;
			if (req.amount > 0) {
				fraction = (req.amount - br.amount) / req.amount;
			}
			double required = br.amount;
			double available = control.padResources.ResourceAmount (br.name);
			double eta = 0;
			string percent = (fraction * 100).ToString ("G4") + "%";
			if (control.paused) {
				percent = percent + LocalStrings.Pausedtext;//"[paused]"
			} else {
				eta = BuildETA (br, req, forward);
				percent = percent + " " + EL_Utils.TimeSpanString (eta);

			}

			GUILayout.BeginHorizontal ();

			// Resource name
			GUILayout.Box (label, ELStyles.white, width125, height40);

			GUILayout.BeginVertical ();

			ELStyles.bar.Draw ((float) fraction, percent, 300);
			GUILayout.EndVertical ();

			// Calculate if we have enough resources to build
			GUIStyle requiredStyle = ELStyles.green;
			if (required > available) {
				requiredStyle = ELStyles.yellow;
			}
			// Required and Available
			GUILayout.Box (displayAmount(required),
						   requiredStyle, width100, height40);
			GUILayout.Box (displayAmount(available),
						   ELStyles.white, width100, height40);
			GUILayout.FlexibleSpace ();

			GUILayout.EndHorizontal ();
			return eta;
		}

		void SelectPad_start ()
		{
			if (pad_list != null) {
				pad_list.styleListItem = ELStyles.listItem;
				pad_list.styleListBox = ELStyles.listBox;
				pad_list.DrawBlockingSelector ();
				control.builder.PadSelection_start ();
			}
		}

		public static void SelectPad (ELBuildControl selected_pad)
		{
			if (instance != null) {
				instance.Select_Pad (selected_pad);
			}
		}

		void Select_Pad (ELBuildControl selected_pad)
		{
			if (control != null && control != selected_pad) {
				control.builder.Highlight (false);
			}
			control = selected_pad;
			pad_list.SelectItem (launchpads.IndexOf (control));
			UpdateGUIState ();
		}

		void VesselName ()
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (control.builder.vessel.vesselName);
			GUILayout.FlexibleSpace ();
			GUILayout.Label (control.builder.vessel.situation.ToString ());
			double productivity = control.productivity;
			GUIStyle prodStyle = ELStyles.green;
			if (productivity <= 0) {
				prodStyle = ELStyles.red;
			} else if (productivity < 1) {
				prodStyle = ELStyles.yellow;
			}
			GUILayout.Label(LocalStrings.Productivity + ": " + productivity.ToString("G3"),//Productivity
							 prodStyle);
			GUILayout.EndHorizontal ();
		}

		void SelectPad ()
		{
			GUILayout.BeginHorizontal ();
			pad_list.DrawButton ();
			highlight_pad = GUILayout.Toggle (highlight_pad, LocalStrings.HighlightPad);//"Highlight Pad"
			Select_Pad (launchpads[pad_list.SelectedIndex]);
			GUILayout.EndHorizontal ();
			control.builder.PadSelection ();
		}

		void SelectPad_end ()
		{
			if (pad_list != null) {
				pad_list.DrawDropDown();
				pad_list.CloseOnOutsideClick();
				control.builder.PadSelection_end ();
			}
		}

		void SelectCraft ()
		{
			string strpath = HighLogic.SaveFolder;

			GUILayout.BeginHorizontal ();
			GUI.enabled = craftlist == null;
			if (GUILayout.Button (LocalStrings.SelectCraft, ELStyles.normal,//"Select Craft"
								  expandWidth)) {

				//GUILayout.Button is "true" when clicked
				craftlist = ELCraftBrowser.Spawn (control.craftType,
												  strpath,
												  craftSelectComplete,
												  craftSelectCancel,
												  false);
			}
			GUI.enabled = flagBrowser == null;
			if (GUILayout.Button (flagTexture, ELStyles.normal,
								  width48, height32, noExpandWidth)) {
				CreateFlagBrowser ();
			}
			GUI.enabled = control.craftConfig != null;
			if (GUILayout.Button (LocalStrings.Reload, ELStyles.normal, noExpandWidth)) {//"Reload"
				control.LoadCraft (control.filename, control.flagname);
			}
			if (GUILayout.Button (LocalStrings.Clear, ELStyles.normal, noExpandWidth)) {//"Clear"
				control.UnloadCraft ();
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal ();
		}

		void SelectedCraft ()
		{
			if (control.craftConfig != null) {
				var ship_name = control.craftName;
				GUILayout.Box (LocalStrings.SelectedCraft + ":	" + ship_name, ELStyles.white);//Selected Craft
			}
		}

		void LockedParts ()
		{
			GUILayout.Label (LocalStrings.PartLocked);//"Not all of the blueprints for this vessel can be found."
		}

		void ResourceHeader (string header)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (header, ELStyles.label, noExpandWidth);
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
		}

		void RequiredResources ()
		{
			ResourceHeader (LocalStrings.ResourcesRequired + ":");//Resources required to build
			foreach (var br in control.buildCost.required) {
				double a = br.amount;
				double available = -1;

				available = control.padResources.ResourceAmount (br.name);
				ResourceLine (br.name, br.name, 1.0f, a, a, available);
			}
		}

		void BuildButton ()
		{
			GUI.enabled = control.builder.canBuild;
			if (GUILayout.Button (LocalStrings.Build, ELStyles.normal, expandWidth)) {//"Build"
				control.BuildCraft ();
			}
			GUI.enabled = true;
		}

		void FinalizeButton ()
		{
			GUILayout.BeginHorizontal ();
			GUI.enabled = control.builder.canBuild;
			if (GUILayout.Button (LocalStrings.FinalizeBuild, ELStyles.normal,//"Finalize Build"
								  expandWidth)) {
				control.BuildAndLaunchCraft ();
			}
			GUI.enabled = true;
			if (GUILayout.Button (LocalStrings.TeardownBuild, ELStyles.normal,//"Teardown Build"
								  expandWidth)) {
				control.CancelBuild ();
			}
			GUILayout.EndHorizontal ();
		}

		internal static BuildResource FindResource (List<BuildResource> reslist, string name)
		{
			return reslist.Where(r => r.name == name).FirstOrDefault ();
		}

		void UpdateAlarm (double mostFutureAlarmTime, bool forward)
		{
			if (KACWrapper.APIReady && ELSettings.use_KAC) {
				if (control.paused) {
					// It doesn't make sense to have an alarm for an event that will never happen
					if (control.KACalarmID != "") {
						try {
							KACWrapper.KAC.DeleteAlarm (control.KACalarmID);
						}
						catch {
							// Don't crash if there was some problem deleting the alarm
						}
						control.KACalarmID = "";
					}
				} else if (mostFutureAlarmTime>0) {
					// Find the existing alarm, if it exists
					// Note that we might have created an alarm, and then the user deleted it!
					KACWrapper.KACAPI.KACAlarmList alarmList = KACWrapper.KAC.Alarms;
					KACWrapper.KACAPI.KACAlarm a = null;
					if ((alarmList != null) && (control.KACalarmID != "")) {
						//Debug.Log ("Searching for alarm with ID [" + control.KACalarmID + "]");
						a = alarmList.FirstOrDefault (z => z.ID == control.KACalarmID);
					}

					// set up the strings for the alarm
					string builderShipName = FlightGlobals.ActiveVessel.vesselName;
					string newCraftName = control.craftConfig.GetValue ("ship");

					string alarmMessage = Localizer.Format("#EL_UI_buildMessage", newCraftName);//"[EL] build: \"" +  + "\""
					string alarmNotes = Localizer.Format("#EL_UI_buildNotes", newCraftName, builderShipName);//"Completion of Extraplanetary Launchpad build of \"" +  + "\" on \"" +  + "\""
					if (!forward) { // teardown messages
						alarmMessage = Localizer.Format("#EL_UI_teardownMessage", newCraftName);//"[EL] teardown: \"" +  + "\""
						alarmNotes = Localizer.Format("#EL_UI_teardownNotes", newCraftName, builderShipName);//"Teardown of Extraplanetary Launchpad build of \"" +  + "\" on \"" +  + "\""
					}

					if (a == null) {
						// no existing alarm, make a new alarm
						control.KACalarmID = KACWrapper.KAC.CreateAlarm (KACWrapper.KACAPI.AlarmTypeEnum.Raw, alarmMessage, mostFutureAlarmTime);
						//Debug.Log ("new alarm ID: [" + control.KACalarmID + "]");

						if (control.KACalarmID != "") {
							a = KACWrapper.KAC.Alarms.FirstOrDefault (z => z.ID == control.KACalarmID);
							if (a != null) {
								a.AlarmAction = ELSettings.KACAction;
								a.AlarmMargin = 0;
								a.VesselID = FlightGlobals.ActiveVessel.id.ToString ();
							}
						}
					}
					if (a != null) {
						// Whether we created an alarm or found an existing one, now update it
						a.AlarmTime = mostFutureAlarmTime;
						a.Notes = alarmNotes;
						a.Name = alarmMessage;
					}
				}
			}
		}

		void BuildProgress (bool forward)
		{
			double longestETA = 0;
			foreach (var br in control.builtStuff.required) {
				var req = FindResource (control.buildCost.required, br.name);
				double eta = ResourceProgress (br.name, br, req, forward);
				if (eta > longestETA) {
					longestETA = eta;
				}
			}
			if (longestETA > 0 && longestETA > minimum_alarm_time) {
				double alarmTime = Planetarium.GetUniversalTime () + longestETA;
				UpdateAlarm (alarmTime, forward);
			}
		}

		void CraftBoM ()
		{
			if (control.craftBoM == null) {
				if (!control.CreateBoM ()) {
					return;
				}
			}
			for (int i = 0, count = control.craftBoM.Count; i < count; i++) {
				GUILayout.Label (control.craftBoM[i]);
			}
		}

		void OptionalResources ()
		{
			link_lfo_sliders = GUILayout.Toggle (link_lfo_sliders, LocalStrings.LinkLFOSliders);//"Link LiquidFuel and Oxidizer sliders"
			foreach (var br in control.buildCost.optional) {
				double available = control.padResources.ResourceAmount (br.name);
				double maximum = control.craftResources.ResourceCapacity(br.name);
				float frac = (float) (br.amount / maximum);
				frac = ResourceLine (br.name, br.name, frac, 0,
									 maximum, available);
				if (link_lfo_sliders
					&& (br.name == "LiquidFuel" || br.name == "Oxidizer")) {
					string other;
					if (br.name == "LiquidFuel") {
						other = "Oxidizer";
					} else {
						other = "LiquidFuel";
					}
					var or = FindResource (control.buildCost.optional, other);
					if (or != null) {
						double om = control.craftResources.ResourceCapacity (other);
						or.amount = om * frac;
					}
				}
				br.amount = maximum * frac;
			}
		}

		static string[] state_str = {
			LocalStrings.Build, LocalStrings.Teardown,//"Build""Teardown"
		};
		void PauseButton ()
		{
			int ind = control.state == ELBuildControl.State.Building ? 0 : 1;
			string statestr = state_str[ind];
			GUILayout.BeginHorizontal ();
			if (control.paused) {
				if (GUILayout.Button (LocalStrings.Resume + statestr, ELStyles.normal,//Resume
									  expandWidth)) {
					control.ResumeBuild ();
				}
			} else {
				if (GUILayout.Button (LocalStrings.Pause + statestr, ELStyles.normal,// Pause
									  expandWidth)) {
					control.PauseBuild ();
				}
			}
			if (control.state == ELBuildControl.State.Building) {
				if (GUILayout.Button (LocalStrings.CancelBuild, ELStyles.normal,//"Cancel Build"
									  expandWidth)) {
					control.CancelBuild ();
				}
			} else {
				if (GUILayout.Button (LocalStrings.RestartBuild, ELStyles.normal,//"Restart Build"
									  expandWidth)) {
					control.UnCancelBuild ();
				}
			}
			GUILayout.EndHorizontal ();
		}

		void ReleaseButton ()
		{
			if (GUILayout.Button (LocalStrings.Release, ELStyles.normal, expandWidth)) {//"Release"
				control.ReleaseVessel ();
			}
		}

		void CloseButton ()
		{
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button (LocalStrings.Close)) {//"Close"
				HideGUI ();
			}
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
		}

		void WindowGUI (int windowID)
		{
			ELStyles.Init ();

			SelectPad_start ();

			GUILayout.BeginVertical ();
			VesselName ();
			SelectPad ();

			switch (control.state) {
			case ELBuildControl.State.Idle:
				SelectCraft ();
				break;
			case ELBuildControl.State.Planning:
				SelectCraft ();
				SelectedCraft ();
				if (control.lockedParts) {
					LockedParts ();
				} else {
					resScroll.Begin ();
					RequiredResources ();
					CraftBoM ();
					resScroll.End ();
					BuildButton ();
				}
				break;
			case ELBuildControl.State.Building:
				SelectedCraft ();
				resScroll.Begin ();
				BuildProgress (true);
				resScroll.End ();
				PauseButton ();
				break;
			case ELBuildControl.State.Canceling:
				SelectedCraft ();
				resScroll.Begin ();
				BuildProgress (false);
				resScroll.End ();
				PauseButton ();
				break;
			case ELBuildControl.State.Complete:
				SelectedCraft ();
				FinalizeButton ();
				break;
			case ELBuildControl.State.Transfer:
				SelectedCraft ();
				resScroll.Begin ();
				OptionalResources ();
				resScroll.End ();
				ReleaseButton ();
				break;
			}

			GUILayout.EndVertical ();

			CloseButton ();

			SelectPad_end ();

			GUI.DragWindow (new Rect (0, 0, 10000, 20));

		}

		void OnFlagCancel ()
		{
			flagBrowser = null;
		}

		void OnFlagSelected (FlagBrowser.FlagEntry selected)
		{
			control.flagname = selected.textureInfo.name;
			flagTexture = selected.textureInfo.texture;
			UpdateFlagTexture ();
			flagBrowser = null;
		}

		private void craftSelectComplete (string filename,
										  CraftBrowserDialog.LoadType lt)
		{
			control.LoadCraft (filename, flagURL);
			control.craftType = craftlist.craftType;
			craftlist = null;
		}

		private void craftSelectCancel ()
		{
			craftlist = null;
		}

		void OnGUI ()
		{
			GUI.skin = HighLogic.Skin;
			windowpos = GUILayout.Window (GetInstanceID (),
										  windowpos, WindowGUI,
										  ELVersionReport.GetVersion (),
										  width695);
		}
	}
}

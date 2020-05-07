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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KSP.IO;

namespace ExtraplanetaryLaunchpads {

	[KSPAddon (KSPAddon.Startup.FlightAndEditor, false)]
	public class ELRenameWindow: MonoBehaviour
	{
		public interface IRenamable {
			string Name { get; set; }
			void OnRename ();
		}

		private static IRenamable target = null;
		private static ELRenameWindow windowInstance = null;
		private static Rect windowpos = new Rect(Screen.width * 0.35f,Screen.height * 0.1f,1,1);
		const string fieldName = "RenameWindow.ExtraplanetaryLaunchpads";
		private static TextField nameField = new TextField (fieldName);

		void Awake ()
		{
			windowInstance = this;
			enabled = false;
		}

		void OnDestroy ()
		{
			Debug.Log("[ELRenameWindow] OnDestroy");
			windowInstance = null;
			target = null;
		}

		public static void HideGUI ()
		{
			if (windowInstance != null) {
				windowInstance.enabled = false;
			}
			ClearControlLock ();
		}

		public static void ShowGUI (IRenamable renamable)
		{
			target = renamable;
			nameField.text = renamable.Name;
			if (windowInstance != null) {
				windowInstance.enabled = true;
			}
		}

		void RenamePad ()
		{
			if (target.Name != nameField.text) {
				target.Name = nameField.text;
				target.OnRename ();
			}
		}

		void RenameField ()
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (LocalStrings.Renamelaunchpad + ": ");//Rename launchpad

			if (nameField.HandleInput ()) {
				RenamePad ();
				HideGUI ();
			}
			GUILayout.EndHorizontal ();
		}

		void OKCancelButtons ()
		{
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button (LocalStrings.btn_OK)) {//"OK"
				nameField.AcceptInput ();
				RenamePad ();
				HideGUI ();
			}
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button (LocalStrings.Cancel)) {//"Cancel"
				HideGUI ();
			}
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
		}

		void WindowGUI (int windowID)
		{
			GUILayout.BeginVertical ();

			RenameField ();
			OKCancelButtons ();

			GUILayout.EndVertical ();

			GUI.DragWindow (new Rect (0, 0, 10000, 20));

		}

		static void SetControlLock ()
		{
			InputLockManager.SetControlLock ("EL_Rename_window_lock");
		}

		static void ClearControlLock ()
		{
			InputLockManager.RemoveControlLock ("EL_Rename_window_lock");
		}

		void OnGUI ()
		{
			GUI.skin = HighLogic.Skin;
			windowpos = GUILayout.Window (GetInstanceID (),
				windowpos, WindowGUI,
				LocalStrings.Renamelaunchpad,//"Rename Launchpad"
				GUILayout.Width(500));
			if (windowpos.Contains(Event.current.mousePosition)) {
				SetControlLock ();
			} else {
				ClearControlLock ();
			}
		}
	}
}

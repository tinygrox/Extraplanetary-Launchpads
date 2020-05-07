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

	public class ELDisposablePad : PartModule, IModuleInfo, IPartMassModifier, ELBuildControl.IBuilder, ELControlInterface, ELWorkSink, ELRenameWindow.IRenamable
	{
		[KSPField (isPersistant = false)]
		public string SpawnTransform;
		[KSPField (isPersistant = true, guiActive = true, guiName = "#EL_UI_PadName")]//Pad name
		public string PadName = "";

		[KSPField (isPersistant = true)]
		public bool Operational = true;

		[KSPField] public float EVARange = 0;

		Transform launchTransform;
		double craft_mass;

		public override string GetInfo ()
		{
			return "DisposablePad";
		}

		public string GetPrimaryField ()
		{
			return null;
		}

		public string GetModuleTitle ()
		{
			return "EL Disposable Pad";
		}

		public Callback<Rect> GetDrawModulePanelCallback ()
		{
			return null;
		}

		public bool canBuild
		{
			get {
				return canOperate;
			}
		}

		public bool capture
		{
			get {
				return true;
			}
		}

		public ELBuildControl control
		{
			get;
			private set;
		}

		public new Vessel vessel
		{
			get {
				return base.vessel;
			}
		}

		public new Part part
		{
			get {
				return base.part;
			}
		}

		public string Name
		{
			get {
				return PadName;
			}
			set {
				PadName = value;
			}
		}

		public string LandedAt { get { return ""; } }
		public string LaunchedFrom { get { return ""; } }

		public void PadSelection_start ()
		{
		}

		public void PadSelection ()
		{
		}

		public void PadSelection_end ()
		{
		}

		public void Highlight (bool on)
		{
			if (on) {
				part.SetHighlightColor (XKCDColors.LightSeaGreen);
				part.SetHighlight (true, false);
			} else {
				part.SetHighlightDefault ();
			}
		}

		public bool isBusy
		{
			get {
				return control.state > ELBuildControl.State.Planning;
			}
		}

		public bool canOperate
		{
			get { return Operational; }
			set { Operational = value; }
		}

		public void SetCraftMass (double mass)
		{
			craft_mass = mass;
		}

		public float GetModuleMass (float defaultMass, ModifierStagingSituation sit)
		{
			return (float) craft_mass;
		}

		public ModifierChangeWhen GetModuleMassChangeWhen ()
		{
			return ModifierChangeWhen.CONSTANTLY;
		}

		Part AttachmentPart ()
		{
			AttachNode node = part.FindAttachNode ("bottom");
			if (node == null || node.attachedPart == null) {
				node = part.srfAttachNode;
			}
			return node.attachedPart;
		}

		void PostCapture ()
		{
			Part attachPart = AttachmentPart ();
			if (vessel.rootPart == part) {
				// the pad is (somehow) the vessel's root part that will
				// cause problems when the pad is destroyed, so set the part
				// to which the spawn will be attached as the root
				attachPart.SetHierarchyRoot (attachPart);
			}
			Part spawnRoot = control.craftRoot;
			part.children.Remove (spawnRoot);

			attachPart.addChild (spawnRoot);
			spawnRoot.parent = attachPart;

			AttachNode spawnNode = FindNode (spawnRoot);
			spawnNode.attachedPart = attachPart;
			AttachNode attachNode = attachPart.FindAttachNodeByPart (part);
			if (attachNode != null) {
				attachNode.attachedPart = spawnRoot;
			}

			spawnRoot.CreateAttachJoint (attachPart.attachMode);

			control.DestroyPad ();
		}

		void SetLaunchTransform ()
		{
			if (SpawnTransform != "") {
				launchTransform = part.FindModelTransform (SpawnTransform);
				//Debug.LogFormat ("[EL] launchTransform:{0}:{1}",
				//				   launchTransform, SpawnTransform);
			}
			if (launchTransform == null) {
				launchTransform = part.FindModelTransform ("EL launch pos");
			}
			if (launchTransform == null) {
				Transform t = part.partTransform.Find("model");
				GameObject launchPos = new GameObject ("EL launch pos");
				launchPos.transform.SetParent (t, false);
				launchTransform = launchPos.transform;
				//Debug.LogFormat ("[EL] launchPos {0}", launchTransform);
			}
		}

		AttachNode FindNode (Part p)
		{
			AttachNode node;

			if ((node = p.FindAttachNode ("bottom")) != null) {
				if (node.attachedPart == null) {
					return node;
				}
			}
			if ((node = p.FindAttachNode ("top")) != null) {
				if (node.attachedPart == null) {
					return node;
				}
			}
			for (int i = 0; i < p.attachNodes.Count; i++) {
				node = p.attachNodes[i];
				if (node.attachedPart == null) {
					return node;
				}
			}
			return null;
		}

		public void SetShipTransform (Transform shipTransform, Part rootPart)
		{
			// Don't assume shipTransform == rootPart.transform
			Transform rootXform = rootPart.transform;
			AttachNode n = FindNode (rootPart);
			var rot = Quaternion.identity;
			var pos = Vector3.zero;
			if (n != null) {
				Vector3 nodeAxis = rootXform.TransformDirection(n.orientation);
				Vector3 forward = rootXform.forward;
				float fwdDot = Vector3.Dot (forward, nodeAxis);
				Debug.Log ($"[EL] nodeAxis: {nodeAxis}");
				Debug.Log ($"[EL] rotation: {rootXform.rotation} right:{rootXform.right} forward:{rootXform.forward} up:{rootXform.up}");
				if (Mathf.Abs (fwdDot) < 0.866f) {
					rot = Quaternion.LookRotation (nodeAxis, forward);
					rot = Quaternion.Inverse (rot);
					Debug.Log ($"[EL] {rot}");
					rot = Quaternion.LookRotation (Vector3.up, -Vector3.forward) * rot;
					Debug.Log ($"[EL] {Quaternion.LookRotation (Vector3.up, -Vector3.forward)}");
				} else {
					rot = Quaternion.LookRotation (nodeAxis, rootXform.right);
					rot = Quaternion.Inverse (rot);
					Debug.Log ($"[EL] {rot}");
					rot = new Quaternion (-0.5f, -0.5f, -0.5f, 0.5f) * rot;
				}
				pos = rootXform.TransformVector (n.position);
			}
			Debug.Log ($"[EL] pos: {pos} rot: {rot}");
			shipTransform.position = rot * -pos;
			shipTransform.rotation = rot * shipTransform.rotation;
			Debug.Log ($"[EL] position: {shipTransform.position} rotation: {shipTransform.rotation} right:{shipTransform.right} forward:{shipTransform.forward} up:{shipTransform.up}");
		}

		public Transform PlaceShip (Transform shipTransform, Box vessel_bounds)
		{
			SetLaunchTransform ();
			Vector3 pos = shipTransform.position;
			Quaternion rot = shipTransform.rotation;
			shipTransform.rotation = launchTransform.rotation * rot;
			shipTransform.position = launchTransform.TransformPoint (pos);
			return launchTransform;
		}

		public void PostBuild (Vessel craftVessel)
		{
		}

		public override void OnSave (ConfigNode node)
		{
			control.Save (node);
		}

		public override void OnLoad (ConfigNode node)
		{
			control.Load (node);
		}

		public override void OnAwake ()
		{
			control = new ELBuildControl (this);
			control.PostCapture = PostCapture;
		}

		public override void OnStart (PartModule.StartState state)
		{
			if (state == PartModule.StartState.None
				|| state == PartModule.StartState.Editor) {
				return;
			}
			if (EVARange > 0) {
				EL_Utils.SetupEVAEvent (Events["ShowRenameUI"], EVARange);
			}
			control.OnStart ();
		}

		void OnDestroy ()
		{
			if (control != null) {
				control.OnDestroy ();
			}
		}

		[KSPEvent (guiActive = true, guiName = "#EL_UI_HideUI", active = false)]//Hide UI
		public void HideUI ()
		{
			ELBuildWindow.HideGUI ();
		}

		[KSPEvent (guiActive = true, guiName = "#EL_UI_ShowUI", active = false)]//Show UI
		public void ShowUI ()
		{
			ELBuildWindow.ShowGUI ();
			ELBuildWindow.SelectPad (control);
		}

		[KSPEvent (guiActive = true, guiActiveEditor = true,
				   guiName = "#EL_UI_Rename", active = true)]//Rename
		public void ShowRenameUI ()
		{
			ELRenameWindow.ShowGUI (this);
		}

		public void UpdateMenus (bool visible)
		{
			Events["HideUI"].active = visible;
			Events["ShowUI"].active = !visible;
		}

		public void DoWork (double kerbalHours)
		{
			control.DoWork (kerbalHours);
		}

		public bool isActive
		{
			get {
				return control.isActive;
			}
		}

		public ELVesselWorkNet workNet
		{
			get {
				return control.workNet;
			}
			set {
				control.workNet = value;
			}
		}

		public double CalculateWork ()
		{
			return control.CalculateWork();
		}

		public void OnRename ()
		{
			ELBuildWindow.updateCurrentPads ();
		}
	}
}

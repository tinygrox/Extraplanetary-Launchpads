using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KSP.Localization;

namespace ExtraplanetaryLaunchpads
{
    public static class LocalStrings
    {
        //Settings
        public static string Alarmactions1 = Localizer.Format("#EL_UI_KillWarpAndMessage");// "Kill Warp+Message"
        public static string Alarmactions2 = Localizer.Format("#EL_UI_KillWarpOnly");// "Kill Warp only"
        public static string Alarmactions3 = Localizer.Format("#EL_UI_MessageOnly");// "Message Only"
        public static string Alarmactions4 = Localizer.Format("#EL_UI_PauseGame");// "Pause Game"
        public static string UseToolbar = Localizer.Format("#EL_UI_UseToolbar");// "Use Blizzy's toolbar instead of App launcher"
        public static string UseKAC = Localizer.Format("#EL_UI_UseKAC");// "Create alarms in Kerbal Alarm Clock"
        public static string ShowGUI = Localizer.Format("#EL_UI_VisibleInEditor");// "Build Resources window currently visible in editor"
        public static string ShowCraftHull = Localizer.Format("#EL_UI_ShowCrafthull");// "Show craft hull during construction"
        public static string DebugCraftHull = Localizer.Format("#EL_UI_DebugCraftHull");// "[Debug] Write craft hull points file"
        public static string AlarmType = Localizer.Format("#EL_UI_AlarmType");// "Alarm type"
        public static string MODNAME = Localizer.Format("#EL_UI_MODNAME"); //"Extraplanetary Launchpad"
        public static string btn_OK = Localizer.Format("#EL_UI_OK"); // "OK"

        //Ship Info
        public static string WindowTitle = Localizer.Format("#EL_UI_BuildResources"); // "Build Resources"
        public static string Drymass = Localizer.Format("#EL_UI_Drymass"); // "Dry mass"
        public static string ResourceMass = Localizer.Format("#EL_UI_ResourceMass"); // "Resource mass"
        public static string TotalMass = Localizer.Format("#EL_UI_TotalMass"); // "Total mass"
        public static string BuildTime = Localizer.Format("#EL_UI_BuildTime"); // "Build Time"
        public static string KerbalHourUnit = Localizer.Format("#EL_UI_KerbalHourUnit"); // "Kh"
        public static string Required = Localizer.Format("#EL_UI_Required");// "Required"
        public static string Optional = Localizer.Format("#EL_UI_Optional"); // "Optional"

        //Build Window
        public static string ResourceAmountTip = Localizer.Format("#EL_UI_ResourceAmountTip");// "Must be 100%"
        public static string Pausedtext = Localizer.Format("#EL_UI_Pausedtext"); // "[paused]"
        public static string Productivity = Localizer.Format("#EL_UI_Productivity"); // "Productivity"
        public static string HighlightPad = Localizer.Format("#EL_UI_HighlightPad"); // "Highlight Pad"
        public static string SelectCraft = Localizer.Format("#EL_UI_SelectCraft"); // "Select Craft"
        public static string Reload = Localizer.Format("#EL_UI_Reload"); // "Reload"
        public static string Clear = Localizer.Format("#EL_UI_Clear"); // "Clear"
        public static string SelectedCraft = Localizer.Format("#EL_UI_SelectedCraft"); // "Selected Craft"
        public static string PartLocked = Localizer.Format("#EL_UI_PartLocked");// "Not all of the blueprints for this vessel can be found."
        public static string ResourcesRequired = Localizer.Format("#EL_UI_ResourcesRequired");// "Resources required to build"
        public static string Build = Localizer.Format("#EL_UI_Build");// "Build"
        public static string Teardown = Localizer.Format("#EL_UI_Teardown");// "Teardown" 
        public static string FinalizeBuild = Localizer.Format("#EL_UI_FinalizeBuild");// "Finalize Build"
        public static string TeardownBuild = Localizer.Format("#EL_UI_TeardownBuild"); // "Teardown Build"
        public static string LinkLFOSliders = Localizer.Format("#EL_UI_LinkLFOSliders"); // "Link LiquidFuel and Oxidizer sliders"
        public static string Resume = Localizer.Format("#EL_UI_Resume");// "Resume"
        public static string Pause = Localizer.Format("#EL_UI_Pause"); // "Pause"
        public static string CancelBuild = Localizer.Format("#EL_UI_CancelBuild");// "Cancel Build"
        public static string RestartBuild = Localizer.Format("#EL_UI_RestartBuild");// "Restart Build"
        public static string Release = Localizer.Format("#EL_UI_Release"); // "Release"
        public static string Close = Localizer.Format("#EL_UI_Close");// "Close"

        //CraftBrowser
        public static string BrowserTitle = Localizer.Format("#EL_UI_BrowserTitle"); // "Select a craft to load"

        //Rename Window
        public static string Renamelaunchpad = Localizer.Format("#EL_UI_RenameLaunchpad");// "Rename launchpad"
        public static string Cancel = Localizer.Format("#EL_UI_Cancel");// "Cancel"

        //Resource Window
        public static string StopTransfer = Localizer.Format("#EL_UI_StopTransfer");// "Stop Transfer"
        public static string StartTransfer = Localizer.Format("#EL_UI_StartTransfer"); // "Start Transfer"
        public static string XferState_Hold = Localizer.Format("#EL_UI_XferState_Hold"); // "Hold"
        public static string XferState_In = Localizer.Format("#EL_UI_XferState_In"); // "In"
        public static string XferState_Out = Localizer.Format("#EL_UI_XferState_Out");// "Out"

        //Converter
        public static string Massflow = Localizer.Format("#EL_UI_Massflow"); // "Mass flow"
        public static string Heatflow = Localizer.Format("#EL_UI_Heatflow"); // "Heat flow"
        public static string Inputs = Localizer.Format("#EL_UI_Inputs");//"Inputs"
        public static string Outputs = Localizer.Format("#EL_UI_Outputs"); // "Outputs"
        public static string Brokenconfiguration = Localizer.Format("#EL_UI_BrokenConfiguration"); // "broken configuration"
        public static string ELConverter = Localizer.Format("#EL_UI_ELConverter"); // "EL Converter"
        public static string Operating = Localizer.Format("#EL_UI_Operating"); // "Operating"

        //Extractor
        public static string Requirements = Localizer.Format("#EL_UI_Requirements"); // "Requirements"
        public static string Status_stalled = Localizer.Format("#EL_UI_Statu_stalled");// "stalled"
        public static string Status_NoGroundContact = Localizer.Format("#EL_UI_Statu_noGroundContact"); // "no ground contact"
        public static string Status_insufficientabundance = Localizer.Format("#EL_UI_Statu_InsufficientAbundance"); // "insufficient abundance"
        public static string Status_Inactive = Inactive; // "Inactive"

        //Recycler
        public static string ELRecycler = Localizer.Format("#EL_UI_ELRecycler"); // "EL Recycler"
        public static string Active = Localizer.Format("#EL_UI_Active"); // "Active"
        public static string Inactive = Localizer.Format("#EL_UI_Inactive"); // "Inactive"

        public static string gender_She = Localizer.Format("#EL_UI_gender_She"); // "She"
        public static string gender_He = Localizer.Format("#EL_UI_gender_He"); // "He"

        //Survey Stacke
        public static string Bound = Localizer.Format("#EL_UI_Bound"); // "Bound"
        public static string Direction = Localizer.Format("#EL_UI_Direction"); // "Direction"
        public static string SurveyStation = Localizer.Format("#EL_UI_SurveyStation");  // "Survey Station"
        public static string RenameSite = Localizer.Format("#EL_UI_RenameSite"); // "Rename Site"
        public static string NoSitesFound = Localizer.Format("#EL_UI_NoSitesFound"); // "No sites found. Explosions likely."
        public static string NoSitesFound2 = Localizer.Format("#EL_UI_NoSitesFound2"); // "No sites found."

        //Construction Skill
        public static string Experiencedesc1 = " " + Localizer.Format("#EL_UI_ExperienceDesc1");//" can work in a fully equipped workshop."
        public static string Experiencedesc2 = " " + Localizer.Format("#EL_UI_ExperienceDesc2");//" can work in any workshop."
        public static string Experiencedesc3 = " " + Localizer.Format("#EL_UI_ExperienceDesc3");//" is always productive in a fully equipped workshop."
        public static string Experiencedesc4 = " " + Localizer.Format("#EL_UI_ExperienceDesc4");//" is always productive in any workshop."
        public static string Experiencedesc5 = " " + Localizer.Format("#EL_UI_ExperienceDesc5");//" enables skilled workers in any workshop."
        public static string Experiencedesc6 = " " + Localizer.Format("#EL_UI_ExperienceDesc6");//" enables unskilled workers in a fully equipped workshop."
    }
}

using BepInEx;
using HarmonyLib;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MikageMod
{
    [BepInPlugin("com.admiraldock.fp2.mikagemod", MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("FP2.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static AssetBundle moddedBundle;
        public static GameObject mikageObject;
        private void Awake()
        {
            // Plugin startup logic
            string assetPath = Path.Combine(Path.GetFullPath("."), "mod_overrides");
            moddedBundle = AssetBundle.LoadFromFile(Path.Combine(assetPath, "mikage.assets"));
            if (moddedBundle == null)
            {
                Logger.LogError("Asset file not found, please reinstall the mod!");
                return;
            }
            //This is where the fun begins
            var harmony = new Harmony("com.admiraldock.fp2.mikagemod");
            harmony.PatchAll(typeof(PatchMikage));
            harmony.PatchAll(typeof(PatchInstanceNPC));
            harmony.PatchAll(typeof(PatchNPCList));
        }

        class PatchMikage
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(FPHubNPC), nameof(FPHubNPC.OnActivation), MethodType.Normal)]
            static void Postfix(string ___NPCName)
            {
                if(___NPCName == "Pedro")
                {
                    FPStage.ValidateStageListPos(mikageObject.GetComponent<FPHubNPC>());
                }
            }
        }

        class PatchInstanceNPC
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(FPPlayer), "Start", MethodType.Normal)]
            static void Postfix()
            {
                if (FPStage.stageNameString == "Paradise Prime")
                {
                    Object[] modMikagePre = moddedBundle.LoadAllAssets();
                    foreach (var mod in modMikagePre)
                    {
                        if (mod.GetType() == typeof(GameObject))
                        {
                            mikageObject = (GameObject)Instantiate(mod);
                            mikageObject.name = "NPC_Mikage";
                        }
                    }
                }
            }
        }

        class PatchNPCList
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(FPSaveManager), nameof(FPSaveManager.LoadFromFile), MethodType.Normal)]
            static void Postfix(ref string[] ___npcNames)
            {
                if (!(___npcNames.Contains("01 04 Mikage")))
                {
                    ___npcNames = ___npcNames.AddToArray("01 04 Mikage");
                }
                if (FPSaveManager.npcFlag.Length < ___npcNames.Length)
                {
                    FPSaveManager.npcFlag = FPSaveManager.ExpandByteArray(FPSaveManager.npcFlag, ___npcNames.Length);
                }
                if (FPSaveManager.npcDialogHistory.Length < ___npcNames.Length)
                {
                    FPSaveManager.npcDialogHistory = FPSaveManager.ExpandNPCDialogHistory(FPSaveManager.npcDialogHistory, ___npcNames.Length);
                }

                int id = FPSaveManager.GetNPCNumber("Mikage");
                if (FPSaveManager.npcDialogHistory[id].dialog.Length != 3 && FPSaveManager.npcDialogHistory[id].dialog != null)
                {
                    FPSaveManager.npcDialogHistory[id].dialog = new bool[3];
                }
                else if (id != 0 && FPSaveManager.npcDialogHistory[id].dialog == null) {
                    FPSaveManager.npcDialogHistory[id].dialog = new bool[3];
                }
            }
        }
    }
}
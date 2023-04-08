using BepInEx;
using HarmonyLib;
using Receiver2;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace SR25_plugin
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, "1.0.0")]
    public class Loader : BaseUnityPlugin
    {
        private static GameObject m10;
        private static GameObject intro_tile_with_gun;
        private static Vector3 pos_gun = new Vector3(2.7379f, 200.1879f, 35.5088f);
        private static Quaternion rot_gun = new Quaternion(0.1706f, 0.3219f, 0.0747f, -0.9283f);
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Harmony.CreateAndPatchAll(this.GetType());
        }
        [HarmonyPatch(typeof(ReceiverCoreScript), "SpawnPlayer")]
        [HarmonyPostfix]
        private static void OnStartIntro(ref ReceiverCoreScript __instance)
        {
            if (__instance.game_mode.GetGameMode() != GameMode.RankingCampaign) return;
            if (__instance.player.lah.loadout == null) return;
            if (__instance.player.lah.loadout.gun_internal_name != "Ciarencew.SR25") return;
            if (((RankingProgressionGameMode)__instance.game_mode).progression_data.receiver_rank == 0)
            {
                intro_tile_with_gun = RuntimeTileLevelGenerator.instance.GetTiles()[2];
                m10 = (intro_tile_with_gun.transform.Find("model_10(Clone)")).gameObject;
                UnityEngine.Object.Destroy(m10);
                InventoryItem gun;
                if (__instance.TryGetItemPrefab<InventoryItem>(__instance.player.lah.loadout.gun_internal_name, out gun))
                {
                    UnityEngine.Object.Instantiate<InventoryItem>(gun, pos_gun, rot_gun, intro_tile_with_gun.transform).Move(null);
                }
            }
        }
        /*private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                var RCS = ReceiverCoreScript.Instance();
                if (RCS.game_mode.GetGameMode() != GameMode.RankingCampaign) return;
                if (((RankingProgressionGameMode)RCS.game_mode).progression_data.receiver_rank == 0)
                {
                    intro_tile_with_gun = RuntimeTileLevelGenerator.instance.GetTiles()[2];
                    m10 = (intro_tile_with_gun.transform.Find("model_10(Clone)")).gameObject;
                    /*test.localPosition = Vector3.zero;
                    test.localRotation = Quaternion.identity;
                    test.CopyPosRot(m10.transform);
                    UnityEngine.Object.Destroy(m10);
                    InventoryItem gun;
                    RCS.TryGetItemPrefab<InventoryItem>(RCS.CurrentLoadout.gun_internal_name, out gun);
                    UnityEngine.Object.Instantiate<InventoryItem>(gun, pos_gun, rot_gun, intro_tile_with_gun.transform).Move(null);
                }
            }
        }*/
    }
}

using BepInEx;
using HarmonyLib;
using Receiver2;
using System.Linq;
using UnityEngine;

namespace SR25_plugin
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Loader : BaseUnityPlugin
    {
        private static Transform spawn_gun_pos = new Transform();
        private static ReceiverCoreScript RCS;
        private static GameObject m10;
        private static GameObject intro_tile_with_gun;
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            //Harmony.CreateAndPatchAll(this.GetType());
        }
        /*[HarmonyPatch(typeof(ReceiverCoreScript), "SpawnPlayer")]
        [HarmonyPostfix]
        private static void OnStartIntro()
        {
            RCS = ReceiverCoreScript.Instance();
            if (RCS.game_mode.GetGameMode() != GameMode.RankingCampaign) return;
            if (((RankingProgressionGameMode)RCS.game_mode).progression_data.receiver_rank == 0)
            {
                intro_tile_with_gun = RuntimeTileLevelGenerator.instance.GetTiles()[2];
                m10 = (intro_tile_with_gun.transform.Find("model_10(Clone)")).gameObject;
                spawn_gun_pos.localRotation = m10.transform.localRotation;
                spawn_gun_pos.localPosition = m10.transform.localPosition;
                UnityEngine.Object.Destroy(m10);
                var cool_gun = UnityEngine.Object.Instantiate<GunScript>(FUCK(RCS.CurrentLoadout.gun_internal_name));
                cool_gun.transform.parent = intro_tile_with_gun.transform;
                cool_gun.transform.localPosition = spawn_gun_pos.localPosition;
                cool_gun.transform.localRotation = spawn_gun_pos.localRotation;
            }
        }
        private static GunScript FUCK(string gun_internal_name)
        {
            foreach (GunScript gunscript in RCS.generic_prefabs.OfType<GunScript>())
            {
                if (gunscript.InternalName == gun_internal_name)
                {
                    return gunscript;
                }
            }
            return null;
        }*/
    }
}

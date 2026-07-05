using BepInEx;
using HarmonyLib;
using Receiver2;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace SR25_plugin
{
    [BepInPlugin("CiarenceW.SR-25", "SR-25", "1.0.0")]
    public class Loader : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin SR-25 is loaded!");
        }
    }
}

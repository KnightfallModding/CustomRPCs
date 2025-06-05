using HarmonyLib;
using Il2CppPhoton.Pun;
using Il2CppSystem.Reflection;
using Il2CppExitGames.Client.Photon;
using Il2CppSCG = Il2CppSystem.Collections.Generic;

namespace CustomRPCs;

[HarmonyPatch]
public class CustomRPCPatch
{
    [HarmonyPatch(typeof(PhotonNetwork), nameof(PhotonNetwork.StaticReset))]
    [HarmonyPrefix]
    private static void StaticResetPatch()
    {
        CustomRPCsHandler.LoadRPCsFromAttribute();
        CustomRPCsHandler.RegisterNames();
    }

    [HarmonyPatch(typeof(SupportClass), nameof(SupportClass.GetMethods))]
    [HarmonyPostfix]
    private static void GetMethodsPatch(Il2CppSystem.Type type, Il2CppSystem.Type attribute, ref Il2CppSCG.List<MethodInfo> __result)
    {
        // Only include custom RPCs when looking for `PunRPC` attribute
        if (type == null || attribute != PhotonNetwork.typePunRPC)
            return;

        foreach (MethodInfo rpc in CustomRPCsHandler.FindOfType(type))
            __result.AddIfMissing(rpc);
    }
}
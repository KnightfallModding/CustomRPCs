using MelonLoader;
using UnityEngine;
using System.Linq;
using Il2CppPhoton.Pun;
using System.Collections.Generic;

using SR = System.Reflection;
using Il2CppSR = Il2CppSystem.Reflection;

namespace CustomRPCs;

public class CustomRPCsHandler
{
    public static List<string> CustomRPCMethodsList = [];

    /// <summary>
    /// Store full names of all Mono methods with
    /// Attribute `[CustomRPC]` before the attributes
    /// get lost during `RegisterTypeInIl2Cpp()`.
    /// </summary>
    public static void LoadRPCsFromAttribute()
    {
        List<string> customRpcs = GetAll_FullNames();
        customRpcs.ForEach(rpc => CustomRPCMethodsList.AddIfMissing(rpc));
    }

    /// <summary>
    /// List names for all Mono methods inheriting
    /// MonoBehavior with attribute `[CustomRPC]`.
    /// 
    /// The name format is `namespace.class.methodname`.
    /// </summary>
    /// <returns></returns>
    public static List<string> GetAll_FullNames()
    {
        return System.AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => !a.FullName.StartsWith("UnityEngine.") && !a.FullName.StartsWith("System.")) // cause exceptions + not interesting
        .SelectMany(a => a.GetTypes())
        .Where(t => t.IsSubclassOf(typeof(MonoBehaviour)))
        .SelectMany(t => t
            .GetMethods(SR.BindingFlags.Public | SR.BindingFlags.NonPublic | SR.BindingFlags.Instance)
            .Where(m => m.IsDefined(CustomRPC.TYPE, false))
            .Select(m => $"{t.FullName}.{m.Name}"))
        .ToList();
    }

    /// <summary>
    /// Add all CustomRPC method names found in all Mono assemblies
    /// to `PhotonNetwork.PhotonServerSettings.RpcList`.
    /// 
    /// This uses Reflection and is based on the code from
    /// `PhotonEditor.UpdateRpcList` (from Unity Assets Store).
    /// </summary>
    public static void RegisterNames()
    {
        if (PhotonNetwork.PhotonServerSettings is null)
            return;

        var additionalRpcs = new List<string>();
        foreach (var fullName in CustomRPCMethodsList)
        {
            string rpcName = fullName.Split(".")[^1];
            if (!PhotonNetwork.PhotonServerSettings.RpcList.Contains(rpcName) && !additionalRpcs.Contains(rpcName))
                additionalRpcs.Add(rpcName);
        }

        if (additionalRpcs.Count <= 0)
        {
            Melon<Plugin>.Logger.Msg($"[RegisterNames] No custom RPC found.");
            return;
        }

        if (additionalRpcs.Count + PhotonNetwork.PhotonServerSettings.RpcList.Count >= byte.MaxValue)
        {
            Melon<Plugin>.Logger.Error($"[RegisterNames] Max RPC count (255) reached! Can't add more custom RPCs.");
            return;
        }

        additionalRpcs.Sort();
        additionalRpcs.ForEach(rpc =>
        {
            PhotonNetwork.PhotonServerSettings.RpcList.Add(rpc);
            Melon<Plugin>.Logger.Msg($"[RegisterNames] -> Adding custom RPC: {rpc}");
        });
    }

    /// <summary>
    /// Returns Il2Cpp.MethodInfo for all methods which
    /// contained [CustomRPC] and were registered
    /// through `CustomRPC.RegisterAttributes()`.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<Il2CppSR.MethodInfo> FindOfType(Il2CppSystem.Type type)
    {
        var customRpcs = new List<Il2CppSR.MethodInfo>();

        foreach (var method in type.GetMethods())
        {
            string fullName = $"{type.FullName}.{method.Name}";
            if (CustomRPCMethodsList.Contains(fullName))
                customRpcs.Add(method);
        }

        return customRpcs;
    }
}
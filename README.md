# Custom RPCs

## Why ?

Short answer: Il2Cpp.

Long answer:

We can't use the built-in [PunRPC] Attribute to define our methods as valid RPCs in Photon.
This is due to Il2Cpp handling [PunRPC] as `Il2CppSystem.Attribute`, which is not a valid `System.Attribute` that can be used on methods anymore.

To counter this, we create a custom Attribute [CustomRPC] and we hook into the Photon logic flow to add our `RPCA_` methods as valid PunRPC methods.

## Development

1. Add `CustomRPCs.dll` as a dependency in your `csproj`:

    ```xml
    <Reference Include="CustomRPCs">
        <HintPath>Libs\CustomRPCs.dll</HintPath>
    </Reference>
    ```
2. Add the `[CustomRPC]` attribute to your `RPCA_` methods.
3. Profit!


## Runtime

Add `CustomRPCs.dll` as a classic Melon mod in `Mods/`.

At game start, you should see:

```
Loading Mods...

------------------------------
Melon Assembly loaded: '.\Mods\CustomRPCs.dll'
------------------------------

------------------------------
CustomRPCs vX.X.X
by t1nquen
Assembly: CustomRPCs.dll
------------------------------
```

## Example

The following example creates a [CustomRPC] method `RPCA_SetScale(float scale)` allowing to change the size of a player on network.  

Note that the `ScalingTool` component MUST be added **client-side** on all Player views for the RPCA to work.  
This is done through the `ComponentAdder` patch.
  
Since `ScalingTool` is a custom class, it MUST include the [RegisterTypeInIl2Cpp] attribute before using `AddComponent<>()`

For demo purposes, you can trigger a random scale change on your player by pressing the `H` key.

```C#
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using HarmonyLib;
using Il2CppPhoton.Pun;

using CustomRPCs;

namespace HideNSeek;

[RegisterTypeInIl2Cpp]
public class ScalingTool : MonoBehaviourPun
{
    private Player player;
    void Start() => player = GetComponent<Player>();
    void Update()
    {
        if (player.IsLocalPlayer() && Input.GetKeyDown(KeyCode.H))
            Call_SetScale(Random.Range(0f, 2f));
    }

    /// <summary>
    /// Call `RPCA_SetScale(scale)` on all clients.
    /// </summary>
    /// <param name="scale"></param>
    public void Call_SetScale(float scale)
    {
        player.refs.view.RPC("RPCA_SetScale", RpcTarget.All, scale);
    }

    /// <summary>
    /// Define custom RPCA with [CustomRPC]
    /// so that it is registered to Photon.
    /// </summary>
    /// <param name="scale"></param>
    [CustomRPC]
    public void RPCA_SetScale(float scale)
    {
        player.transform.localScale = new Vector3(scale, scale, scale);
    }
}

[HarmonyPatch]
public static class ComponentAdder
{
    /// <summary>
    /// Add our custom Pun component to all players.
    /// Using a patch ensures that players are actually
    /// defined defined at the time we add the component(s).
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPatch(typeof(Player), nameof(Player.Start))]
    [HarmonyPostfix]
    private static void StartPatch(Player __instance)
    {
        __instance.gameObject.AddComponent<ScalingTool>();
    }
}
```
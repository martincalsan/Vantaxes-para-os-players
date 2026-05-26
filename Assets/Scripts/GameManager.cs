using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    readonly NetworkVariable<int> _mode = new(0);
    static readonly string[] ModeNames = { "Server Authority", "Server + Prediction", "Client Authority" };

    const float MinInterval    = 5f;
    const float MaxInterval    = 15f;
    const float EffectDuration = 10f;

    public int Mode => _mode.Value;

    void Awake() => Instance = this;

    public override void OnNetworkSpawn()
    {
        if (IsServer) StartCoroutine(EffectLoop());
    }

    void OnGUI()
    {
        if (NetworkManager.Singleton == null) return;

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host"))   NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
            return;
        }

        GUILayout.Label($"Modo: {ModeNames[_mode.Value]}");
        if (GUILayout.Button("Cambiar Modo")) RequestModeChangeServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void RequestModeChangeServerRpc() => _mode.Value = (_mode.Value + 1) % 3;

IEnumerator EffectLoop()
{
    while (true)
    {
        yield return new WaitForSeconds(Random.Range(MinInterval, MaxInterval));

        var available = new List<PlayerMovement>();
        foreach (var p in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
            if (p.CurrentEffect == 0) available.Add(p);

        if (available.Count == 0) continue;

        var target = available[Random.Range(0, available.Count)];
        target.ApplyEffect(Random.Range(0, 2) == 0 ? 1 : 2, EffectDuration);
    }
}
}
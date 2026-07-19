using Fusion;
using UnityEngine;

public class NetworkLobby : NetworkBehaviour
{
    private GameLauncher launcher;
    private string targetScene;

    private bool[] readyStates = new bool[4];
    private int[] characterSelections = { -1, -1, -1, -1 };

    public static NetworkLobby Instance { get; private set; }

    public System.Action OnStateChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Init(GameLauncher gameLauncher, string sceneName)
    {
        launcher = gameLauncher;
        targetScene = sceneName;
    }

    public int GetPlayerIndex(PlayerRef player)
    {
        if (Runner == null) return -1;
        int i = 0;
        foreach (var p in Runner.ActivePlayers)
        {
            if (p == player) return i;
            i++;
        }
        return -1;
    }

    public bool IsReady(int index)
    {
        if (index < 0 || index >= 4) return false;
        return readyStates[index];
    }

    public int GetCharacter(int index)
    {
        if (index < 0 || index >= 4) return 0;
        return characterSelections[index];
    }

    public int PlayerCount
    {
        get
        {
            if (Runner == null) return 0;
            int c = 0;
            foreach (var _ in Runner.ActivePlayers) c++;
            return c;
        }
    }

    public bool AllReady
    {
        get
        {
            for (int i = 0; i < PlayerCount; i++)
                if (!readyStates[i]) return false;
            return true;
        }
    }

    public bool AllCharactersSelected
    {
        get
        {
            for (int i = 0; i < PlayerCount; i++)
                if (characterSelections[i] < 0) return false;
            return true;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetReady(PlayerRef player, NetworkBool ready)
    {
        int index = GetPlayerIndex(player);
        if (index >= 0) readyStates[index] = ready;
        BroadcastState();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetCharacter(PlayerRef player, int characterIndex)
    {
        int index = GetPlayerIndex(player);
        if (index >= 0) characterSelections[index] = characterIndex;
        BroadcastState();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestState()
    {
        BroadcastState();
    }

    private void BroadcastState()
    {
        if (!Object.HasStateAuthority) return;

        int playerCount = PlayerCount;
        int readyMask = 0;
        for (int i = 0; i < 4; i++)
        {
            if (readyStates[i]) readyMask |= (1 << i);
        }

        RPC_SyncState(playerCount, readyMask,
            characterSelections[0], characterSelections[1],
            characterSelections[2], characterSelections[3]);

        if (OnStateChanged != null)
            OnStateChanged.Invoke();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SyncState(int playerCount, int readyMask,
        int char0, int char1, int char2, int char3)
    {
        for (int i = 0; i < 4; i++)
        {
            readyStates[i] = (readyMask & (1 << i)) != 0;
        }
        characterSelections[0] = char0;
        characterSelections[1] = char1;
        characterSelections[2] = char2;
        characterSelections[3] = char3;

        if (OnStateChanged != null)
            OnStateChanged.Invoke();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_NotifyNotAllReady()
    {
        LobbyUI ui = FindFirstObjectByType<LobbyUI>();
        if (ui != null) ui.ShowNotAllReadyPopup();
    }

    public void TryStartGame()
    {
        if (!Object.HasStateAuthority) return;

        if (!AllCharactersSelected)
        {
            Debug.LogWarning("[NetworkLobby] Cannot start game until all players select a character.");
            RPC_NotifyNotAllReady();
            return;
        }

        if (!AllReady)
        {
            RPC_NotifyNotAllReady();
            return;
        }

        if (launcher == null || string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError("[NetworkLobby] Missing launcher or target scene.");
            return;
        }

        launcher.LoadGameScene(targetScene);
    }
}

using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class CharacterSelectManager : MonoBehaviourPunCallbacks
{
    public const string PROP_READY = "ready";
    private const string ROOM_SLOT_PREFIX = "roleSlot_"; // roleSlot_0..3，值=actorNumber，0表示空

    [Header("Role Buttons (0~3)")]
    public Button[] roleButtons = new Button[4];
    public Text[] roleButtonTexts = new Text[4];   // 每个按钮上显示 Free/Taken 的Text（拖进去）

    [Header("Hero Names (0~3)")]
    public string[] heroNames = new string[4]
    {
        "Flare",
        "Blossom",
        "Leaf",
        "Volt"
    };

    [Header("Bottom UI")]
    public Button readyButton;
    public Button startButton;                     // 只有Master可用
    public Text statusText;
    public Text myChoiceText;

    private int myRole = -1;
    private int pendingRole = -1;
    private bool claimInProgress = false;

    private int LocalActor => PhotonNetwork.LocalPlayer.ActorNumber;

    private string SlotKey(int i) => ROOM_SLOT_PREFIX + i;

    private void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[CharacterSelect] Not in room.");
            return;
        }

        EnsureRoomSlotsInitialized();
        TryResolveMyRoleFromRoomSlots();
        RefreshUI();
    }

    private void EnsureRoomSlotsInitialized()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Hashtable set = new Hashtable();
        bool needSet = false;

        for (int i = 0; i < 4; i++)
        {
            string k = SlotKey(i);
            if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(k))
            {
                set[k] = 0; // 0 = free
                needSet = true;
            }
        }

        if (needSet)
            PhotonNetwork.CurrentRoom.SetCustomProperties(set);
    }

    private void TryResolveMyRoleFromRoomSlots()
    {
        myRole = -1;

        for (int i = 0; i < 4; i++)
        {
            int owner = GetSlotOwner(i);
            if (owner == LocalActor)
            {
                myRole = i;
                break;
            }
        }
    }

    // UI按钮：绑定到 4 个按钮的 OnClick（传 0~3）
    public void OnClickChooseRole(int roleIndex)
    {
        if (!PhotonNetwork.InRoom) return;
        if (roleIndex < 0 || roleIndex > 3) return;
        if (claimInProgress) return;

        if (myRole == roleIndex) return;

        int owner = GetSlotOwner(roleIndex);
        if (owner != 0 && owner != LocalActor)
        {
            SetStatus($"Role {roleIndex} already taken.");
            RefreshUI();
            return;
        }

        if (myRole != -1)
        {
            ReleaseRoleSlot(myRole);
        }

        pendingRole = roleIndex;
        claimInProgress = true;

        Hashtable set = new Hashtable { { SlotKey(roleIndex), LocalActor } };
        Hashtable expected = new Hashtable { { SlotKey(roleIndex), 0 } };

        PhotonNetwork.CurrentRoom.SetCustomProperties(set, expected);

        RefreshUI();
    }

    public void OnClickReady()
    {
        if (!PhotonNetwork.InRoom) return;

        if (myRole == -1)
        {
            SetStatus("Please choose a role first.");
            return;
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
        {
            { PROP_READY, true },
            { PlayerSpawner.PROP_ROLE, myRole },
        });

        SetStatus("Ready!");
        RefreshUI();
    }

    public void OnClickStartLevel1()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (!AllPlayersReadyAndSelected())
        {
            SetStatus("Not everyone is ready.");
            return;
        }

        PhotonNetwork.LoadLevel("Level 1");
    }

    private bool AllPlayersReadyAndSelected()
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (!p.CustomProperties.ContainsKey(PlayerSpawner.PROP_ROLE)) return false;

            if (!p.CustomProperties.TryGetValue(PROP_READY, out object rv)) return false;
            if (!(rv is bool b) || !b) return false;
        }
        return true;
    }

    private void ReleaseRoleSlot(int roleIndex)
    {
        if (!PhotonNetwork.InRoom) return;
        if (roleIndex < 0 || roleIndex > 3) return;

        Hashtable set = new Hashtable { { SlotKey(roleIndex), 0 } };
        Hashtable expected = new Hashtable { { SlotKey(roleIndex), LocalActor } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(set, expected);
    }

    private int GetSlotOwner(int roleIndex)
    {
        if (!PhotonNetwork.InRoom) return 0;

        string k = SlotKey(roleIndex);
        if (PhotonNetwork.CurrentRoom.CustomProperties != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(k, out object v) &&
            v is int actor)
        {
            return actor;
        }
        return 0;
    }

    private Player FindPlayerByActor(int actor)
    {
        foreach (var p in PhotonNetwork.PlayerList)
            if (p.ActorNumber == actor) return p;
        return null;
    }

    // ✅ 新增：把 roleIndex 显示成英雄名（无则回退 Role i）
    private string GetHeroName(int i)
    {
        if (heroNames != null && i >= 0 && i < heroNames.Length && !string.IsNullOrEmpty(heroNames[i]))
            return heroNames[i];
        return $"Role {i}";
    }

    private void RefreshUI()
    {
        if (!PhotonNetwork.InRoom) return;

        for (int i = 0; i < 4; i++)
        {
            int owner = GetSlotOwner(i);
            string label;
            string hero = GetHeroName(i);

            if (owner == 0)
            {
                label = $"{hero}\nFree";
                roleButtons[i].interactable = true;
            }
            else
            {
                var p = FindPlayerByActor(owner);
                string name = p != null ? p.NickName : ("Actor " + owner);

                if (owner == LocalActor)
                {
                    label = $"{hero}\n(Yours)";
                    roleButtons[i].interactable = true;
                }
                else
                {
                    label = $"{hero}\nTaken: {name}";
                    roleButtons[i].interactable = false;
                }
            }

            if (roleButtonTexts != null && i < roleButtonTexts.Length && roleButtonTexts[i] != null)
                roleButtonTexts[i].text = label;
        }

        if (myChoiceText != null)
        {
            if (myRole == -1) myChoiceText.text = "My Role: (none)";
            else myChoiceText.text = $"My Role: {GetHeroName(myRole)}";
        }

        if (startButton != null)
        {
            startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
            startButton.interactable = PhotonNetwork.IsMasterClient && AllPlayersReadyAndSelected();
        }
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (claimInProgress && pendingRole != -1 && propertiesThatChanged.ContainsKey(SlotKey(pendingRole)))
        {
            int owner = GetSlotOwner(pendingRole);

            if (owner == LocalActor)
            {
                myRole = pendingRole;

                PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
                {
                    { PlayerSpawner.PROP_ROLE, myRole },
                    { PROP_READY, false }
                });

                SetStatus($"Selected {GetHeroName(myRole)}");
            }
            else
            {
                SetStatus($"Role {pendingRole} was taken by someone else.");
            }

            pendingRole = -1;
            claimInProgress = false;
        }

        RefreshUI();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) => RefreshUI();

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < 4; i++)
            {
                if (GetSlotOwner(i) == otherPlayer.ActorNumber)
                {
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { SlotKey(i), 0 } });
                }
            }
        }

        RefreshUI();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        RefreshUI();
    }
}

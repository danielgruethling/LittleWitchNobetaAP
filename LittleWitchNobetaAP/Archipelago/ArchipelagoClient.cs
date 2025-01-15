using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Il2Cpp;
using LittleWitchNobetaAP.Utils;
using MelonLoader;
using UnityEngine;

namespace LittleWitchNobetaAP.Archipelago;

public class ArchipelagoClient : MonoBehaviour
{
    public const string APVersion = "0.5.1";
    private const string Game = "Little Witch Nobeta";

    public static bool Authenticated;
    private bool _attemptingConnection;

    public static readonly ArchipelagoSessionData ServerData = new();
    public DeathLinkHandler? DeathLinkHandler;
    private static ArchipelagoSession? _session;

    /// <summary>
    /// call to connect to an Archipelago session. Connection info should already be set up on ServerData
    /// </summary>
    /// <returns></returns>
    public void Connect ()
    {
        if (Authenticated || _attemptingConnection) return;

        try
        {
            _session = ArchipelagoSessionFactory.CreateSession(ServerData.Hostname, Convert.ToInt32(ServerData.Port));
            SetupSession();
        }
        catch (Exception e)
        {
            Melon<LwnApMod>.Logger.Error(e);
        }

        TryConnect();
    }

    /// <summary>
    /// add handlers for Archipelago events
    /// </summary>
    private void SetupSession ()
    {
        if (_session == null) return;
        _session.MessageLog.OnMessageReceived += message => ArchipelagoConsole.LogMessage(message.ToString());
        _session.Items.ItemReceived += OnItemReceived;
        _session.Socket.ErrorReceived += OnSessionErrorReceived;
        _session.Socket.SocketClosed += OnSessionSocketClosed;
    }

    /// <summary>
    /// attempt to connect to the server with our connection info
    /// </summary>
    private void TryConnect ()
    {
        try
        {
            // it's safe to thread this function call but unity notoriously hates threading so do not use excessively
            ThreadPool.QueueUserWorkItem(
                _ => HandleConnectResult(
                    _session?.TryConnectAndLogin(
                        Game,
                        ServerData.SlotName,
                        ItemsHandlingFlags.AllItems, // TODO make sure to change this line
                        new Version(APVersion),
                        password: ServerData.Password,
                        requestSlotData: true // ServerData.NeedSlotData
                    ) ?? throw new ArgumentNullException()));
        }
        catch (Exception e)
        {
            Melon<LwnApMod>.Logger.Error(e);
            HandleConnectResult(new LoginFailure(e.ToString()));
            _attemptingConnection = false;
        }
    }

    /// <summary>
    /// handle the connection result and do things
    /// </summary>
    /// <param name="result"></param>
    private void HandleConnectResult (LoginResult result)
    {
        string outText;
        if (result.Successful && _session is not null)
        {
            var success = (LoginSuccessful)result;

            ServerData.SetupSession(success.SlotData, _session.RoomState.Seed);
            Authenticated = true;

            DeathLinkHandler = new DeathLinkHandler(_session.CreateDeathLinkService(), ServerData.SlotName);
#if NET35
            session.Locations.CompleteLocationChecksAsync(null, ServerData.CheckedLocations.ToArray());
#else
            _session.Locations.CompleteLocationChecksAsync(ServerData.CheckedLocations.ToArray());
#endif
            outText = $"Successfully connected to {ServerData.Hostname} as {ServerData.SlotName}!";

            ArchipelagoConsole.LogMessage(outText);
        }
        else
        {
            var failure = (LoginFailure)result;
            outText = $"Failed to connect to {ServerData.Hostname} as {ServerData.SlotName}.";
            outText = failure.Errors.Aggregate(outText, (current, error) => current + $"\n    {error}");

            Melon<LwnApMod>.Logger.Error(outText);

            Authenticated = false;
            Disconnect();
        }

        ArchipelagoConsole.LogMessage(outText);
        _attemptingConnection = false;
    }

    /// <summary>
    /// something we wrong or we need to properly disconnect from the server. cleanup and re null our session
    /// </summary>
    private void Disconnect ()
    {
        Melon<LwnApMod>.Logger.Msg("disconnecting from server...");
#if NET35
        session?.Socket.Disconnect();
#else
        _session?.Socket.DisconnectAsync();
#endif
        _session = null;
        Authenticated = false;
    }

    public new void SendMessage (string message) => _session?.Socket.SendPacketAsync(new SayPacket { Text = message });

    /// <summary>
    /// we received an item so reward it here
    /// </summary>
    /// <param name="helper">item helper which we can grab our item from</param>
    private static void OnItemReceived (ReceivedItemsHelper helper)
    {
        var receivedItem = helper.DequeueItem();

        if (helper.Index < ServerData.Index) return;

        ServerData.Index++;

        // reward the item here
        // if items can be received while in an invalid state for actually handling them, they can be placed in a local
        // queue to be handled later
        GiveItem(receivedItem);
    }

    private static void GiveItem(ItemInfo item)
    {
        var itemName = ArchipelagoData.Items.Keys.ToArray()[item.ItemId];
        var itemGroup = ArchipelagoData.Items[itemName];
        
        if (Singletons.SceneManager && Singletons.SceneManager.stageId < 2)
        {
            switch (itemGroup)
            {
                case "Attack Magics":
                case "Double Jump":
                case "Counter":
                case "Bag Upgrade":
                    IncrementWitchAbility(itemName);
                    break;
                case "Boss Souls":
                    GiveBossSoul(itemName);
                    break;
                case "Filler":
                    GiveFiller(itemName);
                    break;
                default:
                    break;
            }
            
            var senderName = $"Unknown player {item.Player}";
            try
            {
                senderName = _session?.Players.GetPlayerName(item.Player);
            }
            catch (Exception)
            {
                // ignored
            }

            var senderLocationName = $"Unknown location {item.LocationId}";
            try
            {
                senderName = item.ItemDisplayName;
            }
            catch (Exception)
            {
                // ignored
            }

            Il2Cpp.Game.AppearEventPrompt($"Got {itemName} from {senderName}'s world ({senderLocationName}).");
        }
        else
        {
            //TODO queue item here
        }
    }

    private static void GiveFiller(string itemName)
    {
        switch (itemName)
        {
            case "HPCure":
                GiveGameItem(ItemSystem.ItemType.HPCure);
                break;
            case "HPCureMiddle":
                GiveGameItem(ItemSystem.ItemType.HPCureMiddle);
                break;
            case "HPCureBig":
                GiveGameItem(ItemSystem.ItemType.HPCureBig);
                break;
            case "MPCure":
                GiveGameItem(ItemSystem.ItemType.MPCure);
                break;
            case "MPCureMiddle":
                GiveGameItem(ItemSystem.ItemType.MPCureMiddle);
                break;
            case "MPCureBig":
                GiveGameItem(ItemSystem.ItemType.MPCureBig);
                break;
            case "Defense":
                GiveGameItem(ItemSystem.ItemType.Defense);
                break;
            case "DefenseMiddle":
                GiveGameItem(ItemSystem.ItemType.DefenseM);
                break;
            case "DefenseBig":
                GiveGameItem(ItemSystem.ItemType.DefenseB);
                break;
            case "Souls":
                if (Singletons.WizardGirl != null)
                    Il2Cpp.Game.CreateSoul(SoulSystem.SoulType.Money, Singletons.WizardGirl.transform.position,
                        Singletons.RuntimeVariables.Settings.ChestSoulCount);
                break;
            case "Trial Key":
                GiveGameItem(ItemSystem.ItemType.SPMaxAdd);
                break;
        }
    }

    private static void GiveBossSoul(string itemName)
    {
        switch (itemName)
        {
            case "Specter Armor Soul":
                ServerData.KilledBosses.Add("Boss_Act01");
                break;
            case "Tania Soul":
                ServerData.KilledBosses.Add("Boss_Level02");
                break;
            case "Monica Soul":
                ServerData.KilledBosses.Add("Boss_Level03_Big");
                break;
            case "Enraged Armor Soul":
                ServerData.KilledBosses.Add("Boss_Act01_Plus");
                break;
            case "Vanessa Soul":
                ServerData.KilledBosses.Add("Boss_Level04");
                break;
            case "Vanessa V2 Soul":
                ServerData.KilledBosses.Add("Boss_Level05");
                break;
        }
    }

    private static void IncrementWitchAbility(string itemName)
    {
        switch (itemName)
        {
            case "Arcane":
                if (Singletons.GameSave != null) Singletons.GameSave.stats.secretMagicLevel += 1;
                break;
            case "Ice":
                if (Singletons.GameSave != null) Singletons.GameSave.stats.iceMagicLevel += 1;
                break;
            case "Fire":
                if (Singletons.GameSave != null) Singletons.GameSave.stats.fireMagicLevel += 1;
                break;
            case "Thunder":
                if (Singletons.GameSave != null) Singletons.GameSave.stats.thunderMagicLevel += 1;
                break;
            case "Wind":
                if (Singletons.GameSave != null) Singletons.GameSave.stats.windMagicLevel += 1;
                break;
            case "Mana Absorption":
                if (Singletons.GameSave != null) Singletons.GameSave.stats.manaAbsorbLevel += 1;
                break;
            case "Progressive Bag Upgrade":
                var items = Singletons.WizardGirl?.g_PlayerItem;
                if (items == null) return;
                items.g_iItemSize += 1;
                Singletons.StageUi.itemBar.UpdateItemSize(items.g_iItemSize);
                Singletons.StageUi.itemBar.UpdateItemSprite(items.g_HoldItem);
                break;
        }
    }
    
    private static void GiveGameItem (ItemSystem.ItemType itemType)
    {
        var wizardGirl = Singletons.WizardGirl;
        var items = wizardGirl?.g_PlayerItem;

        if (wizardGirl == null || items == null) return;
        
        // Find first empty slot if there's any
        for (var i = 0 ; i < items.g_iItemSize ; i++)
        {
            if (items.g_HoldItem[i] != ItemSystem.ItemType.Null) continue;
            
            items.g_HoldItem[i] = itemType;
            Singletons.StageUi.itemBar.UpdateItemSprite(items.g_HoldItem);

            return;
        }

        // For trial keys replace first slot that is not a Trial Key and create souls for lost item
        if (itemType == ItemSystem.ItemType.SPMaxAdd)
        {
            for (var i = 0 ; i < items.g_iItemSize ; i++)
            {
                if (items.g_HoldItem[i] == ItemSystem.ItemType.SPMaxAdd) continue;
                
                items.g_HoldItem[i] = itemType;
                Singletons.StageUi.itemBar.UpdateItemSprite(items.g_HoldItem);
                Il2Cpp.Game.CreateSoul(SoulSystem.SoulType.Money, wizardGirl.transform.position, Singletons.RuntimeVariables.Settings.ChestSoulCount);

                return;
            }
        }

        // Create souls because item does not fit
        Il2Cpp.Game.CreateSoul(SoulSystem.SoulType.Money, wizardGirl.transform.position, Singletons.RuntimeVariables.Settings.ChestSoulCount);
    }

    /// <summary>
    /// something went wrong with our socket connection
    /// </summary>
    /// <param name="e">thrown exception from our socket</param>
    /// <param name="message">message received from the server</param>
    private static void OnSessionErrorReceived (Exception e, string message)
    {
        Melon<LwnApMod>.Logger.Error(e);
        ArchipelagoConsole.LogMessage(message);
    }

    /// <summary>
    /// something went wrong closing our connection. disconnect and clean up
    /// </summary>
    /// <param name="reason"></param>
    private void OnSessionSocketClosed (string reason)
    {
        Melon<LwnApMod>.Logger.Error($"Connection to Archipelago lost: {reason}");
        Disconnect();
    }
}
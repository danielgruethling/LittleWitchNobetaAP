using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using Il2Cpp;
using LittleWitchNobetaAP.Patches;
using LittleWitchNobetaAP.Utils;
using MelonLoader;
using UnityEngine;

namespace LittleWitchNobetaAP.Archipelago;

public class ArchipelagoClient : MonoBehaviour
{
    public const string APVersion = "0.6.3";
    private const string Game = "Little Witch Nobeta";

    public static bool IsAuthenticated { get; private set; }
    private bool _isAttemptingConnection;

    public static readonly ArchipelagoSessionData ServerData = new();
    public static DeathLinkHandler? DeathLinkHandler {get; private set;}
    public static ArchipelagoSaveFile? ApSaveFile {get; set;}
    public static ArchipelagoSession? Session { get; private set; }
    
    private static Queue<Tuple<ItemInfo, int>> PendingItems { get;} = new();

    /// <summary>
    /// call to connect to an Archipelago session. Connection info should already be set up on ServerData
    /// </summary>
    public void Connect ()
    {
        if (IsAuthenticated || _isAttemptingConnection) return;

        try
        {
            Session = ArchipelagoSessionFactory.CreateSession(ServerData.Hostname, Convert.ToInt32(ServerData.Port));
            SetupSession();
        }
        catch (Exception e)
        {
            Melon<LwnApMod>.Logger.Error(e);
        }
        
        // Before connecting, reset magic levels to 0 and get them from AP
        Melon<LwnApMod>.Logger.Msg($"Resyncing magic levels");
        if (Singletons.GameSave != null) Singletons.GameSave.stats.secretMagicLevel = 0;
        if (Singletons.GameSave != null) Singletons.GameSave.stats.iceMagicLevel = 0;
        if (Singletons.GameSave != null) Singletons.GameSave.stats.fireMagicLevel = 0;
        if (Singletons.GameSave != null) Singletons.GameSave.stats.thunderMagicLevel = 0;
        if (Singletons.GameSave != null) Singletons.GameSave.stats.windMagicLevel = 0;
        if (Singletons.GameSave != null) Singletons.GameSave.stats.manaAbsorbLevel = 0;

        TryConnect();
    }

    /// <summary>
    /// add handlers for Archipelago events
    /// </summary>
    private void SetupSession ()
    {
        if (Session == null) return;
        Session.MessageLog.OnMessageReceived += message => ArchipelagoConsole.LogMessage(message.ToString());
        Session.Items.ItemReceived += OnItemReceived;
        Session.Socket.ErrorReceived += OnSessionErrorReceived;
        Session.Socket.SocketClosed += OnSessionSocketClosed;
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
                    Session?.TryConnectAndLogin(
                        Game,
                        ServerData.SlotName,
                        ItemsHandlingFlags.AllItems,
                        new Version(APVersion),
                        password: ServerData.Password,
                        requestSlotData: true
                    ) ?? throw new ArgumentNullException()));
        }
        catch (Exception e)
        {
            Melon<LwnApMod>.Logger.Error(e);
            HandleConnectResult(new LoginFailure(e.ToString()));
            _isAttemptingConnection = false;
        }
    }

    /// <summary>
    /// handle the connection result and do things
    /// </summary>
    /// <param name="result"></param>
    private void HandleConnectResult (LoginResult result)
    {
        string outText;
        if (result.Successful && Session is not null && Singletons.GameSave is not null)
        {
            var success = (LoginSuccessful)result;

            ServerData.SetupSession(success.SlotData, Session.RoomState.Seed);
            IsAuthenticated = true;

            DeathLinkHandler = new DeathLinkHandler(Session.CreateDeathLinkService(), ServerData.SlotName);
            Session.Locations.CompleteLocationChecksAsync(ServerData.CheckedLocations.ToArray());
            outText = $"Successfully connected to {ServerData.Hostname} as {ServerData.SlotName}!";

            ArchipelagoConsole.LogMessage(outText);
            MovementPatches.BlockInput = false;
            Singletons.GameSave.basic.showTeleportMenu = true;
            Dictionary<string, object> slotData = success.SlotData;
            foreach (var optionName in slotData.Keys)
            {
                Melon<LwnApMod>.Logger.Msg($"Setting option {optionName} to {slotData[optionName]}");

                switch (optionName)
                {
                    case "death_link" when Convert.ToBoolean(slotData[optionName]):
                        DeathLinkHandler.ToggleDeathLink();
                        break;
                    case "trial_keys" when Convert.ToBoolean(slotData[optionName]):
                        TrialKeysPatches.TrialKeysEnabled = true;
                        break;
                }
            }
        }
        else
        {
            var failure = (LoginFailure)result;
            outText = $"Failed to connect to {ServerData.Hostname} as {ServerData.SlotName}.";
            outText = failure.Errors.Aggregate(outText, (current, error) => current + $"\n    {error}");

            Melon<LwnApMod>.Logger.Error(outText);

            IsAuthenticated = false;
            Disconnect();
        }

        ArchipelagoConsole.LogMessage(outText);
        _isAttemptingConnection = false;
    }

    /// <summary>
    /// something went wrong, or we need to properly disconnect from the server. cleanup and re-null our session
    /// </summary>
    private static void Disconnect ()
    {
        Melon<LwnApMod>.Logger.Msg("disconnecting from server...");
#if NET35
        session?.Socket.Disconnect();
#else
        Session?.Socket.DisconnectAsync();
#endif
        Session = null;
        IsAuthenticated = false;
    }

    public new void SendMessage (string message) => Session?.Socket.SendPacketAsync(new SayPacket { Text = message });

    /// <summary>
    /// we received an item so reward it here
    /// </summary>
    /// <param name="helper">item helper which we can grab our item from</param>
    private static void OnItemReceived (ReceivedItemsHelper helper)
    {
        var receivedItem = helper.DequeueItem();
        var itemName = ArchipelagoData.Items.Keys.ToArray()[receivedItem.ItemId - 1];
        var itemGroup = ArchipelagoData.Items[itemName];
        Thread.Sleep(20);

        //Resync spell levels even when they were received before, otherwise skip
        if (helper.Index < ServerData.Index)
        {
            if (itemGroup is "Attack Magics" or "Double Jump" or "Counter")
            {
                GiveItem(receivedItem);
            }

            return;
        }

        ServerData.Index++;

        // reward the item here
        // if items can be received while in an invalid state for actually handling them, they can be placed in a local
        // queue to be handled later
        GiveItem(receivedItem);
    }

    public static void DequeueItems()
    {
        while (PendingItems.Count > 0)
        {
            var itemInfoTuple = PendingItems.Dequeue();
            var itemName = ArchipelagoData.Items.Keys.ToArray()[itemInfoTuple.Item1.ItemId - 1];
            var itemGroup = ArchipelagoData.Items[itemName];
            
            //Resync spell levels even when they were received before, otherwise skip
            if (itemInfoTuple.Item2 <= ServerData.Index)
            {
                if (itemGroup is "Attack Magics" or "Double Jump" or "Counter")
                {
                    IncrementWitchAbility(itemName);
                }
                
                continue;
            }
            GiveItem(itemInfoTuple.Item1);
        }
    }

    private static void GiveItem(ItemInfo item)
    {
        Melon<LwnApMod>.Logger.Msg($"Got item with Id {item.ItemId}");
        var itemName = ArchipelagoData.Items.Keys.ToArray()[item.ItemId - 1];
        var itemGroup = ArchipelagoData.Items[itemName];
        
        if (Singletons.SceneManager && Singletons.SceneManager.stageId >= 2)
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
                case "Trial Key":
                case "Filler":
                    GiveFiller(itemName);
                    break;
                case "Lore":
                    GiveLore(itemName);
                    break;
            }
            
            var senderName = $"Unknown player {item.Player}";
            try
            {
                senderName = Session?.Players.GetPlayerName(item.Player);
            }
            catch (Exception)
            {
                // ignored
            }

            var senderLocationName = $"Unknown location {item.LocationId}";
            try
            {
                senderLocationName = item.LocationDisplayName;
            }
            catch (Exception)
            {
                // ignored
            }

            MelonCoroutines.Start(LwnApMod.RunOnMainThread(() =>
            {
                try
                {
                    var message = $"Got {itemName} from {senderName}'s world ({senderLocationName}).";
                    ArchipelagoConsole.LogMessage(message);
                    Il2Cpp.Game.AppearEventPrompt(message);
                }
                catch (Exception)
                {
                    //ignored
                }
            }));
        }
        else
        {
            // queue item here
            PendingItems.Enqueue(new Tuple<ItemInfo, int>(item, ServerData.Index));
        }
    }

    private static void GiveLore(string itemName)
    {
        var loreItem =
            (from item in ArchipelagoData.Items
                where item.Key == itemName
                select item.Key).FirstOrDefault();
        if (loreItem is not null)
        {
            /*if (Singletons.GameSave is not null)
            {
                Singletons.GameSave.props.propCollection[int.Parse(new string(loreItem.TakeWhile(char.IsDigit).ToArray())) - 1] = true;
            }
            else
            {
                Melon<LwnApMod>.Logger.Error($"Game save is null");
            }*/
        }
        else
        {
            Melon<LwnApMod>.Logger.Error($"Did not find lore item: {itemName}");
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
                    MelonCoroutines.Start(LwnApMod.RunOnMainThread(() => 
                        Il2Cpp.Game.CreateSoul(SoulSystem.SoulType.Money,
                        Singletons.WizardGirl.transform.position, 400)));
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
                MelonCoroutines.Start(LwnApMod.RunOnMainThread(() =>
                {
                    var items = Singletons.WizardGirl?.g_PlayerItem;
                    if (items == null) return;
                    items.g_iItemSize += 1;
                    Singletons.StageUi.itemBar.UpdateItemSize(items.g_iItemSize);
                    Singletons.StageUi.itemBar.UpdateItemSprite(items.g_HoldItem);
                }));
                
                break;
        }
    }
    
    private static void GiveGameItem (ItemSystem.ItemType itemType)
    {
        MelonCoroutines.Start(LwnApMod.RunOnMainThread(() =>
        {
            var wizardGirl = Singletons.WizardGirl;
            var items = wizardGirl?.g_PlayerItem;

            if (wizardGirl == null || items == null) return;
            
            Melon<LwnApMod>.Logger.Msg($"Giving item {itemType}");

            // Find first empty slot if there's any
            for (var i = 0; i < items.g_iItemSize; i++)
            {
                if (items.g_HoldItem[i] != ItemSystem.ItemType.Null) continue;

                items.g_HoldItem[i] = itemType;
                Singletons.StageUi.itemBar.UpdateItemSprite(items.g_HoldItem);

                return;
            }

            // For trial keys replace first slot that is not a Trial Key and create souls for lost item
            if (itemType == ItemSystem.ItemType.SPMaxAdd)
            {
                for (var i = 0; i < items.g_iItemSize; i++)
                {
                    if (items.g_HoldItem[i] == ItemSystem.ItemType.SPMaxAdd) continue;

                    Melon<LwnApMod>.Logger.Msg($"Adding trial key to item slot {i}");
                    items.g_HoldItem[i] = itemType;
                    Singletons.StageUi.itemBar.UpdateItemSprite(items.g_HoldItem);
                    Il2Cpp.Game.CreateSoul(SoulSystem.SoulType.Money, wizardGirl.transform.position, 400);

                    return;
                }
            }

            // Create souls because item does not fit
            Il2Cpp.Game.CreateSoul(SoulSystem.SoulType.Money, wizardGirl.transform.position, 400);
        }));
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
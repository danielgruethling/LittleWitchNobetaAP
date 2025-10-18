using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using LittleWitchNobetaAP.Utils;
using MelonLoader;

namespace LittleWitchNobetaAP.Archipelago;

public class DeathLinkHandler
{
    private static bool _deathLinkEnabled;
    private readonly Queue<DeathLink> _deathLinks = new();
    private readonly DeathLinkService _service;
    private readonly string _slotName;

    /// <summary>
    ///     instantiates our death link handler, sets up the hook for receiving death links, and enables death link if needed
    /// </summary>
    /// <param name="deathLinkService">
    ///     The new DeathLinkService that our handler will use to send and
    ///     receive death links
    /// </param>
    /// <param name="name">Slot name</param>
    /// <param name="enableDeathLink">Whether we should enable death link or not on startup</param>
    public DeathLinkHandler(DeathLinkService deathLinkService, string name, bool enableDeathLink = false)
    {
        _service = deathLinkService;
        _service.OnDeathLinkReceived += DeathLinkReceived;
        _slotName = name;
        _deathLinkEnabled = enableDeathLink;

        if (_deathLinkEnabled) _service.EnableDeathLink();
    }

    /// <summary>
    ///     enables/disables death link
    /// </summary>
    public void ToggleDeathLink()
    {
        _deathLinkEnabled = !_deathLinkEnabled;

        if (_deathLinkEnabled)
            _service.EnableDeathLink();
        else
            _service.DisableDeathLink();
    }

    /// <summary>
    ///     what happens when we receive a deathLink
    /// </summary>
    /// <param name="deathLink">Received Death Link object to handle</param>
    private void DeathLinkReceived(DeathLink deathLink)
    {
        _deathLinks.Enqueue(deathLink);

        Melon<LwnApMod>.Logger.Msg(string.IsNullOrWhiteSpace(deathLink.Cause)
            ? $"Received Death Link from: {deathLink.Source}"
            : $"Received Death Link from: {deathLink.Source}, cause: {deathLink.Cause}");
    }

    /// <summary>
    ///     can be called when in a valid state to kill the player, dequeueing and immediately killing the player with a
    ///     message if we have a death link in the queue
    /// </summary>
    public void KillPlayer()
    {
        try
        {
            if (_deathLinks.Count < 1) return;
            _deathLinks.Dequeue();

            // Kill the player
            var wizardGirl = Singletons.WizardGirl;
            wizardGirl?.SetForceSlip();
            wizardGirl?.FallDead();
        }
        catch (Exception e)
        {
            Melon<LwnApMod>.Logger.Error(e);
        }
    }

    /// <summary>
    ///     called to send a death link to the multiworld
    /// </summary>
    public void SendDeathLink()
    {
        try
        {
            if (!_deathLinkEnabled) return;

            Melon<LwnApMod>.Logger.Msg("sharing your death...");

            // add the cause here
            var linkToSend = new DeathLink(_slotName);

            _service.SendDeathLink(linkToSend);
        }
        catch (Exception e)
        {
            Melon<LwnApMod>.Logger.Error(e);
        }
    }
}
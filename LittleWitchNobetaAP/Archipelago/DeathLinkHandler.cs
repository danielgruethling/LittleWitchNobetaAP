using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Il2Cpp;
using LittleWitchNobetaAP.Utils;
using MelonLoader;

namespace LittleWitchNobetaAP.Archipelago;

public class DeathLinkHandler
{
    private static bool deathLinkEnabled;
    private readonly string slotName;
    private readonly DeathLinkService service;
    private readonly Queue<DeathLink> deathLinks = new();

    /// <summary>
    /// instantiates our death link handler, sets up the hook for receiving death links, and enables death link if needed
    /// </summary>
    /// <param name="deathLinkService">The new DeathLinkService that our handler will use to send and
    /// receive death links</param>
    /// <param name="enableDeathLink">Whether we should enable death link or not on startup</param>
    public DeathLinkHandler (DeathLinkService deathLinkService, string name, bool enableDeathLink = false)
    {
        service = deathLinkService;
        service.OnDeathLinkReceived += DeathLinkReceived;
        slotName = name;
        deathLinkEnabled = enableDeathLink;

        if (deathLinkEnabled)
        {
            service.EnableDeathLink();
        }
    }

    /// <summary>
    /// enables/disables death link
    /// </summary>
    public void ToggleDeathLink ()
    {
        deathLinkEnabled = !deathLinkEnabled;

        if (deathLinkEnabled)
        {
            service.EnableDeathLink();
        }
        else
        {
            service.DisableDeathLink();
        }
    }

    /// <summary>
    /// what happens when we receive a deathLink
    /// </summary>
    /// <param name="deathLink">Received Death Link object to handle</param>
    private void DeathLinkReceived (DeathLink deathLink)
    {
        deathLinks.Enqueue(deathLink);

        Melon<LwnApMod>.Logger.Msg(string.IsNullOrWhiteSpace(deathLink.Cause)
            ? $"Received Death Link from: {deathLink.Source}"
            : deathLink.Cause);
    }

    /// <summary>
    /// can be called when in a valid state to kill the player, dequeueing and immediately killing the player with a
    /// message if we have a death link in the queue
    /// </summary>
    public void KillPlayer ()
    {
        try
        {
            if (deathLinks.Count < 1) return;

            var deathLink = deathLinks.Dequeue();
            var cause = string.IsNullOrWhiteSpace(deathLink.Cause) ? GetDeathLinkCause(deathLink) : deathLink.Cause;

            // Kill the player
            var wizardGirl = Singletons.WizardGirl;
            wizardGirl?.Hit(new AttackData { g_fStrength = float.MaxValue }, true);

            Melon<LwnApMod>.Logger.Msg(cause);
        }
        catch (Exception e)
        {
            Melon<LwnApMod>.Logger.Error(e);
        }
    }

    /// <summary>
    /// returns message for the player to see when a death link is received without a cause
    /// </summary>
    /// <param name="deathLink">death link object to get relevant info from</param>
    /// <returns></returns>
    private static string GetDeathLinkCause (DeathLink deathLink) => $"Received death from {deathLink.Source}";

    /// <summary>
    /// called to send a death link to the multiworld
    /// </summary>
    public void SendDeathLink ()
    {
        try
        {
            if (!deathLinkEnabled) return;

            Melon<LwnApMod>.Logger.Msg("sharing your death...");

            // add the cause here
            var linkToSend = new DeathLink(slotName);

            service.SendDeathLink(linkToSend);
        }
        catch (Exception e)
        {
            Melon<LwnApMod>.Logger.Error(e);
        }
    }
}
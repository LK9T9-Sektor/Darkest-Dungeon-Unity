using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Sektor.DarkestDungeon.Lan.Contracts.Transport;
using UnityEngine;

/// <summary>
/// Bridges Steam transport messages to the legacy multiplayer handlers.
/// RPC-style messages ("rpc.&lt;method&gt;") replay the same game-layer calls the
/// Photon RPC path used, so the raid flow stays provider agnostic; "party_config"
/// messages carry the rival party composition. Handlers are registered by type id
/// (no branching on identifiers), keeping the wire vocabulary local to this bridge.
/// </summary>
public sealed class SteamRaidBridge
{
    private const string RpcPrefix = "rpc.";
    private const char ArgSeparator = ':';

    /// <summary>Wire type id of the party composition message; shared with the session bridge.</summary>
    public const string PartyConfigType = "party_config";

    private readonly ITransport _transport;
    private readonly Dictionary<string, Action<string[]>> _handlers;

    /// <summary>Creates the bridge bound to the given transport for outbound messages.</summary>
    public SteamRaidBridge(ITransport transport)
    {
        _transport = transport;
        _handlers = new Dictionary<string, Action<string[]>>
        {
            { "ExecuteBarkMessage", HandleBarkMessage },
            { "PlayerLoaded", HandlePlayerLoaded },
            { "HeroPassButtonClicked", HandleHeroPassButtonClicked },
            { "HeroMoveButtonClicked", HandleHeroMoveButtonClicked },
            { "HeroSkillButtonClicked", HandleHeroSkillButtonClicked },
            { "HeroSkillSelected", HandleHeroSkillSelected },
            { "HeroMoveSelected", HandleHeroMoveSelected },
            { "HeroMoveDeselected", HandleHeroMoveDeselected },
        };
    }

    /// <summary>Dispatches an inbound transport message to its local handler.</summary>
    public void Dispatch(TransportMessage message)
    {
        if (message.Type == PartyConfigType)
        {
            Debug.Log("[STEAM] party_config received from " + message.SenderId);
            MultiplayerSync.OnPartyConfigReceived(message.SenderId, MultiplayerPartyData.Deserialize(message.Payload));
            return;
        }

        if (!message.Type.StartsWith(RpcPrefix, StringComparison.Ordinal))
            return;

        string method = message.Type.Substring(RpcPrefix.Length);
        ExecuteLocally(method, ParseArgs(message.Payload));
    }

    /// <summary>
    /// Sends an RPC to the remote participant and executes it locally, mirroring the
    /// PhotonTargets.All semantics of the legacy RPC calls.
    /// </summary>
    public void SendRpc(string method, params object[] args)
    {
        string[] stringArgs = new string[args.Length];
        for (int i = 0; i < args.Length; i++)
            stringArgs[i] = args[i].ToString();

        _transport.SendMessage(RpcPrefix + method, FrameArgs(stringArgs));
        ExecuteLocally(method, stringArgs);
    }

    /// <summary>Executes the given RPC against the local game layer.</summary>
    private void ExecuteLocally(string method, string[] args)
    {
        Action<string[]> handler;
        if (_handlers.TryGetValue(method, out handler))
        {
            handler(args);
            return;
        }

        Debug.LogWarning("[STEAM] Unknown RPC: " + method);
    }

    private void HandleBarkMessage(string[] args)
    {
        int team;
        if (args.Length < 2 || !int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out team))
            return;

        PhotonGameManager.BarkMessages.Add(new BarkMessage((Team)team, args[1]));
    }

    private void HandlePlayerLoaded(string[] args)
    {
        PhotonGameManager.PlayersPreparedCount++;
    }

    private void HandleHeroPassButtonClicked(string[] args)
    {
        if (PhotonGameManager.Instanse != null)
            PhotonGameManager.Instanse.HeroPassButtonClicked();
    }

    private void HandleHeroMoveButtonClicked(string[] args)
    {
        int targetId;
        if (args.Length < 1 || !int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out targetId))
            return;

        if (PhotonGameManager.Instanse != null)
            PhotonGameManager.Instanse.HeroMoveButtonClicked(targetId);
    }

    private void HandleHeroSkillButtonClicked(string[] args)
    {
        int targetId;
        if (args.Length < 1 || !int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out targetId))
            return;

        if (PhotonGameManager.Instanse != null)
            PhotonGameManager.Instanse.HeroSkillButtonClicked(targetId);
    }

    private void HandleHeroSkillSelected(string[] args)
    {
        int slotIndex;
        if (args.Length < 1 || !int.TryParse(args[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out slotIndex))
            return;

        if (PhotonGameManager.Instanse != null)
            PhotonGameManager.Instanse.HeroSkillSelected(slotIndex);
    }

    private void HandleHeroMoveSelected(string[] args)
    {
        if (PhotonGameManager.Instanse != null)
            PhotonGameManager.Instanse.HeroMoveSelected();
    }

    private void HandleHeroMoveDeselected(string[] args)
    {
        if (PhotonGameManager.Instanse != null)
            PhotonGameManager.Instanse.HeroMoveDeselected();
    }

    private static string FrameArgs(string[] args)
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < args.Length; i++)
        {
            builder.Append(args[i].Length).Append(ArgSeparator).Append(args[i]);
        }

        return builder.ToString();
    }

    private static string[] ParseArgs(string text)
    {
        List<string> args = new List<string>();
        int position = 0;
        while (position < text.Length)
        {
            int separatorIndex = text.IndexOf(ArgSeparator, position);
            if (separatorIndex < 0)
                break;

            int length;
            if (!int.TryParse(text.Substring(position, separatorIndex - position),
                NumberStyles.Integer, CultureInfo.InvariantCulture, out length))
                break;

            position = separatorIndex + 1;
            if (length < 0 || position + length > text.Length)
                break;

            args.Add(text.Substring(position, length));
            position += length;
        }

        return args.ToArray();
    }
}

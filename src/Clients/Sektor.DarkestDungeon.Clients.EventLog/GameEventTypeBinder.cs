using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Sektor.DarkestDungeon.Core.GameEvents;

namespace Sektor.DarkestDungeon.Clients.EventLog
{
    /// <summary>
    /// Resolves only the types of the v1 game event contract and their collections while reading
    /// JSONL files; any other <c>$type</c> is rejected so a recording cannot instantiate arbitrary
    /// types (spec §1).
    /// </summary>
    internal sealed class GameEventTypeBinder : DefaultSerializationBinder
    {
        private const string ListTypeNamePrefix = "System.Collections.Generic.List`1[[";
        private const string ArrayTypeNameSuffix = "[]";
        private const string StringTypeName = "System.String";

        private static readonly Dictionary<string, Type> EventTypes = new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            [typeof(ActionRejected).FullName!] = typeof(ActionRejected),
            [typeof(AttackExecuted).FullName!] = typeof(AttackExecuted),
            [typeof(AudioPlayed).FullName!] = typeof(AudioPlayed),
            [typeof(BattleEnded).FullName!] = typeof(BattleEnded),
            [typeof(HeroStatChanged).FullName!] = typeof(HeroStatChanged),
            [typeof(PartyMoved).FullName!] = typeof(PartyMoved),
            [typeof(RoomDiscovered).FullName!] = typeof(RoomDiscovered),
            [typeof(RoundEnded).FullName!] = typeof(RoundEnded),
            [typeof(RoundStarted).FullName!] = typeof(RoundStarted),
            [typeof(SceneSnapshot).FullName!] = typeof(SceneSnapshot),
            [typeof(SkillResolved).FullName!] = typeof(SkillResolved),
            [typeof(SkillUnavailable).FullName!] = typeof(SkillUnavailable),
            [typeof(StateSnapshot).FullName!] = typeof(StateSnapshot),
            [typeof(TorchChanged).FullName!] = typeof(TorchChanged),
            [typeof(TurnEnded).FullName!] = typeof(TurnEnded),
            [typeof(TurnStarted).FullName!] = typeof(TurnStarted),
            [typeof(UnitMoved).FullName!] = typeof(UnitMoved),
        };

        private static readonly Dictionary<string, Type> CollectionElementTypes = new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            [StringTypeName] = typeof(string),
            [typeof(AudioParameter).FullName!] = typeof(AudioParameter),
            [typeof(CombatantBuffState).FullName!] = typeof(CombatantBuffState),
            [typeof(CombatantState).FullName!] = typeof(CombatantState),
            [typeof(CombatantStatusState).FullName!] = typeof(CombatantStatusState),
            [typeof(SceneNode).FullName!] = typeof(SceneNode),
            [typeof(SkillTargetResult).FullName!] = typeof(SkillTargetResult),
        };

        /// <summary>Gets the shared binder instance.</summary>
        public static GameEventTypeBinder Instance { get; } = new GameEventTypeBinder();

        /// <inheritdoc/>
        public override Type BindToType(string? assemblyName, string typeName)
        {
            if (EventTypes.TryGetValue(typeName, out Type? eventType))
                return eventType;

            if (TryResolveList(typeName, out Type? listType))
                return listType!;

            if (TryResolveArray(typeName, out Type? arrayType))
                return arrayType!;

            throw new JsonSerializationException($"Type '{typeName}' is not part of the game event contract.");
        }

        private static bool TryResolveArray(string typeName, out Type? arrayType)
        {
            arrayType = null;
            if (!typeName.EndsWith(ArrayTypeNameSuffix, StringComparison.Ordinal))
                return false;

            string elementTypeName = typeName.Substring(0, typeName.Length - ArrayTypeNameSuffix.Length);
            if (!CollectionElementTypes.TryGetValue(elementTypeName, out Type? elementType))
                return false;

            arrayType = elementType.MakeArrayType();
            return true;
        }

        private static bool TryResolveList(string typeName, out Type? listType)
        {
            listType = null;
            if (!typeName.StartsWith(ListTypeNamePrefix, StringComparison.Ordinal))
                return false;

            int argumentStart = ListTypeNamePrefix.Length;
            int argumentEnd = typeName.IndexOf(',', argumentStart);
            if (argumentEnd <= argumentStart)
                return false;

            string elementTypeName = typeName.Substring(argumentStart, argumentEnd - argumentStart);
            if (!CollectionElementTypes.TryGetValue(elementTypeName, out Type? elementType))
                return false;

            listType = typeof(List<>).MakeGenericType(elementType);
            return true;
        }
    }
}

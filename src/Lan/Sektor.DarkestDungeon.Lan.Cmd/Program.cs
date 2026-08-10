namespace Sektor.DarkestDungeon.Lan.Cmd
{
    using System;
    using System.Threading;

    using Sektor.DarkestDungeon.Lan.Contracts.Results;
    using Sektor.DarkestDungeon.Lan.Contracts.Transport;
    using Sektor.DarkestDungeon.Lan.Steam;

    /// <summary>
    /// Console smoke test for the Steam transport: one process hosts a session, another joins
    /// it, and each side exchanges one message. Requires the Steam client and a valid AppID
    /// via steam_appid.txt next to the executable.
    /// </summary>
    internal static class Program
    {
        private const string PingType = "ping";
        private const string PongType = "pong";
        private const int TimeoutSeconds = 60;
        private const int FlushMilliseconds = 500;

        private static volatile bool _exitRequested;
        private static int _exitCode = 1;

        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("  host");
                Console.WriteLine("  join <sessionId>");
                return 1;
            }

            using (SteamTransport transport = new SteamTransport(new JsonTransportCodec()))
            {
                Result init = transport.Initialize();
                if (!init.IsSuccess)
                {
                    Console.WriteLine("Init failed: " + init.ErrorMessage);
                    return 1;
                }

                Console.WriteLine("Steam ready. Local player: " + transport.LocalPlayerId);
                WireEvents(transport);

                if (args[0] == "host")
                {
                    Result created = transport.CreateSession("smoke", 2);
                    if (!created.IsSuccess)
                    {
                        Console.WriteLine("CreateSession failed: " + created.ErrorMessage);
                        return 1;
                    }
                }
                else if (args[0] == "join" && args.Length > 1)
                {
                    Result joined = transport.JoinSession(args[1]);
                    if (!joined.IsSuccess)
                    {
                        Console.WriteLine("JoinSession failed: " + joined.ErrorMessage);
                        return 1;
                    }
                }
                else
                {
                    Console.WriteLine("Unknown mode: " + args[0]);
                    return 1;
                }

                return PumpUntilFinished(transport);
            }
        }

        private static int PumpUntilFinished(ITransport transport)
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(TimeoutSeconds);
            while (!_exitRequested && DateTime.UtcNow < deadline)
            {
                transport.RunCallbacks();
                Thread.Sleep(50);
            }

            if (!_exitRequested)
            {
                Console.WriteLine("Timed out waiting for a reply.");
                return 1;
            }

            return _exitCode;
        }

        private static void WireEvents(ITransport transport)
        {
            transport.SessionJoined += sessionId =>
            {
                Console.WriteLine("Session joined: " + sessionId);
                Console.WriteLine("ROOM_ID=" + sessionId);
            };

            transport.PlayerJoined += playerId =>
            {
                Console.WriteLine("Player joined: " + playerId);
                transport.SendMessage(PingType, "hello");
            };

            transport.PlayerLeft += playerId =>
            {
                Console.WriteLine("Player left: " + playerId);
                RequestExit(0);
            };

            transport.MessageReceived += message =>
            {
                Console.WriteLine("Received [" + message.Type + "] from " + message.SenderId + ": " + message.Payload);
                if (message.Type == PongType)
                {
                    RequestExit(0);
                    return;
                }

                transport.SendMessage(PongType, "world");
                RequestExit(0);
            };

            transport.Disconnected += () =>
            {
                Console.WriteLine("Disconnected.");
                RequestExit(1);
            };
        }

        private static void RequestExit(int code)
        {
            Thread.Sleep(FlushMilliseconds);
            _exitCode = code;
            _exitRequested = true;
        }
    }
}

using System;
using System.Threading;

using Sektor.DarkestDungeon.Lan.Contracts.Results;
using Sektor.DarkestDungeon.Lan.Contracts.Transport;
using Sektor.DarkestDungeon.Lan.Steam;

namespace Sektor.DarkestDungeon.Lan.Cmd
{
    /// <summary>
    /// Console smoke test for the Steam transport. Without arguments an interactive menu lets the
    /// user pick between hosting a session or joining one (via a pasted ROOM_ID or a Steam invite,
    /// like the legacy SteamLan console app). With arguments it stays scriptable:
    /// <c>host</c> creates a session, <c>join &lt;sessionId&gt;</c> joins one, and
    /// <c>+connect_lobby &lt;sessionId&gt;</c> joins a lobby from a Steam invite URL.
    /// </summary>
    internal static class Program
    {
        private const string PingType = "ping";
        private const string PongType = "pong";
        private const string ConnectLobbyArg = "+connect_lobby";
        private const int TimeoutSeconds = 60;
        private const int FlushMilliseconds = 500;

        private static volatile bool _exitRequested;
        private static int _exitCode = 1;

        private static int Main(string[] args)
        {
            Console.Title = "Darkest Dungeon LAN - Steam";

            using (SteamTransport transport = new SteamTransport(new JsonTransportCodec()))
            {
                Result init = transport.Initialize();
                if (!init.IsSuccess)
                {
                    Console.WriteLine("Init failed: " + init.ErrorMessage);
                    WaitForExit();
                    return 1;
                }

                Console.WriteLine("Steam ready. Local player: " + transport.LocalPlayerId);
                string connectLobbyId = ExtractConnectLobbyId(args);
                WireEvents(transport, exitOnMessage: args.Length > 0 || connectLobbyId != null);

                if (connectLobbyId != null)
                {
                    Console.WriteLine("Connect lobby: " + connectLobbyId + ". Подключаемся...");
                    Result joined = transport.JoinSession(connectLobbyId);
                    if (!joined.IsSuccess)
                    {
                        Console.WriteLine("JoinSession failed: " + joined.ErrorMessage);
                        WaitForExit();
                        return 1;
                    }

                    return PumpUntilFinished(transport);
                }

                if (args.Length == 0)
                {
                    return InteractiveRun(transport);
                }

                return CliRun(transport, args);
            }
        }

        private static int InteractiveRun(ITransport transport)
        {
            while (true)
            {
                ShowMenu();
                ConsoleKey key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        Result created = transport.CreateSession("smoke", 2);
                        if (!created.IsSuccess)
                        {
                            Console.WriteLine("CreateSession failed: " + created.ErrorMessage);
                            WaitForExit();
                            return 1;
                        }

                        Console.WriteLine("Ждём подключения игрока... (Esc — выход)");
                        return PumpUntilEscape(transport);

                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        Console.Write("Введите ROOM_ID хоста (Enter — ждать приглашение Steam): ");
                        string sessionId = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(sessionId))
                        {
                            Result joined = transport.JoinSession(sessionId.Trim());
                            if (!joined.IsSuccess)
                            {
                                Console.WriteLine("JoinSession failed: " + joined.ErrorMessage);
                                WaitForExit();
                                return 1;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Ожидание приглашения через Steam... (Esc — выход)");
                        }

                        return PumpUntilEscape(transport);

                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        return 0;
                }
            }
        }

        private static int CliRun(ITransport transport, string[] args)
        {
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
                Console.WriteLine("Usage:");
                Console.WriteLine("  host");
                Console.WriteLine("  join <sessionId>");
                Console.WriteLine("  +connect_lobby <sessionId>");
                return 1;
            }

            return PumpUntilFinished(transport);
        }

        private static string ExtractConnectLobbyId(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == ConnectLobbyArg && i + 1 < args.Length)
                {
                    return args[i + 1];
                }
            }

            return null;
        }

        private static void ShowMenu()
        {
            Console.WriteLine("\n==========");
            Console.WriteLine("Darkest Dungeon — Steam LAN");
            Console.WriteLine("==========");
            Console.WriteLine();
            Console.WriteLine("1. Я ХОСТ (создать сессию)");
            Console.WriteLine("2. Я КЛИЕНТ (вступить через Steam или ROOM_ID)");
            Console.WriteLine("3. ВЫХОД");
            Console.WriteLine("==========");
            Console.Write("\nВаш выбор: ");
        }

        private static int PumpUntilEscape(ITransport transport)
        {
            while (!_exitRequested)
            {
                transport.RunCallbacks();
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    _exitCode = 0;
                    break;
                }

                Thread.Sleep(10);
            }

            return _exitCode;
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

        private static void WireEvents(ITransport transport, bool exitOnMessage)
        {
            transport.SessionInviteReceived += sessionId =>
            {
                Console.WriteLine("Получено приглашение: " + sessionId + ". Подключаемся...");
                Result joined = transport.JoinSession(sessionId);
                if (!joined.IsSuccess)
                {
                    Console.WriteLine("JoinSession failed: " + joined.ErrorMessage);
                }
            };

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
                if (exitOnMessage)
                {
                    RequestExit(0);
                }
            };

            transport.MessageReceived += message =>
            {
                Console.WriteLine("Received [" + message.Type + "] from " + message.SenderId + ": " + message.Payload);
                if (!exitOnMessage)
                {
                    if (message.Type == PingType)
                    {
                        transport.SendMessage(PongType, "world");
                    }

                    return;
                }

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
                if (exitOnMessage)
                {
                    RequestExit(1);
                }
            };
        }

        private static void WaitForExit()
        {
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey(true);
        }

        private static void RequestExit(int code)
        {
            Thread.Sleep(FlushMilliseconds);
            _exitCode = code;
            _exitRequested = true;
        }
    }
}

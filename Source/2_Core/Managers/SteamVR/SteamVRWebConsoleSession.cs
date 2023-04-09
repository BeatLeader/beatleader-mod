using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BeatLeader.SteamVR {
    internal class SteamVRWebConsoleSession : IDisposable {
        #region Constants

        private const string Host = "localhost";
        private const int Port = 27062;

        #endregion

        #region Message

        private static Message ParseMessage(string rawMessage) {
            return JObject.Parse(rawMessage).ToObject<Message>();
        }

        public struct Message {
            public int nTimestamp;
            public string sLogLevel;
            public string sLogName;
            public string sMessage;
            public string sType;
        }

        #endregion

        #region State

        public State CurrentState { get; private set; } = State.Closed;
        public string FailReason { get; private set; } = string.Empty;

        private ClientWebSocket? _webSocket;

        public void Dispose() {
            _webSocket?.Dispose();
        }

        public enum State {
            Closed,
            Opened
        }

        #endregion

        #region ConnectTask

        private static readonly byte[] openMessage = Encoding.UTF8.GetBytes("console_open");

        public async Task<(State, string)> ConnectTask(CancellationToken cancellationToken) {
            try {
                _webSocket = new ClientWebSocket();
                _webSocket.Options.SetRequestHeader("Origin", $"http://{Host}:{Port}");
                _webSocket.Options.SetRequestHeader("Sec-WebSocket-Key", "/KsVqxccgJI5AeJTuPd0KQ==");
                _webSocket.Options.SetRequestHeader("Sec-WebSocket-Extensions", "permessage-deflate; client_max_window_bits");
                await _webSocket.ConnectAsync(new Uri($"ws://{Host}:{Port}"), cancellationToken);

                var buffer = new ArraySegment<byte>(openMessage);
                await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);

                CurrentState = State.Opened;
            } catch (Exception ex) {
                FailReason = ex.Message;
                CurrentState = State.Closed;
            }

            return (CurrentState, FailReason);
        }

        #endregion

        #region ListenTask

        private readonly StringBuilder _messageBuilder = new StringBuilder();
        private readonly byte[] _receiveBuffer = new byte[256];

        public async Task<(State, string)> ListenTask(Action<Message> messageHandler, CancellationToken cancellationToken) {
            if (_webSocket == null) return (CurrentState, FailReason);

            try {
                while (true) {
                    var buffer = new ArraySegment<byte>(_receiveBuffer);
                    var result = await _webSocket.ReceiveAsync(buffer, cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close) {
                        FailReason = "Closed remotely";
                        CurrentState = State.Closed;
                    }

                    var receivedText = Encoding.UTF8.GetString(_receiveBuffer, 0, result.Count);
                    _messageBuilder.Append(receivedText);

                    if (!result.EndOfMessage) continue;

                    messageHandler?.Invoke(ParseMessage(_messageBuilder.ToString()));
                    _messageBuilder.Clear();
                }
            } catch (Exception ex) {
                FailReason = ex.Message;
                CurrentState = State.Closed;
            }

            return (CurrentState, FailReason);
        }

        #endregion

        #region HTTP requests

        //Easy way to send HTTP request with custom "Referer" header

        public bool TrySendCommandRequest(string command, out string failReason) {
            var r = new StringBuilder();
            r.AppendLine($"GET /console_command.action?sCommand={command} HTTP/1.1");
            r.AppendLine($"Host: {Host}:{Port}");
            r.AppendLine($"Referer: http://{Host}:{Port}/console/index.html");
            r.AppendLine("Connection: Close");
            r.AppendLine();
            r.AppendLine();
            return TrySendRequest(r.ToString(), out failReason);
        }

        private static bool TrySendRequest(string request, out string failReason) {
            try {
                using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(Host, Port);
                socket.Send(Encoding.UTF8.GetBytes(request));
                var response = ReadResponse(socket);

                if (!response.Contains("HTTP/1.1 200 OK")) {
                    failReason = response;
                    return false;
                }

                failReason = string.Empty;
                return true;
            } catch (Exception ex) {
                failReason = ex.Message;
                return false;
            }
        }

        private static string ReadResponse(Socket socket) {
            var response = new StringBuilder();
            var buffer = new byte[256];

            while (true) {
                var i = socket.Receive(buffer);
                if (i == 0) break;
                response.Append(Encoding.UTF8.GetString(buffer, 0, i));
                break; //Only need response code, dropping after first chunk
            }

            return response.ToString();
        }

        #endregion
    }
}
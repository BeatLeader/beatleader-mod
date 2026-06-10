using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeatLeader {
    public class ReplaySocket {
        public static WebSocketClient SocketClient;
        public static string SocketHost = "wss://sockets.api.beatleader.com/stream/player/post";

        private static readonly SemaphoreSlim _connectLock = new(1, 1);

        private static async Task EnsureConnected() {
            if (SocketClient != null && SocketClient.IsAlive()) return;

            await _connectLock.WaitAsync();
            try {
                if (SocketClient != null && SocketClient.IsAlive()) return;

                SocketClient?.Dispose();
                SocketClient = null;

                bool loggedIn = await Authentication.WaitLogin();
                if (loggedIn) {
                    SocketClient = new WebSocketClient(SocketHost);
                    await SocketClient.ConnectAsync();
                }
            } catch (Exception e) {
                Plugin.Log.Error($"Socket connection error: {e}");
            } finally {
                _connectLock.Release();
            }
        }

        public static async void ConnectAfterAuth() {
            try {
                await EnsureConnected();
            } catch (Exception e) {
                Plugin.Log.Error($"Socket connect after auth error: {e}");
            }
        }

        public static async void SendStatus(string status) {
            try {
                await EnsureConnected();
                SocketClient?.QueueText($"{{status: \"{status}\"}}");
            } catch (Exception e) {
                Plugin.Log.Error($"Socket send status error: {e}");
            }
        }

        public static async Task PublishNewMessage(string message) {
            await EnsureConnected();
            SocketClient?.QueueText(message);
        }

        private static void QueueData(byte[] data) {
            SocketClient?.QueueBinary(data);
        }

        public static async void LaunchedMap(ReplayInfo info) {
            try {
                await EnsureConnected();
            } catch {
                return;
            }

            using var stream = new MemoryStream();
            var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write((byte)StructType.info);
            ReplayEncoder.EncodeInfo(info, writer);
            QueueData(stream.ToArray());
        }

        public static void SendFrame(Frame frame) {
            using var stream = new MemoryStream(128);
            var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write((byte)StructType.frames);
            writer.Write((uint)1);
            ReplayEncoder.EncodeFrame(frame, writer);
            QueueData(stream.ToArray());
        }

        public static void SendNote(NoteEvent note) {
            using var stream = new MemoryStream(128);
            var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write((byte)StructType.notes);
            writer.Write((uint)1);
            ReplayEncoder.EncodeNote(note, writer);
            QueueData(stream.ToArray());
        }

        public static void SendWall(WallEvent wall) {
            using var stream = new MemoryStream(32);
            var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write((byte)StructType.walls);
            writer.Write((uint)1);
            ReplayEncoder.EncodeWall(wall, writer);
            QueueData(stream.ToArray());
        }

        public static void SendPause(Pause pause) {
            using var stream = new MemoryStream(32);
            var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write((byte)StructType.pauses);
            writer.Write((uint)1);
            ReplayEncoder.EncodePause(pause, writer);
            QueueData(stream.ToArray());
        }

        public static void SendHeight(AutomaticHeight height) {
            using var stream = new MemoryStream(16);
            var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write((byte)StructType.heights);
            writer.Write((uint)1);
            ReplayEncoder.EncodeHeight(height, writer);
            QueueData(stream.ToArray());
        }

        public static void FinishedMap(Replay replay, PlayEndData playEndData, bool submit) {
            using var stream = new MemoryStream();
            var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write((byte)99);
            ReplayEncoder.EncodeInfo(replay.info, writer);
            writer.Write((int)playEndData.EndType);
            writer.Write(playEndData.Time);
            writer.Write(submit);
            QueueData(stream.ToArray());
        }
    }
}

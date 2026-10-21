using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Npgsql;

namespace YMInc.StoreDeploymentApp.Services
{
    public class StoreChangeEvent
    {
        public int StoreId { get; set; }
        public string Table { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public double Timestamp { get; set; }
    }

    public class PresenceChangeEvent
    {
        public string SessionId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string UserInitials { get; set; } = string.Empty;
        public string StoreNumber { get; set; } = string.Empty;
        public string ActiveSection { get; set; } = string.Empty;
        public string ActiveField { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public double Timestamp { get; set; }
    }

    public class RealtimeSyncService : IAsyncDisposable
    {
        private static RealtimeSyncService? _instance;
        public static RealtimeSyncService Instance => _instance ??= new RealtimeSyncService();

        private NpgsqlConnection? _listenConn;
        private CancellationTokenSource? _cts;
        private bool _isStarted;
        private string? _connectionString;

        public event Action<StoreChangeEvent>? StoreChanged;
        public event Action<PresenceChangeEvent>? PresenceChanged;
        public event Action<bool, string>? StatusChanged;

        public bool IsConnected => _listenConn != null && _listenConn.State == System.Data.ConnectionState.Open;

        public void Start(string unpooledConnectionString)
        {
            if (_isStarted) return;
            _isStarted = true;
            _connectionString = unpooledConnectionString;
            _cts = new CancellationTokenSource();

            _ = Task.Run(() => ListenLoopAsync(_cts.Token));
        }

        private async Task ListenLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(_connectionString))
                    {
                        await Task.Delay(3000, ct);
                        continue;
                    }

                    var builder = new NpgsqlConnectionStringBuilder(_connectionString)
                    {
                        Pooling = false, // Critical for PostgreSQL LISTEN
                        KeepAlive = 15,
                        Timeout = 15,
                        CommandTimeout = 0
                    };

                    _listenConn = new NpgsqlConnection(builder.ConnectionString);
                    await _listenConn.OpenAsync(ct);

                    _listenConn.Notification += (sender, args) =>
                    {
                        try
                        {
                            if (args.Channel == "store_realtime_events")
                            {
                                var evt = JsonSerializer.Deserialize<StoreChangeEvent>(args.Payload, new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });

                                if (evt != null)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        StoreChanged?.Invoke(evt);
                                    });
                                }
                            }
                            else if (args.Channel == "store_presence_events")
                            {
                                var pEvt = JsonSerializer.Deserialize<PresenceChangeEvent>(args.Payload, new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });

                                if (pEvt != null)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        PresenceChanged?.Invoke(pEvt);
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to parse realtime notification: {ex.Message}");
                        }
                    };

                    await using (var cmd = new NpgsqlCommand("LISTEN store_realtime_events; LISTEN store_presence_events;", _listenConn))
                    {
                        await cmd.ExecuteNonQueryAsync(ct);
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusChanged?.Invoke(true, "⚡ Live Sync Active (PostgreSQL LISTEN)");
                    });

                    // Keep waiting for notifications on this socket
                    while (!ct.IsCancellationRequested)
                    {
                        await _listenConn.WaitAsync(ct);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusChanged?.Invoke(false, $"⚠️ Live Sync Reconnecting ({ex.Message.Split('\n')[0]})");
                    });

                    if (_listenConn != null)
                    {
                        try { await _listenConn.DisposeAsync(); } catch { }
                        _listenConn = null;
                    }

                    // Reconnection backoff
                    try { await Task.Delay(4000, ct); } catch { break; }
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts?.Cancel();
            if (_listenConn != null)
            {
                try { await _listenConn.DisposeAsync(); } catch { }
                _listenConn = null;
            }
        }
    }
}

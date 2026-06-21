namespace SmartApiary.WebApi.Hubs;

using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

public class TelemetryHub : Hub
{
    public static readonly ConcurrentDictionary<string, byte> WatchedHives = new();

    public async Task JoinHiveGroup(string hiveId)
    {
        WatchedHives.TryAdd(hiveId, 0);
        await Groups.AddToGroupAsync(Context.ConnectionId, hiveId);
    }

    public async Task LeaveHiveGroup(string hiveId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, hiveId);
    }
}

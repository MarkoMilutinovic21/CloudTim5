namespace SmartApiary.WebApi.Hubs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartApiary.Application.Common.Interfaces;
using System.Collections.Concurrent;
using System.Security.Claims;

[Authorize(Roles = "Beekeeper")]
public class TelemetryHub(
    IHiveRepository hiveRepository,
    IApiaryRepository apiaryRepository) : Hub
{
    public static readonly ConcurrentDictionary<string, int> WatchedHives = new();

    private HashSet<string> JoinedHives
    {
        get
        {
            if (Context.Items.TryGetValue(nameof(JoinedHives), out object? value) &&
                value is HashSet<string> joined)
            {
                return joined;
            }

            HashSet<string> created = new(StringComparer.OrdinalIgnoreCase);
            Context.Items[nameof(JoinedHives)] = created;
            return created;
        }
    }

    public async Task JoinHiveGroup(string hiveId)
    {
        if (!Guid.TryParse(hiveId, out Guid parsedHiveId))
            throw new HubException("Neispravan identifikator košnice.");

        string? userIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out Guid beekeeperId))
            throw new HubException("Korisnik nije autentifikovan.");

        var hive = await hiveRepository.GetByIdAsync(parsedHiveId, Context.ConnectionAborted);
        var apiary = hive is null
            ? null
            : await apiaryRepository.GetByIdAsync(hive.ApiaryId, Context.ConnectionAborted);

        if (apiary is null || apiary.OwnerId != beekeeperId)
            throw new HubException("Nemate pristup ovoj košnici.");

        string normalizedHiveId = parsedHiveId.ToString();
        if (!JoinedHives.Add(normalizedHiveId))
            return;

        WatchedHives.AddOrUpdate(normalizedHiveId, 1, (_, count) => count + 1);
        await Groups.AddToGroupAsync(Context.ConnectionId, normalizedHiveId);
    }

    public async Task LeaveHiveGroup(string hiveId)
    {
        if (!Guid.TryParse(hiveId, out Guid parsedHiveId))
            return;

        string normalizedHiveId = parsedHiveId.ToString();
        if (!JoinedHives.Remove(normalizedHiveId))
            return;

        DecrementWatcher(normalizedHiveId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, normalizedHiveId);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (string hiveId in JoinedHives)
            DecrementWatcher(hiveId);

        JoinedHives.Clear();
        return base.OnDisconnectedAsync(exception);
    }

    private static void DecrementWatcher(string hiveId)
    {
        while (WatchedHives.TryGetValue(hiveId, out int count))
        {
            if (count <= 1)
            {
                if (WatchedHives.TryRemove(hiveId, out _))
                    return;
            }
            else if (WatchedHives.TryUpdate(hiveId, count - 1, count))
            {
                return;
            }
        }
    }
}

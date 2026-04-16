using System.Collections.Concurrent;

public class PresenceTracker
{
    private readonly ConcurrentDictionary<string, List<string>> _onlineUsers = new();

    public Task UserConnected(string userId, string connectionId)
    {
        _onlineUsers.AddOrUpdate(
            userId,
            _ => new List<string> { connectionId },
            (_, list) =>
            {
                lock (list)
                {
                    list.Add(connectionId);
                }
                return list;
            });

        return Task.CompletedTask;
    }

    public Task UserDisconnected(string userId, string connectionId)
    {
        if (_onlineUsers.TryGetValue(userId, out var list))
        {
            lock (list)
            {
                list.Remove(connectionId);
                if (list.Count == 0)
                {
                    _onlineUsers.TryRemove(userId, out _);
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task<string[]> GetOnlineUsers()
    {
        return Task.FromResult(_onlineUsers.Keys.OrderBy(k => k).ToArray());
    }
}
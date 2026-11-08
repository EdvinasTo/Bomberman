using Bomberman.Server.src.Interfaces;

namespace Bomberman.Server.src.Patterns.Command;

public class PlaceBombCommand : ICommand
{
    private readonly IBombService _bombService;
    private readonly string _playerId;
    private readonly ILogger _logger;

    public PlaceBombCommand(IBombService bombService, string playerId, ILogger logger)
    {
        _bombService = bombService;
        _playerId = playerId;
        _logger = logger;
    }

    // Fire and forget, like the old hub code: the bomb runs until it explodes.
    public void Execute() => _ = RunSafelyAsync();

    private async Task RunSafelyAsync()
    {
        try
        {
            await _bombService.TryPlaceBombAsync(_playerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process bomb for connection {ConnectionId}", _playerId);
        }
    }

    public void Undo() => _bombService.RemovePlayerBomb(_playerId);
}
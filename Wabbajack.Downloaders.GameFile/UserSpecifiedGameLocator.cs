using Microsoft.Extensions.Logging;
using Wabbajack.DTOs;
using Wabbajack.Paths;

namespace Wabbajack.Downloaders.GameFile;

/// <summary>
/// Implementation of IGameLocator that uses a user-specified folder instead of auto-detecting game location.
/// </summary>
public class UserSpecifiedGameLocator : IGameLocator
{
    private readonly ILogger<UserSpecifiedGameLocator> _logger;
    private AbsolutePath _userSpecifiedLocation;
    
    public UserSpecifiedGameLocator(ILogger<UserSpecifiedGameLocator> logger)
    {
        _logger = logger;
        _userSpecifiedLocation = default;
    }

    /// <summary>
    /// Sets the game location to be used for all games.
    /// </summary>
    public void SetGameLocation(AbsolutePath location)
    {
        _userSpecifiedLocation = location;
        _logger.LogInformation("User specified game location set to: {location}", location);
    }

    public AbsolutePath GameLocation(Game game)
    {
        if (_userSpecifiedLocation == default)
            throw new Exception($"Game folder not specified");
            
        return _userSpecifiedLocation;
    }

    public bool IsInstalled(Game game)
    {
        return _userSpecifiedLocation != default;
    }

    public bool TryFindLocation(Game game, out AbsolutePath path)
    {
        path = _userSpecifiedLocation;
        return _userSpecifiedLocation != default;
    }
}
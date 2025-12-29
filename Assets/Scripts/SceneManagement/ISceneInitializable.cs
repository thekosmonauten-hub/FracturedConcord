using System.Collections;

/// <summary>
/// Interface for scenes/components that need deferred initialization.
/// Allows heavy initialization to be spread across frames to prevent blocking.
/// </summary>
public interface ISceneInitializable
{
    /// <summary>
    /// Initialize the scene/component asynchronously.
    /// This is called after the scene is loaded but before it's shown to the player.
    /// </summary>
    /// <returns>Coroutine that handles initialization</returns>
    IEnumerator Initialize();
    
    /// <summary>
    /// Whether this component has completed initialization.
    /// </summary>
    bool IsInitialized { get; }
}


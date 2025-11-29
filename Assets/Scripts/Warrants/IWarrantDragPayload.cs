using UnityEngine;

/// <summary>
/// Provides data for warrant drag/drop interactions so sockets and inventory sources
/// can exchange payloads without tightly coupling their implementations.
/// </summary>
public interface IWarrantDragPayload
{
    /// <summary>
    /// The unique ID of the warrant represented by this payload.
    /// </summary>
    string WarrantId { get; }

    /// <summary>
    /// Optional icon sprite used for drag ghosts and socket previews.
    /// </summary>
    Sprite Icon { get; }

    /// <summary>
    /// Called by a socket when the payload has been accepted. Implementations can use
    /// the <paramref name="replacedWarrantId"/> argument to support swaps.
    /// </summary>
    /// <param name="target">The socket that accepted the payload.</param>
    /// <param name="replacedWarrantId">The ID previously assigned to the target.</param>
    void OnAssignmentAccepted(WarrantSocketView target, string replacedWarrantId);
}



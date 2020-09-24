using UnityEngine;
using MCUU.SuperTiled2Unity;

namespace MCUU.Controllers {
  public abstract class CharacterTileMovementController : MonoBehaviour {
    /// <summary>
    /// The dimension of the sides of the tiles in the tilemap
    /// </summary>
    protected float tileDimension;

    /// <summary>
    /// How fast the character moves.
    /// </summary>
    protected float movementSpeed;

    /// <summary>
    /// The normalized direction the character is heading.
    /// </summary>
    private Vector3 movementVector;

    /// <summary>
    /// The direction the character is moving.
    /// </summary>
    private float movementDistance;

    /// <summary>
    /// The tile coordinates that the character will start on.
    /// </summary>
    public Vector3Int startingTileCoordinates;

    /// <summary>
    /// The tile coordinates that the current is currently at.
    /// </summary>
    private Vector3Int currentTileCoordinates;

    void Start() {
      movementVector = new Vector3(0, 0, 0);
      movementDistance = 0;
      currentTileCoordinates = startingTileCoordinates;
      transform.localPosition = SuperMapUtils.GetPositionVector(startingTileCoordinates, tileDimension);
    }

    void FixedUpdate() {
      Vector3 translationVector = movementVector.normalized * movementSpeed * Time.fixedDeltaTime;
      movementDistance += translationVector.magnitude;

      if (movementDistance >= tileDimension) {
        // Player has traversed over a tile distance, so snap back to the grid
        float difference = movementDistance - tileDimension;
        if (translationVector.x != 0) {
          // Snap back horizontally
          translationVector.x += translationVector.x > 0 ? -difference : difference;
          // Update the tile the player is currently on
          currentTileCoordinates.x += translationVector.x > 0 ? 1 : -1;
        } else if (translationVector.y != 0) {
          // Snap back vertically
          translationVector.y += translationVector.y > 0 ? -difference : difference;
          // Update the tile the player is currently on
          currentTileCoordinates.y += translationVector.y > 0 ? 1 : -1;
        }
        transform.localPosition += translationVector;

        // Prepare for the next tile traversal
        movementDistance = 0;
        onTileArrival();
      } else {
        // Continue moving in current direction
        transform.localPosition += translationVector;
      }
    }

    abstract protected void onTileArrival();

    public Vector3Int getCurrentTileCoordinates() {
      return currentTileCoordinates;
    }
  }
}

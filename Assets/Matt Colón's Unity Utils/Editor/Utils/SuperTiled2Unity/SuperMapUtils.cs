using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using NesScripts.Controls.PathFind;

namespace MCUU.SuperTiled2Unity {
  public static class SuperMapUtils {
    /// <summary>
    /// Creates a boolean representation of the given tilemap's boundaries.  This
    /// boundary map is used as a parameter for other <see cref="SuperMapUtils"/>
    /// methods.
    /// </summary>
    /// <param name="tilemap">
    /// A Unity tilemap where all tiles are meant to represent boundaries and empty
    /// tiles represent open spaces.
    /// </param>
    /// <returns>
    /// A two-dimensional bool array where boundary tiles are represented as "false"
    /// and empty tiles are represented as "true".
    /// </returns>
    public static bool[,] CreateBoundaryMap(Tilemap tilemap) {
      Vector3Int size = tilemap.size;
      bool[,] boundaryMap = new bool[size.x, size.y];
      for (int x = 0; x < size.x; x++) {
        for (int y = 0; y < size.y; y++) {
          // We have to negate the y value because SuperTiled2Unity uses negative y tile
          // coordinates but the y index in the boundary map needs to be positive
          TileBase tile = tilemap.GetTile(new Vector3Int(x, -y, 0));
          boundaryMap[x, y] = tile == null;
        }
      }

      return boundaryMap;
    }

    /// <summary>
    /// Calculates the position vector based on the tile coordinate and tile width.  This
    /// is under the expectation that the tilemap's top left corner is anchored at the
    /// origin coordinates of the scene.
    /// </summary>
    /// <param name="tileCoordinate">
    /// The tile coordinate in the tilemap.
    /// </param>
    /// <param name="tileWidth">
    /// The width of the tiles in the tilemap in pixels.
    /// </param>
    /// <returns>
    /// A vector representing the position of the tile coordinate in the 2D world space.
    /// </returns>
    public static Vector3 GetPositionVector(Vector3Int tileCoordinate, float tileWidth) {
      return new Vector3(tileCoordinate.x * tileWidth, tileCoordinate.y * tileWidth, 0);
    }

    /// <summary>
    /// Finds the tile coordinate adjacent to a tile coordinate in the given direction.
    /// </summary>
    /// <param name="tileCoordinate">
    /// The tile coordinate in the tilemap.
    /// </param>
    /// <param name="directionalVector">
    /// The direction to look for the adjacent tile coordinate.
    /// </param>
    /// <returns>
    /// The adjacent tile coordinate in the given direction.
    /// </returns>
    public static Vector3Int GetAdjacentTileCoordinate(Vector3Int tileCoordinate, Vector3 directionalVector) {
      // Get the adjacent tile coordinate in the direction of the given vector
      Vector3Int axisAlignedDirectionalVector = new Vector3Int(0, 0, 0);
      if (directionalVector.x != 0) {
        axisAlignedDirectionalVector.x += directionalVector.x > 0 ? 1 : -1;
      } else if (directionalVector.y != 0) {
        axisAlignedDirectionalVector.y += directionalVector.y > 0 ? 1 : -1;
      }
      return tileCoordinate + axisAlignedDirectionalVector;
    }

    /// <summary>
    /// Finds the tile object adjacent to a tile coordinate in the given direction.
    /// </summary>
    /// <param name="tilemap">
    /// The tilemap to reference.
    /// </param>
    /// <param name="tileCoordinate">
    /// The tile coordinate in the tilemap.
    /// </param>
    /// <param name="directionalVector">
    /// The direction to look for the adjacent tile object.
    /// </param>
    /// <returns>
    /// The adjacent tile object in the given direction.
    /// </returns>
    private static TileBase GetAdjacentTileBase(Tilemap tilemap, Vector3Int tileCoordinate, Vector3 directionalVector) {
      Vector3Int adjacentTileCoordinate = GetAdjacentTileCoordinate(tileCoordinate, directionalVector);
      return tilemap.GetTile(adjacentTileCoordinate);
    }

    /// <summary>
    /// Returns true if the tile adjacent to a tile coordinate in the given direction is empty.
    /// </summary>
    /// <param name="tilemap">
    /// The tilemap to reference.
    /// </param>
    /// <param name="tileCoordinate">
    /// The tile coordinate in the tilemap.
    /// </param>
    /// <param name="directionalVector">
    /// The direction to look for the adjacent tile object.
    /// </param>
    /// <returns>
    /// True if the adjacent tile object in the given direction is empty.
    /// </returns>
    // Returns the tile from the given tilemap that is adjacent to the player in a given direction
    public static bool IsAdjacentTileEmpty(Tilemap tilemap, Vector3Int tileCoordinate, Vector3 directionalVector) {
      return GetAdjacentTileBase(tilemap, tileCoordinate, directionalVector) == null;
    }

    /// <summary>
    /// Finds a path of tile coordinates between the given starting (exclusive) and
    /// destination (inclusive) tile coordinates.
    /// </summary>
    /// <param name="boundaryMap">
    /// A two-dimentional bool array created from <see cref="CreateBoundaryMap"/>.
    /// </param>
    /// <param name="startingTileCoordinate">
    /// The tile coordinate of where to start the pathfinding.
    /// </param>
    /// <param name="destinationTileCoordinate">
    /// The tile coordinate of where to end the pathfinding.
    /// </param>
    /// <returns>
    /// A list of tile coordinates representing the path to the destination tile coordinate.
    /// </returns>
    public static List<Vector3Int> GetTileCoordinatePath(bool[,] boundaryMap, Vector3Int startingTileCoordinate, Vector3Int destinationTileCoordinate) {
      // We have to negate the y value because SuperTiled2Unity uses negative y tile
      // coordinates but the y index in the boundary map needs to be positive
      if (boundaryMap[destinationTileCoordinate.x, -destinationTileCoordinate.y] == false) {
        return new List<Vector3Int>();
      }

      NesScripts.Controls.PathFind.Grid boundaryGrid = CreatePathFindGrid(boundaryMap);
      Point startingPoint = new Point(startingTileCoordinate.x, startingTileCoordinate.y);
      Point destinationPoint = new Point(destinationTileCoordinate.x, destinationTileCoordinate.y);
      List<Point> pathPoints = Pathfinding.FindPath(boundaryGrid, startingPoint, destinationPoint, Pathfinding.DistanceType.Manhattan);
      return ConvertToTileCoordinates(pathPoints);
    }

    /// <summary>
    /// Finds a path of tile coordinates between the given starting (exclusive) and
    /// the first boundary encountered (exclusive) in the given direction.
    /// </summary>
    /// <param name="boundaryMap">
    /// A two-dimentional bool array created from <see cref="CreateBoundaryMap"/>.
    /// </param>
    /// <param name="startingTileCoordinate">
    /// The tile coordinate of where to start the pathfinding.
    /// </param>
    /// <param name="directionalVector">
    /// The direction to look for the adjacent tile object.
    /// </param>
    /// <returns>
    /// A list of tile coordinates representing the path to the first boundary encountered.
    /// </returns>
    public static List<Vector3Int> GetTileCoordinatePathUntilBoundary(bool[,] boundaryMap, Vector3Int startingTileCoordinate, Vector3 directionalVector) {
      int offsetX = directionalVector.x > 0 ? 1 : directionalVector.x < 0 ? -1 : 0;
      int offsetY = directionalVector.y > 0 ? 1 : directionalVector.y < 0 ? -1 : 0;
      Vector3Int offset = new Vector3Int(offsetX, offsetY, 0);

      List<Vector3Int> path = new List<Vector3Int>();
      Vector3Int currentTile = startingTileCoordinate + offset;

      while (boundaryMap[currentTile.x, -currentTile.y]) {
        path.Add(new Vector3Int(currentTile.x, currentTile.y, 0));
        currentTile += offset;
      }

      return path;
    }

    /// <summary>
    /// Finds the adjacent tile coordinate to the given tile coordinate that is the furthest
    /// from the given reference tile coordinate.
    /// </summary>
    /// <param name="boundaryMap">
    /// A two-dimentional bool array created from <see cref="CreateBoundaryMap"/>.
    /// </param>
    /// <param name="tileCoordinate">
    /// The tile coordinate by which adjacent tile coordinates will be checked.
    /// </param>
    /// <param name="referenceTileCoordinate">
    /// The tile coordinate from which the distance will be calculated to tile coordinates
    /// adjacent to <see cref="tileCoordinate"/>.
    /// </param>
    /// <returns>
    /// The adjacent tile coordinate that is the furthest from the reference tile coordinate
    /// </returns>
    public static Vector3Int? GetFurthestAdjacentTileCoordinate(bool[,] boundaryMap, Vector3Int tileCoordinate, Vector3Int referenceTileCoordinate) {
      Vector3Int? furthestAdjacentTileCoordinate = null;
      List<Vector3Int> adjacentCandidateTileCoordinates = new List<Vector3Int>();
      adjacentCandidateTileCoordinates.Add(tileCoordinate + new Vector3Int(0, 1, 0)); // Up
      adjacentCandidateTileCoordinates.Add(tileCoordinate + new Vector3Int(1, 0, 0)); // Right
      adjacentCandidateTileCoordinates.Add(tileCoordinate + new Vector3Int(0, -1, 0)); // Down
      adjacentCandidateTileCoordinates.Add(tileCoordinate + new Vector3Int(-1, 0, 0)); // Left

      float furthestDistance = -1;
      float candidateDistance;

      foreach (Vector3Int candidateTileCoordinate in adjacentCandidateTileCoordinates) {
        candidateDistance = (candidateTileCoordinate - referenceTileCoordinate).magnitude;

        // We have to negate the y value because SuperTiled2Unity uses negative y tile
        // coordinates but the y index in the boundary map needs to be positive
        if (candidateDistance > furthestDistance && boundaryMap[candidateTileCoordinate.x, -candidateTileCoordinate.y]) {
          furthestAdjacentTileCoordinate = candidateTileCoordinate;
          furthestDistance = candidateDistance;
        }
      }

      return furthestAdjacentTileCoordinate;
    }

    /// <summary>
    /// Creates a grid that will be used for pathfinding.
    /// </summary>
    /// <param name="boundaryMap">
    /// A two-dimentional bool array created from <see cref="CreateBoundaryMap"/>.
    /// </param>
    /// <returns>
    /// A grid that will be used for pathfinding.
    /// </returns>
    private static NesScripts.Controls.PathFind.Grid CreatePathFindGrid(bool[,] boundaryMap) {
      return new NesScripts.Controls.PathFind.Grid(boundaryMap);
    }

    /// <summary>
    /// Converts a list of pathfinding coordinates to tile coordinates.
    /// </summary>
    /// <param name="pathPoints">
    /// A list of points created while pathfinding.
    /// </param>
    /// <returns>
    /// A list of tile coordinates.
    /// </returns>
    private static List<Vector3Int> ConvertToTileCoordinates(List<Point> pathPoints) {
      List<Vector3Int> tileCoordinates = new List<Vector3Int>();
      if (pathPoints.Count != 0) {
        for (int i = 0; i < pathPoints.Count; i++) {
          Point pathPoint = pathPoints[i];
          tileCoordinates.Add(new Vector3Int(pathPoint.x, -pathPoint.y, 0));
        }
      }

      return tileCoordinates;
    }
  }
}

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
    /// Calculates the position vector based on the tile coordinates and tile width.  This
    /// is under the expectation that the tilemap's top left corner is anchored at the
    /// origin coordinates of the scene.
    /// </summary>
    /// <param name="tileCoordinates">
    /// The tile coordinates in the tilemap.
    /// </param>
    /// <param name="tileWidth">
    /// The width of the tiles in the tilemap in pixels.
    /// </param>
    /// <returns>
    /// A vector representing the position of the tile coordinates in the 2D world space.
    /// </returns>
    public static Vector3 GetPositionVector(Vector3Int tileCoordinates, float tileWidth) {
      return new Vector3((tileCoordinates.x * tileWidth) + (tileWidth / 2), (tileCoordinates.y * tileWidth) - (tileWidth / 2), 0);
    }

    /// <summary>
    /// Finds a path of tile coordinates between the given starting (exclusive) and
    /// destination (inclusive) tile coordinates.
    /// </summary>
    /// <param name="boundaryMap">
    /// A two-dimentional bool array created from <see cref="CreateBoundaryMap"/>.
    /// </param>
    /// <param name="startingTileCoordinates">
    /// The tile coordinates of where to start the pathfinding.
    /// </param>
    /// <param name="destinationTileCoordinates">
    /// The tile coordinates of where to end the pathfinding.
    /// </param>
    /// <returns>
    /// A list of tile coordinates representing the path to the destination tile coordinates.
    /// </returns>
    public static List<Vector3Int> GetTileCoordinatePath(bool[,] boundaryMap, Vector3Int startingTileCoordinates, Vector3Int destinationTileCoordinates) {
      // We have to negate the y value because SuperTiled2Unity uses negative y tile
      // coordinates but the y index in the boundary map needs to be positive
      if (boundaryMap[destinationTileCoordinates.x, -destinationTileCoordinates.y] == false) {
        return new List<Vector3Int>();
      }

      NesScripts.Controls.PathFind.Grid boundaryGrid = CreatePathFindGrid(boundaryMap);
      Point startingPoint = new Point(startingTileCoordinates.x, startingTileCoordinates.y);
      Point destinationPoint = new Point(destinationTileCoordinates.x, destinationTileCoordinates.y);
      List<Point> pathPoints = Pathfinding.FindPath(boundaryGrid, startingPoint, destinationPoint, Pathfinding.DistanceType.Manhattan);
      return ConvertToTileCoordinates(pathPoints);
    }

    /// <summary>
    /// Finds the adjacent tile coordinates to the given tile coordinates that is the furthest
    /// from the given reference tile coordinates.
    /// </summary>
    /// <param name="boundaryMap">
    /// A two-dimentional bool array created from <see cref="CreateBoundaryMap"/>.
    /// </param>
    /// <param name="tileCoordinates">
    /// The tile coordinates by which adjacent tile coordinates will be checked.
    /// </param>
    /// <param name="referenceTileCoordinates">
    /// The tile coordinates from which the distance will be calculated to tile coordinates
    /// adjacent to <see cref="tileCoordinates"/>.
    /// </param>
    /// <returns>
    /// The adjacent tile coordinates that is the furthest from the reference tile coordinate
    /// </returns>
    public static Vector3Int? GetFurthestAdjacentTileCoordinate(bool[,] boundaryMap, Vector3Int tileCoordinates, Vector3Int referenceTileCoordinates) {
      Vector3Int? furthestAdjacentTileCoordinates = null;
      List<Vector3Int> adjacentCandidateTileCoordinates = new List<Vector3Int>();
      adjacentCandidateTileCoordinates.Add(tileCoordinates + new Vector3Int(0, 1, 0)); // Up
      adjacentCandidateTileCoordinates.Add(tileCoordinates + new Vector3Int(1, 0, 0)); // Right
      adjacentCandidateTileCoordinates.Add(tileCoordinates + new Vector3Int(0, -1, 0)); // Down
      adjacentCandidateTileCoordinates.Add(tileCoordinates + new Vector3Int(-1, 0, 0)); // Left

      float furthestDistance = -1;
      float candidateDistance;

      foreach (Vector3Int candidateTileCoordinates in adjacentCandidateTileCoordinates) {
        candidateDistance = (candidateTileCoordinates - referenceTileCoordinates).magnitude;

        // We have to negate the y value because SuperTiled2Unity uses negative y tile
        // coordinates but the y index in the boundary map needs to be positive
        if (candidateDistance > furthestDistance && boundaryMap[candidateTileCoordinates.x, -candidateTileCoordinates.y]) {
          furthestAdjacentTileCoordinates = candidateTileCoordinates;
          furthestDistance = candidateDistance;
        }
      }

      return furthestAdjacentTileCoordinates;
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

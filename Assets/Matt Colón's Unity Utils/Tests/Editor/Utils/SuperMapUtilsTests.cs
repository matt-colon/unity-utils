using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using MCUU.SuperTiled2Unity;

namespace Tests {
  public class SuperMapUtilsTests {
    [Test]
    public void TestGetPositionVector() {
      float tileWidth = 0.08f;
      Vector3Int tileCoordinate = new Vector3Int(0, 0, 0);
      Vector3 positionVector = SuperMapUtils.GetPositionVector(tileCoordinate, tileWidth);
      Assert.AreEqual(new Vector3(0, 0, 0), positionVector);

      tileCoordinate = new Vector3Int(4, 4, 0);
      positionVector = SuperMapUtils.GetPositionVector(tileCoordinate, tileWidth);
      Assert.AreEqual(new Vector3(0.32f, 0.32f, 0), positionVector);

      tileWidth = 0.16f;
      tileCoordinate = new Vector3Int(4, 4, 0);
      positionVector = SuperMapUtils.GetPositionVector(tileCoordinate, tileWidth);
      Assert.AreEqual(new Vector3(0.64f, 0.64f, 0), positionVector);

      tileWidth = 0.32f;
      tileCoordinate = new Vector3Int(4, 4, 0);
      positionVector = SuperMapUtils.GetPositionVector(tileCoordinate, tileWidth);
      Assert.AreEqual(new Vector3(1.28f, 1.28f, 0), positionVector);
    }

    [Test]
    public void TestGetAdjacentTileCoordinate() {
      // Adjacent tile coordinate above
      Vector3Int tileCoordinate = new Vector3Int(0, 0, 0);
      Vector3 directionalVector = new Vector3(0, 1, 0);
      Vector3Int adjacentTileCoordinate = SuperMapUtils.GetAdjacentTileCoordinate(tileCoordinate, directionalVector);
      Assert.AreEqual(new Vector3Int(0, 1, 0), adjacentTileCoordinate);

      // Adjacent tile coordinate below
      directionalVector = new Vector3(0, -1, 0);
      adjacentTileCoordinate = SuperMapUtils.GetAdjacentTileCoordinate(tileCoordinate, directionalVector);
      Assert.AreEqual(new Vector3Int(0, -1, 0), adjacentTileCoordinate);

      // Adjacent tile coordinate to the left
      directionalVector = new Vector3(-1, 0, 0);
      adjacentTileCoordinate = SuperMapUtils.GetAdjacentTileCoordinate(tileCoordinate, directionalVector);
      Assert.AreEqual(new Vector3Int(-1, 0, 0), adjacentTileCoordinate);

      // Adjacent tile coordinate to the right
      directionalVector = new Vector3(1, 0, 0);
      adjacentTileCoordinate = SuperMapUtils.GetAdjacentTileCoordinate(tileCoordinate, directionalVector);
      Assert.AreEqual(new Vector3Int(1, 0, 0), adjacentTileCoordinate);

      // Adjacent tile coordinate with non-axis-aligned directional vector
      directionalVector = new Vector3(1, 0.25f, 0);
      adjacentTileCoordinate = SuperMapUtils.GetAdjacentTileCoordinate(tileCoordinate, directionalVector);
      Assert.AreEqual(new Vector3Int(1, 0, 0), adjacentTileCoordinate);

      // Adjacent tile coordinate with bias towards axis-aligning to the x axis
      directionalVector = new Vector3(1, 1, 0);
      adjacentTileCoordinate = SuperMapUtils.GetAdjacentTileCoordinate(tileCoordinate, directionalVector);
      Assert.AreEqual(new Vector3Int(1, 0, 0), adjacentTileCoordinate);
      directionalVector = new Vector3(-1, -1, 0);
      adjacentTileCoordinate = SuperMapUtils.GetAdjacentTileCoordinate(tileCoordinate, directionalVector);
      Assert.AreEqual(new Vector3Int(-1, 0, 0), adjacentTileCoordinate);
    }

    [Test]
    public void TestGetTileCoordinatePath() {
      bool[,] boundaryMap = CreateTestBoundaryMap();

      // From middle to top-left corner
      List<Vector3Int> path = SuperMapUtils.GetTileCoordinatePath(boundaryMap, new Vector3Int(3, -3, 0), new Vector3Int(1, -1, 0));
      Assert.AreEqual(4, path.Count);
      Assert.AreEqual(new Vector3Int(1, -1, 0), path[3]);

      // From middle to top-right corner
      path = SuperMapUtils.GetTileCoordinatePath(boundaryMap, new Vector3Int(3, -3, 0), new Vector3Int(5, -1, 0));
      Assert.AreEqual(4, path.Count);
      Assert.AreEqual(new Vector3Int(5, -1, 0), path[3]);

      // From middle to bottom-right corner
      path = SuperMapUtils.GetTileCoordinatePath(boundaryMap, new Vector3Int(3, -3, 0), new Vector3Int(5, -5, 0));
      Assert.AreEqual(4, path.Count);
      Assert.AreEqual(new Vector3Int(5, -5, 0), path[3]);

      // From middle to bottom-left corner
      path = SuperMapUtils.GetTileCoordinatePath(boundaryMap, new Vector3Int(3, -3, 0), new Vector3Int(1, -5, 0));
      Assert.AreEqual(4, path.Count);
      Assert.AreEqual(new Vector3Int(1, -5, 0), path[3]);

      // From top-left corner to bottom-right corner
      path = SuperMapUtils.GetTileCoordinatePath(boundaryMap, new Vector3Int(1, -1, 0), new Vector3Int(5, -5, 0));
      Assert.AreEqual(8, path.Count);
      Assert.AreEqual(new Vector3Int(5, -5, 0), path[7]);

      // From top-right corner to bottom-left corner
      path = SuperMapUtils.GetTileCoordinatePath(boundaryMap, new Vector3Int(5, -1, 0), new Vector3Int(1, -5, 0));
      Assert.AreEqual(8, path.Count);
      Assert.AreEqual(new Vector3Int(1, -5, 0), path[7]);
    }

    [Test]
    public void TestGetTileCoordinatePathWithUnreachableDestination() {
      bool[,] boundaryMap = CreateInvalidBoundaryMap();

      // From middle to top-left corner
      List<Vector3Int> path = SuperMapUtils.GetTileCoordinatePath(boundaryMap, new Vector3Int(3, -3, 0), new Vector3Int(1, -1, 0));
      Assert.AreEqual(0, path.Count);
    }

    [Test]
    public void TestGetTileCoordinatePathWithBoundaryDestination() {
      bool[,] boundaryMap = CreateTestBoundaryMap();

      // From middle to boundary tile
      List<Vector3Int> path = SuperMapUtils.GetTileCoordinatePath(boundaryMap, new Vector3Int(3, -3, 0), new Vector3Int(2, -2, 0));
      Assert.AreEqual(0, path.Count);
    }

    [Test]
    public void TestGetTileCoordinatePathUntilBoundary() {
      bool[,] boundaryMap = CreateTestBoundaryMap();

      // Rightward
      Vector3Int startingTileCoordinate = new Vector3Int(1, -1, 0);
      Vector3 directionalVector = new Vector3(1, 0, 0);
      List<Vector3Int> path = SuperMapUtils.GetTileCoordinatePathUntilBoundary(boundaryMap, startingTileCoordinate, directionalVector);
      Assert.AreEqual(4, path.Count);
      Assert.AreEqual(new Vector3Int(2, -1, 0), path[0]);
      Assert.AreEqual(new Vector3Int(3, -1, 0), path[1]);
      Assert.AreEqual(new Vector3Int(4, -1, 0), path[2]);
      Assert.AreEqual(new Vector3Int(5, -1, 0), path[3]);

      // Downward
      startingTileCoordinate = new Vector3Int(5, -1, 0);
      directionalVector = new Vector3(0, -1, 0);
      path = SuperMapUtils.GetTileCoordinatePathUntilBoundary(boundaryMap, startingTileCoordinate, directionalVector);
      Assert.AreEqual(4, path.Count);
      Assert.AreEqual(new Vector3Int(5, -2, 0), path[0]);
      Assert.AreEqual(new Vector3Int(5, -3, 0), path[1]);
      Assert.AreEqual(new Vector3Int(5, -4, 0), path[2]);
      Assert.AreEqual(new Vector3Int(5, -5, 0), path[3]);

      // Leftward
      startingTileCoordinate = new Vector3Int(5, -5, 0);
      directionalVector = new Vector3(-1, 0, 0);
      path = SuperMapUtils.GetTileCoordinatePathUntilBoundary(boundaryMap, startingTileCoordinate, directionalVector);
      Assert.AreEqual(4, path.Count);
      Assert.AreEqual(new Vector3Int(4, -5, 0), path[0]);
      Assert.AreEqual(new Vector3Int(3, -5, 0), path[1]);
      Assert.AreEqual(new Vector3Int(2, -5, 0), path[2]);
      Assert.AreEqual(new Vector3Int(1, -5, 0), path[3]);

      // Upward
      startingTileCoordinate = new Vector3Int(1, -5, 0);
      directionalVector = new Vector3(0, 1, 0);
      path = SuperMapUtils.GetTileCoordinatePathUntilBoundary(boundaryMap, startingTileCoordinate, directionalVector);
      Assert.AreEqual(4, path.Count);
      Assert.AreEqual(new Vector3Int(1, -4, 0), path[0]);
      Assert.AreEqual(new Vector3Int(1, -3, 0), path[1]);
      Assert.AreEqual(new Vector3Int(1, -2, 0), path[2]);
      Assert.AreEqual(new Vector3Int(1, -1, 0), path[3]);
    }

    [Test]
    public void TestGetFurthestAdjacentTileCoordinate() {
      bool[,] boundaryMap = CreateTestBoundaryMap();

      // SuperMapUtils.GetFurthestAdjacentTileCoordinate() prioritizes equal distances
      // in the following order: up, right, down, left

      // At middle, referencing top-left corner, open on all sides
      Vector3Int? tileCoordinate = SuperMapUtils.GetFurthestAdjacentTileCoordinate(boundaryMap, new Vector3Int(3, -3, 0), new Vector3Int(1, -1, 0));
      Assert.AreNotEqual(null, tileCoordinate);
      Assert.AreEqual(new Vector3Int(4, -3, 0), tileCoordinate);

      // At middle, referencing top-right corner, open on all sides
      tileCoordinate = SuperMapUtils.GetFurthestAdjacentTileCoordinate(boundaryMap, new Vector3Int(3, -3, 0), new Vector3Int(5, -1, 0));
      Assert.AreNotEqual(null, tileCoordinate);
      Assert.AreEqual(new Vector3Int(3, -4, 0), tileCoordinate);

      // At middle, referencing bottom-right corner, open on all sides
      tileCoordinate = SuperMapUtils.GetFurthestAdjacentTileCoordinate(boundaryMap, new Vector3Int(3, -3, 0), new Vector3Int(5, -5, 0));
      Assert.AreNotEqual(null, tileCoordinate);
      Assert.AreEqual(new Vector3Int(3, -2, 0), tileCoordinate);

      // At middle, referencing bottom-left corner, open on all sides
      tileCoordinate = SuperMapUtils.GetFurthestAdjacentTileCoordinate(boundaryMap, new Vector3Int(3, -3, 0), new Vector3Int(1, -5, 0));
      Assert.AreNotEqual(null, tileCoordinate);
      Assert.AreEqual(new Vector3Int(3, -2, 0), tileCoordinate);

      // Above middle, referencing top-left corner, open only on top and bottom
      tileCoordinate = SuperMapUtils.GetFurthestAdjacentTileCoordinate(boundaryMap, new Vector3Int(3, -2, 0), new Vector3Int(1, -1, 0));
      Assert.AreNotEqual(null, tileCoordinate);
      Assert.AreEqual(new Vector3Int(3, -3, 0), tileCoordinate);

      // Right of middle, referencing top-right corner, open only on left and right
      tileCoordinate = SuperMapUtils.GetFurthestAdjacentTileCoordinate(boundaryMap, new Vector3Int(4, -3, 0), new Vector3Int(5, -1, 0));
      Assert.AreNotEqual(null, tileCoordinate);
      Assert.AreEqual(new Vector3Int(3, -3, 0), tileCoordinate);

      // Below middle, referencing bottom-right corner, open only on top and bottom
      tileCoordinate = SuperMapUtils.GetFurthestAdjacentTileCoordinate(boundaryMap, new Vector3Int(3, -4, 0), new Vector3Int(5, -5, 0));
      Assert.AreNotEqual(null, tileCoordinate);
      Assert.AreEqual(new Vector3Int(3, -3, 0), tileCoordinate);

      // Left of middle, referencing bottom-left corner, open only on left and right
      tileCoordinate = SuperMapUtils.GetFurthestAdjacentTileCoordinate(boundaryMap, new Vector3Int(2, -3, 0), new Vector3Int(1, -5, 0));
      Assert.AreNotEqual(null, tileCoordinate);
      Assert.AreEqual(new Vector3Int(3, -3, 0), tileCoordinate);
    }

    [Test]
    public void TestGetFurthestAdjacentTileCoordinateWhenSurroundedByBoundaries() {
      bool[,] boundaryMap = CreateInvalidBoundaryMap();

      // At middle, referencing top-left corner, closed on all sides
      Vector3Int? tileCoordinate = SuperMapUtils.GetFurthestAdjacentTileCoordinate(boundaryMap, new Vector3Int(3, -3, 0), new Vector3Int(1, -1, 0));
      Assert.AreEqual(null, tileCoordinate);
    }

    // Create the following boundary map (X represents a boundary):
    // XXXXXXX
    // X     X
    // X X X X
    // X     X
    // X X X X
    // X     X
    // XXXXXXX
    private bool[,] CreateTestBoundaryMap() {
      bool[,] boundaryMap = new bool[7, 7];

      boundaryMap[0, 0] = false;
      boundaryMap[1, 0] = false;
      boundaryMap[2, 0] = false;
      boundaryMap[3, 0] = false;
      boundaryMap[4, 0] = false;
      boundaryMap[5, 0] = false;
      boundaryMap[6, 0] = false;
      
      boundaryMap[0, 1] = false;
      boundaryMap[1, 1] = true;
      boundaryMap[2, 1] = true;
      boundaryMap[3, 1] = true;
      boundaryMap[4, 1] = true;
      boundaryMap[5, 1] = true;
      boundaryMap[6, 1] = false;

      boundaryMap[0, 2] = false;
      boundaryMap[1, 2] = true;
      boundaryMap[2, 2] = false;
      boundaryMap[3, 2] = true;
      boundaryMap[4, 2] = false;
      boundaryMap[5, 2] = true;
      boundaryMap[6, 2] = false;

      boundaryMap[0, 3] = false;
      boundaryMap[1, 3] = true;
      boundaryMap[2, 3] = true;
      boundaryMap[3, 3] = true;
      boundaryMap[4, 3] = true;
      boundaryMap[5, 3] = true;
      boundaryMap[6, 3] = false;

      boundaryMap[0, 4] = false;
      boundaryMap[1, 4] = true;
      boundaryMap[2, 4] = false;
      boundaryMap[3, 4] = true;
      boundaryMap[4, 4] = false;
      boundaryMap[5, 4] = true;
      boundaryMap[6, 4] = false;

      boundaryMap[0, 5] = false;
      boundaryMap[1, 5] = true;
      boundaryMap[2, 5] = true;
      boundaryMap[3, 5] = true;
      boundaryMap[4, 5] = true;
      boundaryMap[5, 5] = true;
      boundaryMap[6, 5] = false;

      boundaryMap[0, 6] = false;
      boundaryMap[1, 6] = false;
      boundaryMap[2, 6] = false;
      boundaryMap[3, 6] = false;
      boundaryMap[4, 6] = false;
      boundaryMap[5, 6] = false;
      boundaryMap[6, 6] = false;

      return boundaryMap;
    }

    // Create the following boundary map (X represents a boundary):
    // XXXXXXX
    // X X X X
    // X XXX X
    // X X X X
    // X XXX X
    // X X X X
    // XXXXXXX
    private bool[,] CreateInvalidBoundaryMap() {
      bool[,] boundaryMap = new bool[7, 7];

      boundaryMap[0, 0] = false;
      boundaryMap[1, 0] = false;
      boundaryMap[2, 0] = false;
      boundaryMap[3, 0] = false;
      boundaryMap[4, 0] = false;
      boundaryMap[5, 0] = false;
      boundaryMap[6, 0] = false;
      
      boundaryMap[0, 1] = false;
      boundaryMap[1, 1] = true;
      boundaryMap[2, 1] = false;
      boundaryMap[3, 1] = true;
      boundaryMap[4, 1] = false;
      boundaryMap[5, 1] = true;
      boundaryMap[6, 1] = false;

      boundaryMap[0, 2] = false;
      boundaryMap[1, 2] = true;
      boundaryMap[2, 2] = false;
      boundaryMap[3, 2] = false;
      boundaryMap[4, 2] = false;
      boundaryMap[5, 2] = true;
      boundaryMap[6, 2] = false;

      boundaryMap[0, 3] = false;
      boundaryMap[1, 3] = true;
      boundaryMap[2, 3] = false;
      boundaryMap[3, 3] = true;
      boundaryMap[4, 3] = false;
      boundaryMap[5, 3] = true;
      boundaryMap[6, 3] = false;

      boundaryMap[0, 4] = false;
      boundaryMap[1, 4] = true;
      boundaryMap[2, 4] = false;
      boundaryMap[3, 4] = false;
      boundaryMap[4, 4] = false;
      boundaryMap[5, 4] = true;
      boundaryMap[6, 4] = false;

      boundaryMap[0, 5] = false;
      boundaryMap[1, 5] = true;
      boundaryMap[2, 5] = false;
      boundaryMap[3, 5] = true;
      boundaryMap[4, 5] = false;
      boundaryMap[5, 5] = true;
      boundaryMap[6, 5] = false;

      boundaryMap[0, 6] = false;
      boundaryMap[1, 6] = false;
      boundaryMap[2, 6] = false;
      boundaryMap[3, 6] = false;
      boundaryMap[4, 6] = false;
      boundaryMap[5, 6] = false;
      boundaryMap[6, 6] = false;

      return boundaryMap;
    }
  }
}

using Godot;
using System;

public class MapView : Spatial {
	private ChunkContainer chunkContainer;

	public MapView() {

	}

	public override void _Ready() {
		var watch = System.Diagnostics.Stopwatch.StartNew();
		chunkContainer = (ChunkContainer) GetNode("ChunkContainer");

		var gameMap = new GameMap(500, 500);
		gameMap.Generate();

		chunkContainer.SetupChunks(gameMap);
		GD.PrintS($"MapView: {watch.ElapsedMilliseconds}ms");
	}
}

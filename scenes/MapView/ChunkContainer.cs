using Godot;
using System;
using System.Collections.Generic;
using System.Threading;

class ChunkTerrainMesh : ArrayMesh {
	public ChunkTerrainMesh(){
		var terrainMaterial = (ShaderMaterial) ResourceLoader.Load("res://scenes/MapView/materials/TerrainShaderMaterial.tres");
		var st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);

		var full_size = GameMap.CELL_SIZE;
		var half_size = full_size / 2f;

		var v1 = new Vector3(0, 0, 0);
		var v2 = new Vector3(full_size, 0, 0);
		var v3 = new Vector3(0, 0, full_size);
		var v4 = new Vector3(full_size, 0, full_size);
		var v1_u = new Vector2(-1, -1);
		var v2_u = new Vector2(1, -1);
		var v3_u = new Vector2(-1, 1);
		var v4_u = new Vector2(1, 1);
		var center = new Vector3(GameMap.CELL_SIZE / 2, 0, GameMap.CELL_SIZE / 2);
		var center_u = new Vector2(0, 0);
	
		for (int x = 0; x < GameMap.CHUNK_WIDTH; x++) {
			for (int y = 0; y < GameMap.CHUNK_HEIGHT; y++) {
				var pos = new Vector2(x, y); // new Vector2(GameMap.CHUNK_WIDTH * GameMap.CELL_SIZE, GameMap.CHUNK_HEIGHT * GameMap.CELL_SIZE);
				var origin = new Vector3(x * GameMap.CELL_SIZE, 0, y * GameMap.CELL_SIZE);
				var origin2 = new Vector2(x * GameMap.CELL_SIZE, y * GameMap.CELL_SIZE);
				st.AddUv2(pos);

				st.AddUv(v1_u);
				st.AddVertex(origin + v1);
				st.AddUv(v2_u);
				st.AddVertex(origin + v2);
				st.AddUv(center_u);
				st.AddVertex(origin + center);
				
				st.AddUv(v1_u);
				st.AddVertex(origin + v1);
				st.AddUv(center_u);
				st.AddVertex(origin + center);
				st.AddUv(v3_u);
				st.AddVertex(origin + v3);
				
				st.AddUv(center_u);
				st.AddVertex(origin + center);
				st.AddUv(v2_u);
				st.AddVertex(origin + v2);
				st.AddUv(v4_u);
				st.AddVertex(origin + v4);

				st.AddUv(center_u);
				st.AddVertex(origin + center);
				st.AddUv(v4_u);
				st.AddVertex(origin + v4);
				st.AddUv(v3_u);
				st.AddVertex(origin + v3);
			}
		}
		st.Commit(this);
		SurfaceSetMaterial(0, terrainMaterial);
	}
}

public class ChunkContainer : MultiMeshInstance {
	
	private GameMap gameMap;
	public ArrayMesh terrainMesh;

	public void SetupChunks(GameMap gameMap) {
		GD.PrintS("Setup chunks");
		var terrainMesh = new ChunkTerrainMesh();

		this.gameMap = gameMap;
		var watch = System.Diagnostics.Stopwatch.StartNew();

		int numChunks = (gameMap.width / GameMap.CHUNK_WIDTH) * (gameMap.height / GameMap.CHUNK_HEIGHT);
		GD.PrintS("Number of chunks:", numChunks);

		var terrainMaterial = (ShaderMaterial) terrainMesh.SurfaceGetMaterial(0);
		terrainMaterial.SetShaderParam("map_width", gameMap.width);
		terrainMaterial.SetShaderParam("map_height", gameMap.height);

		var cellHeightsImage = new Image();
		cellHeightsImage.Create(gameMap.width, gameMap.height, false, Image.Format.Rgbaf);
		cellHeightsImage.Lock();
		for (int col = 0; col < gameMap.width; col++) {
			for (int row = 0; row < gameMap.height; row++) {
				var cell = gameMap.GetCell(new MapCoord(row, col));
				cellHeightsImage.SetPixel(col, row, new Color((float) cell.Height, 0, 0, 1));
			}
		}
		cellHeightsImage.Unlock();
		var cellHeights = new ImageTexture();
		cellHeights.CreateFromImage(cellHeightsImage);
		terrainMaterial.SetShaderParam("cell_heights", cellHeights);

		var cellColorsImage = new Image();
		cellColorsImage.Create(gameMap.width, gameMap.height, false, Image.Format.Rgbaf);
		cellColorsImage.Lock();
		for (int col = 0; col < gameMap.width; col++) {
			for (int row = 0; row < gameMap.height; row++) {
				var cell = gameMap.GetCell(new MapCoord(row, col));
				cellColorsImage.SetPixel(col, row, cell.Color);
			}
		}
		cellColorsImage.Unlock();
		var cellColors = new ImageTexture();
		cellColors.CreateFromImage(cellColorsImage);
		terrainMaterial.SetShaderParam("cell_colors", cellColors);

		Multimesh = new MultiMesh();
		Multimesh.Mesh = terrainMesh;
		Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;
		Multimesh.ColorFormat = MultiMesh.ColorFormatEnum.Float;
		Multimesh.CustomDataFormat = MultiMesh.CustomDataFormatEnum.Float;
		Multimesh.InstanceCount = numChunks;
		Multimesh.VisibleInstanceCount = numChunks;


		int i = 0;
		for (int row = 0; row < gameMap.height; row += GameMap.CHUNK_HEIGHT) {
			for (int col = 0; col < gameMap.width; col += GameMap.CHUNK_WIDTH) {
				var chunkPosition = new Vector3(col * GameMap.CELL_SIZE, 0, row * GameMap.CELL_SIZE);
				Multimesh.SetInstanceCustomData(i, new Color((float) col, (float) row, 0, 0));
				Multimesh.SetInstanceTransform(i, new Transform(Basis.Identity, chunkPosition));
				i++;
			}
		}
		GD.PrintS($"Chunks generate: {watch.ElapsedMilliseconds}ms");
	}
}

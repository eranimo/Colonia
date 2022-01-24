using System;
using System.Collections.Generic;
using Godot;
using LibNoise.Primitive;

public enum Direction {
	North,
	South,
	East,
	West
}

public class MapCell {
	public readonly GameMap gameMap;
	public readonly MapCoord coord;

	public int Height;
	public Color Color;

	public MapCell(GameMap gameMap, MapCoord coord) {
		this.gameMap = gameMap;
		this.coord = coord;
	}

	public MapCell GetNeighbor(Direction dir) {
		if (dir == Direction.North) {
			return gameMap.GetCell(new MapCoord(coord.Row - 1, coord.Col));
		} else if (dir == Direction.South) {
			return gameMap.GetCell(new MapCoord(coord.Row + 1, coord.Col));
		} else if (dir == Direction.East) {
			return gameMap.GetCell(new MapCoord(coord.Row, coord.Col + 1));
		} else if (dir == Direction.West) {
			return gameMap.GetCell(new MapCoord(coord.Row, coord.Col - 1));
		}
		return null;
	}

	public override string ToString() {
		return base.ToString() + string.Format("(Coord: {0}, Height: {1}, Color: {2})", this.coord, this.Height, this.Color);
	}
}

public class MapCoord {
	public int Col;
	public int Row;

	public MapCoord(int row, int col) {
		this.Row = row;
		this.Col = col;
	}

	public override bool Equals(object obj) {
		if ((obj == null) || ! this.GetType().Equals(obj.GetType())) {
			return false;
		}
		MapCoord other = (MapCoord) obj;
		return this.Row == other.Row && this.Col == other.Col;
	}

	public override int GetHashCode() {
		return (Col, Row).GetHashCode();
	}

	public override string ToString() {
		return base.ToString() + string.Format("({0}, {1})", this.Col, this.Row);
	}

	public static MapCoord operator +(MapCoord a, MapCoord b){
		return new MapCoord(a.Col + b.Col, a.Row + b.Row);
	}

}

class WorldNoise {
	public int width;
	public int height;
	public int octaves;
	public int frequency;
	public float amplitude;
	private LibNoise.Primitive.ImprovedPerlin noise;

	public WorldNoise(int width, int height, int seed, int octaves = 5, int frequency = 2, float amplitude = 0.5f) {
		this.width = width;
		this.height = height;
		this.octaves = octaves;
		this.frequency = frequency;
		this.amplitude = amplitude;
		this.noise = new LibNoise.Primitive.SimplexPerlin(seed, LibNoise.NoiseQuality.Best);
	}

	/// <summary>Gets a coordinate noise value projected onto a sphere</summary>
	public float Get(int x, int y) {
		var coordLong = ((x / (double) this.width) * 360) - 180;
		var coordLat = ((-y / (double) this.height) * 180) + 90;
		var inc = ((coordLat + 90.0) / 180.0) * Math.PI;
		var azi = ((coordLong + 180.0) / 360.0) * (2 * Math.PI);
		var nx = (float) (Math.Sin(inc) * Math.Cos(azi));
		var ny = (float) (Math.Sin(inc) * Math.Sin(azi));
		var nz = (float) (Math.Cos(inc));

		float amplitude = 1;
		float freq = 1;
		var v = 0f;
		for (var i = 0; i < this.octaves; i++) {
			v += this.noise.GetValue(nx * freq, ny * freq, nz * freq) * amplitude;
			amplitude *= this.amplitude;
			freq *= this.frequency;
		}

		v = (v + 1) / 2;
		return v;
	}
}

public class GameMap {
	public readonly int width;
	public readonly int height;
	public Dictionary<MapCoord, MapCell> Cells = new Dictionary<MapCoord, MapCell>();

	public const float CELL_SIZE = 10.0f;
	public const int CHUNK_WIDTH = 100;
	public const int CHUNK_HEIGHT = 100;

	public GameMap(
		int width,
		int height
	) {
		this.width = width;
		this.height = height;
	}

	public void Generate() {
		var noise = new WorldNoise(width, height, 123);

		for (int row = 0; row < height; row++) {
			for (int col = 0; col < width; col++) {
				var coord = new MapCoord(col, row);
				var cell = new MapCell(this, coord);
				cell.Height = (int) Mathf.Round(noise.Get(col, row) * 100f);
				if (cell.Height < 50) {
					cell.Color = new Color("#ff1f538c");
				} else {
					cell.Color = new Color("#ff529a3b");
				}
				Cells[coord] = cell;
			}
		}
	}

	public MapCell GetCell(MapCoord pos) {
		try {
			return Cells[pos];
		} catch (KeyNotFoundException) {
			return null;
		}
	} 
}
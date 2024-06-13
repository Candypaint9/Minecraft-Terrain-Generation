using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelData
{
	//max height natural terrain can ever be formed
	public static readonly int maxNaturalTerrainHeight = 110;
	public static readonly int minNaturalTerrainHeight = 40;

	//reach to break blocks
	public static readonly int blockReach = 6;

	public static readonly int chunkWidth = 16;
    public static readonly int chunkHeight = 256;
	

	public static readonly int textureAtlasSizeInBlocks = 4;
	public static float normalizedBlockTextureSize
	{ 
		get { return 1f / textureAtlasSizeInBlocks; }
	}


	public static readonly Vector3Int[] facesCheck = new Vector3Int[]
	{
		//back, front, left, right, top, bottom
		new Vector3Int(0, 0, -1),
		new Vector3Int(0, 0, 1),
		new Vector3Int(-1, 0, 0),
		new Vector3Int(1, 0, 0),
		new Vector3Int(0, 1, 0),
		new Vector3Int(0, -1, 0)
	};


	public static readonly Vector2Int[] surroundingTreesCheck = new Vector2Int[]
	{
		new Vector2Int(1, 0),
		new Vector2Int(0, 1),
		new Vector2Int(0, -1),
		new Vector2Int(-1, 0),

		new Vector2Int(-1, -1),
		new Vector2Int(1, -1),
		new Vector2Int(-1, 1),
		new Vector2Int(1, 1),
	};


	public static readonly Vector3Int[][] voxelVerts = new Vector3Int[][]
	{
		//back
		new Vector3Int[]
		{
			new Vector3Int(0, 0, 0),
			new Vector3Int(0, 1, 0),
			new Vector3Int(1, 0, 0),
			new Vector3Int(1, 1, 0)
		},

		//front
		new Vector3Int[]
		{
			new Vector3Int(1, 0, 1),
			new Vector3Int(1, 1, 1),
			new Vector3Int(0, 0, 1),
			new Vector3Int(0, 1, 1)
		},
		


		//left
		new Vector3Int[]
		{
			new Vector3Int(0, 0, 1),
			new Vector3Int(0, 1, 1),
			new Vector3Int(0, 0, 0),
			new Vector3Int(0, 1, 0),			
		},

		//right
		new Vector3Int[]
		{
			new Vector3Int(1, 0, 0),
			new Vector3Int(1, 1, 0),
			new Vector3Int(1, 0, 1),
			new Vector3Int(1, 1, 1),
		},

		//top
		new Vector3Int[]
		{
			new Vector3Int(0, 1, 0),
			new Vector3Int(0, 1, 1),
			new Vector3Int(1, 1, 0),
			new Vector3Int(1, 1, 1),
		},

		//bottom
		new Vector3Int[]
		{
			new Vector3Int(1, 0, 0),
			new Vector3Int(1, 0, 1),
			new Vector3Int(0, 0, 0),
			new Vector3Int(0, 0, 1),
		}
	};


	public static readonly Vector2[] voxelUvs = new Vector2[]
	{
		new Vector2(0, 0),
		new Vector2(0, normalizedBlockTextureSize),
		new Vector2(normalizedBlockTextureSize, 0),
		new Vector2(normalizedBlockTextureSize, normalizedBlockTextureSize)
	};


	public static Vector2[] _posArray = new Vector2[]
	{
		new Vector2(-BiomeMapValues.biomeBlending, BiomeMapValues.biomeBlending),
		new Vector2(-BiomeMapValues.biomeBlending, -BiomeMapValues.biomeBlending),
		new Vector2(BiomeMapValues.biomeBlending, BiomeMapValues.biomeBlending),
		new Vector2(BiomeMapValues.biomeBlending, -BiomeMapValues.biomeBlending)
	};
}

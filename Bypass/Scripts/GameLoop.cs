using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameLoop : Node2D
{

	private const int MATERIAL_LAYER = 0;

	// Lists of Piece data for player and opponent
	private (TileMap, Piece)[] playerPieces = new (TileMap, Piece)[10];
	private (TileMap, Piece)[] oppPieces = new (TileMap, Piece)[10];


	// Parent Node of game pieces
	private Node2D pieceMap;

	// TileMap showing possible moves
	private TileMap overlayMap;


	private Player player, opponent;

	// Current Piece selected. null if no piece is selected.
	private Piece selectedPiece;

	private Control ui;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		pieceMap = GetNode<Node2D>("Board/PieceMap");
		overlayMap = GetNode<TileMap>("Board/OverlayMap");

		player = new Player("Yellowguy08");
		opponent = new Player("Max_L");

		ui = GetNode<Control>("UI");
		ui.GetNode<Label>("Player").Text = player.GetUsername();
		ui.GetNode<Label>("Player/elo").Text = player.GetElo().ToString();

		ui.GetNode<Label>("Opponent").Text = opponent.GetUsername();
		ui.GetNode<Label>("Opponent/elo").Text = opponent.GetElo().ToString();

		for (int i = 0; i < playerPieces.Length; i++)
		{
			playerPieces[i] = (pieceMap.GetChild<TileMap>(i), player.InitPieces(i, true));
		}
		for (int i = 0; i < playerPieces.Length; i++)
		{
			oppPieces[i] = (pieceMap.GetChild<TileMap>(i+10), opponent.InitPieces(i, false));
		}

		UpdatePieces();

	}

	public override void _Process(double delta)
	{
        
    }

    public override void _Input(InputEvent @event) // Refine Input Handling
	{
		if (@event is InputEventMouseButton eventMouseButton)
		{
			if (eventMouseButton.IsPressed())
			{
				// Get Global Mouse Position
				Vector2 gmp = GetGlobalMousePosition();

				// Convert to Shifted Tile Position
				Vector2I localMousePosition = overlayMap.LocalToMap(gmp);

				// Correct for Inaccurate Shift
				Vector2I correctedTilePosition = new(localMousePosition.Y - 1, -localMousePosition.X - 1);

				// Set selectedPiece to Reference of Piece at correctedTilePosition
				selectedPiece = PieceAt(correctedTilePosition).Item2;
				UpdatePieces();
			}
		}
    }


	public void UpdatePieces()
	{
		foreach (TileMap child in pieceMap.GetChildren())
        {
            child.ClearLayer(MATERIAL_LAYER);
        }
		overlayMap.ClearLayer(0);
		CreatePieces(playerPieces);
		CreatePieces(oppPieces);
		if (selectedPiece != null)
		{
			CreateOverlay(selectedPiece);
		}
	}

	public (TileMap, Piece) PieceAt(Vector2I location)
	{

		foreach ((TileMap, Piece) piece in playerPieces)
		{
			if (piece.Item2.GetLocalPosition().Equals(location))
			{
				return piece;
			}
		}
		
		foreach((TileMap, Piece) piece in oppPieces)
        {
            if (piece.Item2.GetLocalPosition().Equals(location))
            {
				return piece;
            }
        }

	
		return (null, null);
	}

	public void CreateOverlay(Piece selectedPiece)
	{
		List<Vector2I> moves = GetMoves(selectedPiece);

		foreach (Vector2I move in moves)
		{
			if (move != Vector2I.One * int.MinValue)
			{
				overlayMap.SetCell(0, move, 0, new(3, 2));
			}
		}
	}

	public bool CanMove(Vector2I position)
	{

		(TileMap, Piece) p = PieceAt(position);

		return (p.Item2 == null) && IsValidPos(position);
	}

	public Vector2I[] ValidateMoves(Vector2I[] moves)
	{

		Vector2I[] validMoves = new Vector2I[4];

		for (int i = 0; i < validMoves.Length; i++)
		{
			if (CanMove(moves[i]))
			{
				validMoves[i] = moves[i];
			} else
            {
				validMoves[i] = Vector2I.One * int.MinValue;
            }
		}

		return validMoves;

	}

	/**
	* @param selectedPiece: Currently selected Piece; Will Never be Null
	* @param boardSate: List of all Pieces on the board
	* @return List of Vector2I Positions of Possible Moves
	*/
	private List<Vector2I> GetMoves(Piece selectedPiece)
	{

		List<Vector2I> validSurroundingMoves = new List<Vector2I>(ValidateMoves(GetSurroundingTiles(selectedPiece.GetLocalPosition())));

		switch (selectedPiece.GetMaterial())
		{

			case Materials.Ice:
				List<Materials> ice_contacts = new List<Materials> { Materials.Sandpaper, Materials.Water };
				return SlideUntil(validSurroundingMoves, ice_contacts);
			case Materials.Wood:
				List<Materials> wood_contacts = new List<Materials> { };
				return SlideUntil(validSurroundingMoves, wood_contacts);
			case Materials.Metal:
				List<Vector2I> totalValidMoves = new List<Vector2I>(validSurroundingMoves);
				totalValidMoves.AddRange(SlideUntil(validSurroundingMoves, 1));
				return totalValidMoves;
			default:
				return validSurroundingMoves;
		}
	}

	/**
	* @param position: Center Point
	* @return List of Vector2I Positions of possible board position surrounding position
	*/
	private Vector2I[] GetSurroundingTiles(Vector2I position)
	{

		int[,] dirs =
		{
						 {0,-1},
			{-1, 0}, /* position */ {1, 0},
						 {0, 1}
		};

		Vector2I[] surrounding_position = new Vector2I[4];

		for (int i = 0; i < dirs.GetLength(0); i++)
		{
			Vector2I newPos = new(position.X + dirs[i, 0], position.Y + dirs[i, 1]);
			if (IsValidPos(newPos) || IsValidPos(newPos))
				surrounding_position[i] = new(position.X + dirs[i, 0], position.Y + dirs[i, 1]);
		}

		return surrounding_position;

	}
	
	// Returns if a Piece is contacting Materials material at Vector2I Position position.
	private bool Contacting(Vector2I position, Materials material)
    {
		List<Piece> boardState = new List<Piece>();

		foreach ((TileMap, Piece) piece in playerPieces)
		{
			boardState.Add(piece.Item2);
		}
		foreach ((TileMap, Piece) piece in oppPieces)
		{
			boardState.Add(piece.Item2);
		}

		Vector2I[] surroundingPosition = GetSurroundingTiles(position);

		foreach (Vector2I p in surroundingPosition)
		{
			if (PieceAt(p).Item2 != null && PieceAt(p).Equals(material))
			{
				return true;
			}
		}

		return false;
    }

	private bool IsValidPos(Vector2I position)
	{
		if (position.Y >= -8 && position.Y <= 8)
		{
			if (position.X >= 1 && position.X <= 3)
			{
				return true;
			} else if (position.X >= -4 && position.X <= -2)
			{
				return true;
            }
		}
		return false;
    }

	public void CreatePieces((TileMap, Piece)[] pieces)
	{
		foreach ((TileMap, Piece) piece in pieces)
		{
			piece.Item1.SetCell(MATERIAL_LAYER, new(0,0), 0, piece.Item2.GetMaterialAtlasPos());
		}
	}

	private List<Vector2I> SlideUntil(List<Vector2I> validSurroundingMoves, List<Materials> contacts)
	{
		List<Vector2I> finalValidMoves = new List<Vector2I>(validSurroundingMoves);

		for (int i = 0; i < validSurroundingMoves.Count; i++)
		{
			Vector2I[] newValidMoves = ValidateMoves(GetSurroundingTiles(validSurroundingMoves[i]));
			while (!newValidMoves[i].Equals(Vector2I.One * int.MinValue) && contacts.TrueForAll(contact => !Contacting(newValidMoves[i], contact)))
			{
				finalValidMoves.RemoveAt(i);
				finalValidMoves.Insert(i, newValidMoves[i]);
				newValidMoves = ValidateMoves(GetSurroundingTiles(newValidMoves[i]));
			}

		}

		return finalValidMoves;
	}
	
	private List<Vector2I> SlideUntil(List<Vector2I> validSurroundingMoves, int dist)
	{
		List<Vector2I> finalValidMoves = new List<Vector2I>(validSurroundingMoves);

		for (int i = 0; i < validSurroundingMoves.Count; i++)
		{
			Vector2I[] newValidMoves = ValidateMoves(GetSurroundingTiles(validSurroundingMoves[i]));
			while (!newValidMoves[i].Equals(Vector2I.One * int.MinValue) && (newValidMoves[i] - validSurroundingMoves[i]).Length() <= dist)
			{
				finalValidMoves.RemoveAt(i);
				finalValidMoves.Insert(i, newValidMoves[i]);
				newValidMoves = ValidateMoves(GetSurroundingTiles(newValidMoves[i]));
			}

		}

		return finalValidMoves;
    }












//-----------------------------------------------------------------------------------------------------------------------------------------------------------------------

	private void PrintList(Vector2I[] c)
	{
		for (int i = 0; i < c.Length; i++)
		{
			GD.Print(c[i].ToString());
		}
		GD.Print("");
	}

	private void PrintList(List<Vector2I> c)
	{
		for (int i = 0; i < c.Count; i++)
		{
			GD.Print(c[i].ToString());
		}
		GD.Print("");
	}
}

public enum Materials
{
    Ice,
    Sandpaper,
    Wood,
    Water,
    Glue,
    Metal
}

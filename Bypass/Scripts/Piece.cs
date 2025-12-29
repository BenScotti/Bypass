using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Piece
{
    private Materials material;
    private Vector2I localPosition;

    public Piece(Materials material, Vector2I localPosition)
    {
        this.material = material;
        this.localPosition = localPosition;
    }

    public Piece(int defaultNum, Materials material, bool isPlayer)
    {
        this.material = material;

        if (defaultNum < 5)
        {
            localPosition = new(isPlayer ? -1 : 0, defaultNum - 7);
        }
        else
        {
            localPosition = new(isPlayer ? 0 : -1, defaultNum - 2);
        }

    }
    
    public Materials GetMaterial() { return material; }

    public Vector2I GetLocalPosition() { return localPosition; }
    public Vector2I GetMaterialAtlasPos()
    {
        int materialIndex = (int)material;
        return new Vector2I(materialIndex % 4, materialIndex / 4);
    }

    public override string ToString()
    {
        return material + " @ " + localPosition;
    }
}
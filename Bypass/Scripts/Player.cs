using Godot;
using System;
using System.Collections.Generic;

public class Player
{
    private string username;
    private int elo = 0;
    private List<string> favoriteColors = new List<string> { "#ffffff", "#000000" };
    private List<Materials> setup = new List<Materials> {   Materials.Ice,
                                                            Materials.Sandpaper,
                                                            Materials.Wood,
                                                            Materials.Water,
                                                            Materials.Metal,

                                                                                Materials.Glue,
                                                                                Materials.Water,
                                                                                Materials.Wood,
                                                                                Materials.Sandpaper,
                                                                                Materials.Ice
                                                        };


    public Player(string username)
    {
        this.username = username;
    }

    public string GetUsername() { return username; }
    public int GetElo() { return elo; }
    public string GetFavoriteColor() { return favoriteColors[0]; }

    // Returns the first string in favoriteColors that doesn't match confilctColor
    public string GetFavoriteColor(string conflictColor)
    {
        if (favoriteColors[0].Equals(conflictColor)) { return favoriteColors[1]; }
        return GetFavoriteColor();
    }

    // Updates Player username to newUsername
    public void ChangeUsername(string newUsername)
    {
        this.username = newUsername;
    }

    // Updates Player elo by change
    public void UpdateElo(int change)
    {
        this.elo += change;
    }

    // Adds a new color to the end of favoriteColors. Throws and ArgumentException if favoriteColors already contains color.
    public void AddFavoriteColor(string color)
    {
        if (favoriteColors.Contains(color))
        {
            throw new ArgumentException("FavoriteColors Already Contains color.");
        }
        favoriteColors.Add(color);
    }

    // Removes any instance of color from favoriteColors and replaces it with #000000 or #ffffff
    // Replaces with #000000 if #ffffff is already in favoriteColors
    // Replaces with #ffffff if #ffffff is not already in favoriteColors.
    public void RemoveFavoriteColor(string color)
    {
        favoriteColors.Remove(color);
        if (favoriteColors.Contains("#ffffff")) { favoriteColors.Add("#000000"); }
        else { favoriteColors.Add("#ffffff"); }
    }

    // Returns an array of 10 pieces according to Player.setup
    public Piece InitPieces(int i, bool isPlayer)
    {
        return new Piece(i, setup[i], isPlayer);
    }
}
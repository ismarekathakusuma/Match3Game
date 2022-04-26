/*
 * Copyright (c) 2017 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour {
	private static Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);
	private static Tile previousSelected = null;

	private SpriteRenderer render;
	private bool isSelected = false;

	private Vector2[] adjacentDirections = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
	private bool matchFound = false;

	void Awake() {
		render = GetComponent<SpriteRenderer>();
    }

	private void Select() {
		isSelected = true;
		render.color = selectedColor;
		previousSelected = gameObject.GetComponent<Tile>();
		SFXManager.instance.PlaySFX(Clip.Select);
	}

	private void Deselect() {
		isSelected = false;
		render.color = Color.white;
		previousSelected = null;
	}

	void OnMouseDown()
	{
		// Make sure the game is permitting tile selections
		if (render.sprite == null || BoardManager.instance.IsShifting)
		{
			return;
		}

		if (isSelected)
		{ // Is it already selected?
			Deselect();
		}
		else
		{
			if (previousSelected == null)
			{ // Is it the first tile selected?
				Select();
			}
			else
			{
				if (GetAllAdjacentTiles().Contains(previousSelected.gameObject))
				{ // Call GetAllAdjacentTiles and check if the previousSelected game object is in the returned adjacent tiles list.
					SwapSprite(previousSelected.render); // Swap the sprite of the tile
					previousSelected.ClearAllMatches();
					previousSelected.Deselect(); // // If it wasn't the first one that was selected, deselect all tiles
					ClearAllMatches();
				}
				else
				{ // The tile isn't next to the previously selected one, deselect the previous one and select the newly selected tile instead
					previousSelected.GetComponent<Tile>().Deselect();
					Select();
				}
			}
		}
	}

	public void SwapSprite(SpriteRenderer render2)
	{ // Accept a SpriteRenderer called render2 as a parameter which will be used together with render to swap sprites.
		if (render.sprite == render2.sprite)
		{ // Check render2 against the SpriteRenderer of the current tile. If they are the same, do nothing, as swapping two identical sprites wouldn't make much sense.
			return;
		}

		Sprite tempSprite = render2.sprite; // Create a tempSprite to hold the sprite of render2
		render2.sprite = render.sprite; // Swap out the second sprite by setting it to the first
		render.sprite = tempSprite; // Swap out the first sprite by setting it to the second (which has been put into tempSprite
		SFXManager.instance.PlaySFX(Clip.Swap); // Swap out the first sprite by setting it to the second (which has been put into tempSprite
	}

	private GameObject GetAdjacent(Vector2 castDir)
	{
		RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);
		if (hit.collider != null)
		{
			return hit.collider.gameObject;
		}
		return null;
	}

	private List<GameObject> GetAllAdjacentTiles()
	{
		List<GameObject> adjacentTiles = new List<GameObject>();
		for (int i = 0; i < adjacentDirections.Length; i++)
		{
			adjacentTiles.Add(GetAdjacent(adjacentDirections[i]));
		}
		return adjacentTiles;
	}

	private List<GameObject> FindMatch(Vector2 castDir)
	{ // This method accepts a Vector2 as a parameter, which will be the direction all raycasts will be fired in
		List<GameObject> matchingTiles = new List<GameObject>(); // Create a new list of GameObjects to hold all matching tiles
		RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir); // Fire a ray from the tile towards the castDir direction
		while (hit.collider != null && hit.collider.GetComponent<SpriteRenderer>().sprite == render.sprite)
		{ // Keep firing new raycasts until either your raycast hits nothing, or the tiles sprite differs from the returned object sprite. If both conditions are met, you consider it a match and add it to your list
			matchingTiles.Add(hit.collider.gameObject);
			hit = Physics2D.Raycast(hit.collider.transform.position, castDir);
		}
		return matchingTiles; // Return the list of matching sprites
	}

	private void ClearMatch(Vector2[] paths) // Take a Vector2 array of paths; these are the paths in which the tile will raycast.
	{
		List<GameObject> matchingTiles = new List<GameObject>(); // Create a GameObject list to hold the matches.
		for (int i = 0; i < paths.Length; i++) // Iterate through the list of paths and add any matches to the matchingTiles list.
		{
			matchingTiles.AddRange(FindMatch(paths[i]));
		}
		if (matchingTiles.Count >= 2) // Continue if a match with 2 or more tiles was found. You might wonder why 2 matching tiles is enough here, that’s because the third match is your initial tile.
		{
			for (int i = 0; i < matchingTiles.Count; i++) // Iterate through all matching tiles and remove their sprites by setting it null.
			{
				matchingTiles[i].GetComponent<SpriteRenderer>().sprite = null;
			}
			matchFound = true; // Set the matchFound flag to true.
		}
	}

	public void ClearAllMatches()
	{
		if (render.sprite == null)
			return;

		ClearMatch(new Vector2[2] { Vector2.left, Vector2.right });
		ClearMatch(new Vector2[2] { Vector2.up, Vector2.down });
		if (matchFound)
		{
			render.sprite = null;
			matchFound = false;

			StopCoroutine(BoardManager.instance.FindNullTiles());
			StartCoroutine(BoardManager.instance.FindNullTiles());

			SFXManager.instance.PlaySFX(Clip.Clear);
		}
	}

}
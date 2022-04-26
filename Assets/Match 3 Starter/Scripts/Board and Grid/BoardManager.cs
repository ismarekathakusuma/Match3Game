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

public class BoardManager : MonoBehaviour {
	public static BoardManager instance;
	public List<Sprite> characters = new List<Sprite>();
	public GameObject tile;
	public int xSize, ySize;

	private GameObject[,] tiles;

	public bool IsShifting { get; set; }

	void Start () {
		instance = GetComponent<BoardManager>();

		Vector2 offset = tile.GetComponent<SpriteRenderer>().bounds.size;
        CreateBoard(offset.x, offset.y);
    }

	private void CreateBoard (float xOffset, float yOffset) {
		tiles = new GameObject[xSize, ySize];

        float startX = transform.position.x;
		float startY = transform.position.y;

		Sprite[] previousLeft = new Sprite[ySize];
		Sprite previousBelow = null;

		for (int x = 0; x < xSize; x++) {
			for (int y = 0; y < ySize; y++) {

				GameObject newTile = Instantiate(tile, new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0), tile.transform.rotation);
				tiles[x, y] = newTile;

				// Randomizing the Board
				newTile.transform.parent = transform; // Parent all the tiles to your BoardManager to keep your Hierarchy clean in the editor.

				List<Sprite> possibleCharacters = new List<Sprite>(); // Create a list of possible characters for this sprite.
				possibleCharacters.AddRange(characters); // Add all characters to the list.

				possibleCharacters.Remove(previousLeft[y]); // Remove the characters that are on the left and below the current sprite from the list of possible characters.
				possibleCharacters.Remove(previousBelow);

				Sprite newSprite = possibleCharacters[Random.Range(0, possibleCharacters.Count)]; // Randomly choose a sprite.
				newTile.GetComponent<SpriteRenderer>().sprite = newSprite; // Set the newly created tile's sprite to the randomly chosen sprite.

				previousLeft[y] = newSprite;
				previousBelow = newSprite;
			}
		}
    }

	public IEnumerator FindNullTiles()
	{
		for (int x = 0; x < xSize; x++)
		{
			for (int y = 0; y < ySize; y++)
			{
				if (tiles[x, y].GetComponent<SpriteRenderer>().sprite == null)
				{
					yield return StartCoroutine(ShiftTilesDown(x, y));
					break;
				}
			}
		}
	}

	private IEnumerator ShiftTilesDown(int x, int yStart, float shiftDelay = .03f)
	{
		IsShifting = true;
		List<SpriteRenderer> renders = new List<SpriteRenderer>();
		int nullCount = 0;

		for (int y = yStart; y < ySize; y++)
		{  // Loop through and finds how many spaces it needs to shift downwards
			SpriteRenderer render = tiles[x, y].GetComponent<SpriteRenderer>();
			if (render.sprite == null)
			{ // Store the number of spaces in an integer named nullCount
				nullCount++;
			}
			renders.Add(render);
		}

		for (int i = 0; i < nullCount; i++)
		{ // Loop again to begin the actual shifting
			yield return new WaitForSeconds(shiftDelay);// Loop again to begin the actual shifting
			for (int k = 0; k < renders.Count - 1; k++)
			{ // Loop through every SpriteRenderer in the list of renders
				renders[k].sprite = renders[k + 1].sprite;
				renders[k + 1].sprite = null; // Swap each sprite with the one above it, until the end is reached and the last sprite is set to null
			}
		}
		IsShifting = false;
	}


}

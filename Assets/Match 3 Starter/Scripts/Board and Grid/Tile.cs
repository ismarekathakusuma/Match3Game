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
				SwapSprite(previousSelected.render);
				previousSelected.Deselect(); // If it wasn't the first one that was selected, deselect all tiles
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



}
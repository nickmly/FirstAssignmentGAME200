using UnityEngine;
using System.Collections;

public class Node {

	public Vector3 worldPos;
	public int gridX, gridY;
	public bool walkable;
	
	public int gCost, hCost;
	public Node parent;
	
	
	public Node(bool newWalkable, Vector3 newWorldPos, int newGridX, int newGridY){
		walkable = newWalkable;
		worldPos = newWorldPos;
		gridX = newGridX;
		gridY = newGridY;
	}
	
	public int FCost{
		get {
			return gCost + hCost;
		}
	}
}

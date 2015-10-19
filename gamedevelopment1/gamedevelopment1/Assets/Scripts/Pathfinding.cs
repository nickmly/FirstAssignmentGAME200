using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour {

	Grid grid;
	public Transform seeker,target;
	
	
	void Awake(){
		grid = GetComponent<Grid>();
	}
	
	void Update(){
		FindPath(seeker.position,target.position);
	}
	
	void FindPath(Vector3 startPos, Vector3 targetPos){
		Node startNode = grid.NodeFromWorldPosition(startPos);
		Node targetNode = grid.NodeFromWorldPosition(targetPos);
		
		List<Node> openSet = new List<Node>(); //Nodes not yet evaulated
		HashSet<Node> closedSet = new HashSet<Node>(); //Nodes already evaluated
		
		openSet.Add (startNode);
		
		while(openSet.Count > 0){ // While we have open nodes to check
			Node currentNode = openSet[0]; //Current node is the first open node
			for(int i = 1; i < openSet.Count; i++){
				if(openSet[i].FCost < currentNode.FCost || (openSet[i].FCost == currentNode.FCost && openSet[i].hCost < currentNode.hCost)){ // If the node we are checking has 
				//a better FCost than the current node
					currentNode = openSet[i]; // Switch the current node to this one
				}
			}
			
			openSet.Remove(currentNode);
			closedSet.Add (currentNode);
			
			if(currentNode == targetNode){ // We found our path
				RetracePath(startNode,targetNode);
				return; // Exit the function
			}
			
			foreach(Node neighbour in grid.GetNeighbours(currentNode)){
				if(!neighbour.walkable || closedSet.Contains(neighbour)){ // If neighbour is not traversable or it's already been evaluated
					continue; //Skip it
				}
				
				int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode,neighbour);
				if(newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)){ // If new path to neighbour is shorter or neighbour is not open for evaluation
					neighbour.gCost = newMovementCostToNeighbour; // Set it's new gCost and hCost
					neighbour.hCost = GetDistance(neighbour,targetNode);
					neighbour.parent = currentNode;
					
					if(!openSet.Contains (neighbour)){
						openSet.Add (neighbour);
					}
				}
			}
		}
	}
	
	void RetracePath(Node startNode, Node endNode){
		List<Node> path = new List<Node>();
		Node currentNode = endNode;
		
		while(currentNode != startNode){ // Trace back until we hit startNode
			path.Add(currentNode);
			currentNode = currentNode.parent; // Go back one node to it's parent 
		}
		path.Reverse(); // Get the path going the right way by reversing it
		grid.path = path;
	}
	
	int GetDistance(Node nodeA, Node nodeB){
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
		
		if(dstX > dstY){
			return 14*dstY + 10*(dstX-dstY); // A * formula
		} else {
			return 14*dstX + 10*(dstY-dstX); // A * formula
		}
	}
}

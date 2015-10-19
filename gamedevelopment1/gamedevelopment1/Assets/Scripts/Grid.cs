using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

	Node[,] grid;
	[SerializeField] private LayerMask unwalkableLayer;
	[SerializeField] private Vector2 gridWorldSize;
	[SerializeField] private float nodeRadius;
	[SerializeField] private Transform player;
	private float nodeDiameter;
	private int gridSizeX, gridSizeY;
	public List<Node> path;

	
	void Start(){
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y/nodeDiameter);
		CreateGrid();
	}
	
	public Node NodeFromWorldPosition(Vector3 worldPos){ //TODO: make this world with a plane that isn't on 0,0
		float percentX = (worldPos.x + gridWorldSize.x/2) / gridWorldSize.x; //Get how far along the worldPos is on the node in a percentage (e.g. left is 0, middle is 0.5, right is 1)
		float percentY = (worldPos.z + gridWorldSize.y/2) / gridWorldSize.y; //Get how far along the worldPos is on the node in a percentage (e.g. left is 0, middle is 0.5, right is 1)
		
		percentX = Mathf.Clamp01(percentX); //Clamp it between 0 and 1 so we don't try to get it if it's outside node
		percentY = Mathf.Clamp01(percentY); 
		
		int x = Mathf.RoundToInt((gridSizeX-1) * percentX); // X is how far along we are -1 so we stay inside the node
		int y = Mathf.RoundToInt((gridSizeY-1) * percentY); 
		
		return grid[x,y];
		
	}
	
	public List<Node> GetNeighbours(Node node){
		List<Node> neighbours = new List<Node>();
		
		//3x3 grid checking:
		for(int x = -1; x <= 1; x++){
			for(int y = -1; y <= 1; y++){
				if(x == 0 && y == 0){ //If we are in the center of the 3x3 grid we are checking
					continue; //Skip this iteration
				}
				
				int checkX = node.gridX + x;
				int checkY = node.gridY + y;
				
				if(checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY){ // If node is inside the 3x3 grid
					neighbours.Add(grid[checkX,checkY]); //Add it
				}
			}
		}
		
		return neighbours;
	}
	
	void CreateGrid(){
		grid = new Node[gridSizeX,gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;//-Vector3.right * gridSizeX/2 goes to left, -Vector3.forward*gridSizeY/2 goes to bottom
		for(int x = 0; x < gridSizeX; x++){
			for(int y = 0; y < gridSizeY; y++){
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius); // As we go along x we create our rows down y
				bool walkable = !(Physics.CheckSphere(worldPoint,nodeRadius,unwalkableLayer)); // Check if the node is walkable
				grid[x,y] = new Node(walkable,worldPoint,x,y);
			}
		}
	}
	
	void OnDrawGizmos(){
		Gizmos.DrawWireCube(transform.position,new Vector3(gridWorldSize.x,1,gridWorldSize.y)); // Axes are flipped so we use z as the vertical axis
		if(grid != null){
			Node playerNode = NodeFromWorldPosition(player.position);
			foreach(Node n in grid){
				Gizmos.color = (n.walkable)?Color.white:Color.red; // Color is set to if walkable then white, if not then red
				if(playerNode == n){
					Gizmos.color = Color.blue;
				}
				if(path != null){
					if(path.Contains(n)){
						Gizmos.color = Color.black;
						Gizmos.DrawCube(n.worldPos,Vector3.one * (nodeDiameter-.1f));//Draw a cube with a little bit of extra space (0.1f) to see it  more clearly
					}
				}
				
			}
		}
	}
}

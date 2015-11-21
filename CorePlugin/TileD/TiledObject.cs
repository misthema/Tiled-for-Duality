/*
 * Created by SharpDevelop.
 * User: misthema
 * Date: 20.11.2015
 * Time: 11:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Duality;
using Duality.Drawing;

namespace TileD_Plugin.TileD
{
	public enum TiledShape {
		Rectangle,
		Ellipse,
		Polygon,
		Polyline
	}
	
	public enum TiledFlipped {
		None,
		Horizontally,
		Vertically,
		Diagonally
	}
	
	
	
	
	/// <summary>
	/// Description of TiledObject.
	/// </summary>
	public class TiledObject : ICmpRenderer
	{
		// Bits on the far end of the 32-bit global tile ID are used for tile flags
		readonly uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
		readonly uint FLIPPED_VERTICALLY_FLAG   = 0x40000000;
		readonly uint FLIPPED_DIAGONALLY_FLAG   = 0x20000000;
		
		public int ID {get;set;}
		public string Name {get;set;}
		public string Type {get;set;}
		public float X {get;set;}
		public float Y {get;set;}
		public float W {get;set;}
		public float H {get;set;}
		public bool Visible {get;set;}
		public TiledShape Shape {get;set;}
		public Vector2[] Points {get;set;}
		public TiledFlipped Flipped {get;set;}
		
		uint gid;
		public uint Gid {
			get {
				return gid;
			}
			set {
				if( (value & FLIPPED_HORIZONTALLY_FLAG) != 0 )
				{
					Flipped = TiledFlipped.Horizontally;
					gid = value - FLIPPED_HORIZONTALLY_FLAG;
				}
				else if( (value & FLIPPED_VERTICALLY_FLAG) != 0 )
				{
					Flipped = TiledFlipped.Vertically;
					gid = value - FLIPPED_VERTICALLY_FLAG;
				}
				else if( (value & FLIPPED_DIAGONALLY_FLAG) != 0 )
				{
					Flipped = TiledFlipped.Diagonally;
					gid = value - FLIPPED_DIAGONALLY_FLAG;
				}
				else
				{
					Flipped = TiledFlipped.None;
					gid = value;
				}
			}
		}
		
		public TiledObject()
		{
			Gid = 0;
		}
		
		public void Draw(IDrawDevice device)
		{
			if( Gid == 0 )
				return;
			var pos = new Vector3(X, Y, 0.0f);
			var scale = 1.0f;
			device.PreprocessCoords(ref pos, ref scale);
			
			if( Points.Length > 0 )
			{
				
				
				for( int i = 0; i < Points.Length; i++ )
				{
					
				}
			}
		}
		
		public bool IsVisible(IDrawDevice device)
		{
			return true;
		}
		
		public float BoundRadius {get;set;}
	}
}

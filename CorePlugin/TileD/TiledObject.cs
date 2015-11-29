/*
 * Created by SharpDevelop.
 * User: misthema
 * Date: 20.11.2015
 * Time: 11:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Xml.Linq;
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
		public float Angle {get;set;}
		public bool Visible {get;set;}
		public TiledShape Shape {get;set;}
		public List<Vector2> Points {get;set;}
		public TiledFlipped Flipped {get;set;}
		public TiledPropertySet Properties {get;set;}
		public TiledObjectGroup ObjectGroup {get;set;}
		
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
			Visible = true;
			Properties = new TiledPropertySet();
		}
		
		public void LoadProperties( XElement node )
		{
			Properties.Extend( node );
		}
		
		public void LoadPolygon( XElement node )
		{
			if( node.HasAttributes )
			{
				foreach( var attribute in node.Attributes() )
				{
					switch( attribute.Name.LocalName )
					{
						case "points":
							LoadPoints(attribute);
							break;
							
						default:
							Log.Editor.Write("Attribute '{0}' not supported in polygons.", attribute.Name);
							break;
					}
				}
			}
		}
		
		public void LoadPolyLine( XElement node )
		{
			
		}
		
		public void LoadPoints( XAttribute attribute )
		{
			var split = attribute.Value.Split(' ');
			
			Points = new List<Vector2>();
			
			foreach( var vec in split )
			{
				var xy = vec.Split(',');
				var x = float.Parse( xy[0], System.Globalization.NumberStyles.Float );
				var y = float.Parse( xy[1], System.Globalization.NumberStyles.Float );
				
				Points.Add( new Vector2( x, y ) );
			}
		}
		
		
		public void Draw(IDrawDevice device)
		{
			if( Gid == 0 )
				return;
			
			if( Points.Count > 0 )
			{
				var pos = new Vector3(X, Y, 0.0f);
				var scale = 1.0f;
				device.PreprocessCoords(ref pos, ref scale);
				
				foreach( var point in Points )
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

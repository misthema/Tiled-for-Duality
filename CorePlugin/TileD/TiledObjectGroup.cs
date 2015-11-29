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
using System.Xml;
using System.Xml.Linq;
using Duality;

using Duality.Drawing;

namespace TileD_Plugin.TileD
{	
	/// <summary>
	/// Description of TiledObjectGroup.
	/// </summary>
	public class TiledObjectGroup
	{
		public string Name {get;set;}
		public int W {get;set;}
		public int H {get;set;}
		public TiledPropertySet Properties {get;set;}
		public Dictionary<string, TiledObject> Objects {get;set;}
		public ColorRgba Color {get;set;}
		public bool Visible {get;set;}
		public float Opacity {get;set;}
		public TiledMap Parent {get;set;}
		
		public TiledObjectGroup()
		{
			Name = "UnnamedObjectGroup";
			Objects = new Dictionary<string, TiledObject>();
			Properties = new TiledPropertySet();
		}
		
		public void LoadObject( XElement node )
		{
			if( node.HasAttributes )
			{
				var obj = new TiledObject();
				obj.ObjectGroup = this;
				obj.Shape = TiledShape.Rectangle; // By default we expect rectangle object
				
				foreach( var attribute in node.Attributes() )
				{
					switch( attribute.Name.LocalName )
					{
						case "id":
							obj.ID = int.Parse( attribute.Value, System.Globalization.NumberStyles.Integer );
							break;
							
						case "name":
							obj.Name = attribute.Value;
							break;
							
						case "type":
							obj.Type = attribute.Value;
							break;
							
						case "x":
							obj.X = float.Parse(attribute.Value, System.Globalization.NumberStyles.Float);
							break;
							
						case "y":
							obj.Y = float.Parse(attribute.Value, System.Globalization.NumberStyles.Float);
							break;
							
						case "width":
							obj.W = float.Parse(attribute.Value, System.Globalization.NumberStyles.Float);
							break;
							
						case "height":
							obj.H = float.Parse(attribute.Value, System.Globalization.NumberStyles.Float);
							break;
							
						case "rotation":
							obj.Angle = float.Parse(attribute.Value, System.Globalization.NumberStyles.Float);
							break;
							
						case "visible":
							obj.Visible = false;
							break;
							
						default:
							Log.Editor.Write("Attribute '{0}' is not supported in objects.", attribute.Name);
							break;
					}
				}
				
				foreach( var element in node.Elements() )
				{
					switch( element.Name.LocalName )
					{
						case "properties":
							obj.LoadProperties( element );
							break;
							
						case "ellipse":
							obj.Shape = TiledShape.Ellipse;
							break;
							
						case "polygon":
							obj.LoadPolygon( element );
							break;
							
						case "polyline":
							obj.LoadPolyLine( element );
							break;
					}
				}
				
				Objects.Add(obj.Name, obj);
			}
		}
		
		public void LoadProperties( XElement node )
		{
			Properties.Extend( node );
		}
	}
}

/*
 * Created by SharpDevelop.
 * User: misthema
 * Date: 20.11.2015
 * Time: 11:52
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Xml.Linq;
using Duality;
using Duality.Resources;

namespace TileD_Plugin.TileD
{
	/// <summary>
	/// Description of TiledImageLayer.
	/// </summary>
	public class TiledImageLayer
	{
		public string Name {get;set;}
		public int X {get;set;}
		public int Y {get;set;}
		public float Opacity {get;set;}
		public bool Visible {get;set;}
		public ContentRef<Material> Image {get;set;}
		public TiledPropertySet Properties {get;set;}
		public TiledMap Parent {get;set;}
		
		public TiledImageLayer()
		{
			Name = "UnnamedImageLayer";
			Properties = new TiledPropertySet();
		}
		
		public void LoadProperties( XElement node )
		{
			Properties.Extend( node );
		}
		
		public void LoadImage( XElement node )
		{
			if( node != null && node.HasAttributes )
			{
				foreach( var attribute in node.Attributes() )
				{
					switch( attribute.Name.LocalName )
					{
						case "source":
							// TODO: Load ImageLayer image
							break;
							
						case "trans":
							// TODO: Load ImageLayer transparent color
							break;
							
						default:
							Log.Editor.WriteWarning("Attribute {0} not supported in image layer images.", attribute.Name);
							break;
					}
				}
			}
		}
	}
}

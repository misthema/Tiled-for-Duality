/*
 * User: misthema
 * Date: 20.11.2015
 * Time: 11:53
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Duality;


namespace TileD_Plugin.TileD
{
	/// <summary>
	/// Description of TiledPropertySet.
	/// </summary>
	public class TiledPropertySet
	{
		Dictionary<string, string> keys;
		
		public TiledPropertySet()
		{
			keys = new Dictionary<string, string>();
		}
		
		public void Extend( XElement node )
		{
			if( node == null )
			{
				Log.Editor.WriteError("Trying to extend TiledPropertySet with a NULL node!");
			}
			
			if( node.Name != "properties" )
			{
				Log.Editor.WriteWarning("Trying to extend TiledPropertySet with wrong node! (Node '{0}')", node.Name);
				return;
			}
			
			if( !node.HasElements )
			{
				Log.Editor.WriteWarning("Node '{0}' has no elements - nothing to load!", node.Name);
				return;
			}
			
			foreach( var child in node.Elements() )
			{
				if( child.Name != "property" )
					continue;
				
				if( child.HasAttributes )
				{
					XAttribute name = child.Attribute("name");
					XAttribute value = child.Attribute("value");
					
					if( name != null && value != null )
					{
						keys.Add(name.Value, value.Value);
					}
				}
			}
		}
		
		
		public string Get( string propName )
		{
			if( !keys.ContainsKey(propName) )
			{
				Log.Editor.WriteWarning("Trying to access a property '{0}' - does not exist!", propName);
				return string.Empty;
			}
			
			return keys[propName];
		}
		
		public int GetInt( string propName )
		{
			return int.Parse(Get(propName), System.Globalization.NumberStyles.Integer);
		}
		
		public float GetFloat( string propName )
		{
			return float.Parse(Get(propName), System.Globalization.NumberStyles.Float);
		}
		
		public bool GetBool( string propName )
		{
			if( Get(propName) == "true" || Get(propName) == "1" )
				return true;
			
			return false;
		}
	}
}

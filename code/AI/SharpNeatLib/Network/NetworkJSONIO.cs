using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using SharpNeat.Utility;

namespace SharpNeat.Network
{
	/// <summary>
	/// Static class for reading and writing Network Definitions(s) to and from XML.
	/// </summary>
	public static class NetworkJSONIO
	{
		public struct ActivationFunctionJSON
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public double Probability { get; set; }
		}

		public struct ActivationFunctionLibraryJSON
		{
			public List<ActivationFunctionJSON> ActivationFunctions { get; set; } = new();
		}

		public struct NetworkConnectionJSON
		{
			public uint SourceId { get; set; }
			public uint TargetId { get; set; }
			public double Weight { get; set; }
		}

		public struct NetworkNodeJSON
		{
			public NodeType Type { get; set; }
			public uint Id { get; set; }
			public int ActivationFnId { get; set; }
			public double[] AuxState { get; set; }
		}

		public struct NetworkDefinitionJSON
		{
			public List<NetworkNodeJSON> Nodes { get; set; } = new();
			public List<NetworkConnectionJSON> Connections { get; set; } = new();
		}

		public struct NetworkJSON
		{
			public ActivationFunctionLibraryJSON Library { get; set; }
			public List<NetworkDefinitionJSON> Networks { get; set; } = new();
		}

		#region Public Static Methods [Save to XmlDocument]

		/// <summary>
		/// Writes a single NetworkDefinition to XML within a containing 'Root' element and the activation function
		/// library that the genome is associated with.
		/// The XML is returned as a newly created XmlDocument.
		/// </summary>
		/// <param name="networkDef">The NetworkDefinition to save.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be emitted. They are required
		/// for HyperNEAT genomes but not for NEAT.</param>
		public static NetworkJSON SaveComplete( NetworkDefinition networkDef, bool nodeFnIds )
		{
			//NetworkJSON doc = new NetworkJSON();
			//WriteComplete( doc, networkDef, nodeFnIds );
			return SaveComplete( new List<NetworkDefinition>{ networkDef }, nodeFnIds );
		}

		/// <summary>
		/// Writes a list of NetworkDefinition(s) to XML within a containing 'Root' element and the activation
		/// function library that the genomes are associated with.
		/// The XML is returned as a newly created XmlDocument.
		/// </summary>
		/// <param name="networkDefList">List of genomes to write as XML.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be emitted. They are required
		/// for HyperNEAT genomes but not for NEAT.</param>
		public static NetworkJSON SaveComplete( IList<NetworkDefinition> networkDefList, bool nodeFnIds )
		{
			NetworkJSON doc = new NetworkJSON();
			WriteComplete( ref doc, networkDefList, nodeFnIds );

			return doc;
		}

		/// <summary>
		/// Writes a single NetworkDefinition to XML.
		/// The XML is returned as a newly created XmlDocument.
		/// </summary>
		/// <param name="networkDef">The genome to save.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be emitted. They are required
		/// for HyperNEAT genomes but not for NEAT.</param>
		public static NetworkDefinitionJSON Save( NetworkDefinition networkDef, bool nodeFnIds )
		{
			NetworkDefinitionJSON doc = new NetworkDefinitionJSON();
			Write( ref doc, networkDef, nodeFnIds );

			return doc;
		}

		#endregion

		#region Public Static Methods [Load from XmlDocument]

		/// <summary>
		/// Reads a list of NetworkDefinition(s) from XML that has a containing 'Root' element. The root element 
		/// also contains the activation function library that the network definitions are associated with.
		/// </summary>
		/// <param name="xmlNode">The XmlNode to read from. This can be an XmlDocument or XmlElement.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be read. If false then 
		/// all node activation function IDs default to 0.</param>
		public static List<NetworkDefinition> LoadCompleteGenomeList( NetworkJSON xmlNode, bool nodeFnIds )
		{
			return ReadCompleteNetworkDefinitionList( xmlNode, nodeFnIds );
		}

		/// <summary>
		/// Reads a NetworkDefinition from XML.
		/// </summary>
		/// <param name="xmlNode">The XmlNode to read from. This can be an XmlDocument or XmlElement.</param>
		/// <param name="activationFnLib">The activation function library used to decode node activation function IDs.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be read. They are required
		/// for HyperNEAT genomes but not for NEAT.  If false then all node activation function IDs default to 0.</param>
		public static NetworkDefinition ReadGenome( NetworkDefinitionJSON xmlNode, IActivationFunctionLibrary activationFnLib, bool nodeFnIds )
		{
			return ReadNetworkDefinition( xmlNode, activationFnLib, nodeFnIds );
		}

		#endregion

		#region Public Static Methods [Write to XML]

		/// <summary>
		/// Writes a list of INetworkDefinition(s) to XML within a containing 'Root' element and the activation
		/// function library that the genomes are associated with.
		/// </summary>
		/// <param name="xw">XmlWriter to write XML to.</param>
		/// <param name="networkDefList">List of network definitions to write as XML.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be emitted. They are required
		/// for HyperNEAT genomes but not for NEAT.</param>
		public static void WriteComplete( ref NetworkJSON xw, IList<NetworkDefinition> networkDefList, bool nodeFnIds )
		{
			int count = networkDefList.Count;
			List<INetworkDefinition> tmpList = new List<INetworkDefinition>( count );
			foreach ( NetworkDefinition networkDef in networkDefList )
			{
				tmpList.Add( networkDef );
			}
			WriteComplete( ref xw, tmpList, nodeFnIds );
		}

		/// <summary>
		/// Writes a list of INetworkDefinition(s) to XML within a containing 'Root' element and the activation
		/// function library that the genomes are associated with.
		/// </summary>
		/// <param name="xw">XmlWriter to write XML to.</param>
		/// <param name="networkDefList">List of network definitions to write as XML.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be emitted. They are required
		/// for HyperNEAT genomes but not for NEAT.</param>
		public static void WriteComplete( ref NetworkJSON xw, IList<INetworkDefinition> networkDefList, bool nodeFnIds )
		{
			if ( networkDefList.Count == 0 )
			{   // Nothing to do.
				return;
			}

			// <Root>
			//xw.WriteStartElement( __ElemRoot );

			// Write activation function library from the first network definition 
			// (we expect all networks to use the same library).
			IActivationFunctionLibrary activationFnLib = networkDefList[0].ActivationFnLibrary;
			var lib = new ActivationFunctionLibraryJSON();
			Write( ref lib, activationFnLib );
			xw.Library = lib;

			// <Networks>
			//xw.WriteStartElement( __ElemNetworks );

			// Write networks.
			foreach ( INetworkDefinition networkDef in networkDefList )
			{
				Debug.Assert( networkDef.ActivationFnLibrary == activationFnLib );
				NetworkDefinitionJSON def = new NetworkDefinitionJSON();
				Write( ref def, networkDef, nodeFnIds );
				xw.Networks.Add(def);
			}

			// </Networks>
			//xw.WriteEndElement();

			// </Root>
			//xw.WriteEndElement();
		}

		/// <summary>
		/// Writes a single INetworkDefinition to XML within a containing 'Root' element and the activation
		/// function library that the genome is associated with.
		/// </summary>
		/// <param name="xw">XmlWriter to write XML to.</param>
		/// <param name="networkDef">Network definition to write as XML.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be emitted. They are required
		/// for HyperNEAT genomes but not for NEAT.</param>
		public static void WriteComplete( ref NetworkJSON xw, INetworkDefinition networkDef, bool nodeFnIds )
		{
			// <Root>
			//xw.WriteStartElement( __ElemRoot );

			// Write activation function library.
			var lib = new ActivationFunctionLibraryJSON();
			Write( ref lib, networkDef.ActivationFnLibrary );
			xw.Library = lib;

			// <Networks>
			//xw.WriteStartElement( __ElemNetworks );

			// Write single network.
			var network = new NetworkDefinitionJSON();
			Write( ref network, networkDef, nodeFnIds );
			xw.Networks.Add(network);

			// </Networks>
			//xw.WriteEndElement();

			// </Root>
			//xw.WriteEndElement();
		}

		/// <summary>
		/// Writes an INetworkDefinition to XML.
		/// </summary>
		/// <param name="xw">XmlWriter to write XML to.</param>
		/// <param name="networkDef">Network definition to write as XML.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be emitted. They are required
		/// for HyperNEAT genomes but not for NEAT.</param>
		public static void Write( ref NetworkDefinitionJSON xw, INetworkDefinition networkDef, bool nodeFnIds )
		{
			//xw.WriteStartElement( __ElemNetwork );

			// Emit nodes.
			//xw.WriteStartElement( __ElemNodes );
			foreach ( INetworkNode node in networkDef.NodeList )
			{
				NetworkNodeJSON nodeData = new NetworkNodeJSON();
				//xw.WriteStartElement( __ElemNode );
				nodeData.Type = node.NodeType;
				nodeData.Id = node.Id;
				if ( nodeFnIds )
				{
					nodeData.ActivationFnId = node.ActivationFnId;
				}
				//xw.WriteEndElement();
				xw.Nodes.Add(nodeData);
			}
			//xw.WriteEndElement();

			// Emit connections.
			//xw.WriteStartElement( __ElemConnections );
			foreach ( INetworkConnection con in networkDef.ConnectionList )
			{
				NetworkConnectionJSON connectionData = new NetworkConnectionJSON();
				connectionData.SourceId = con.SourceNodeId;
				connectionData.TargetId = con.TargetNodeId;
				connectionData.Weight = con.Weight;

				xw.Connections.Add(connectionData);
			}
			//xw.WriteEndElement();

			// </Network>
			//xw.WriteEndElement();
		}

		/// <summary>
		/// Writes an activation function library to XML. This links activation function names to the 
		/// integer IDs used by network nodes, which allows us emit just the ID for each node thus 
		/// resulting in XML that is more compact compared to emitting the activation function name for
		/// each node.
		/// </summary>
		public static void Write( ref ActivationFunctionLibraryJSON xw, IActivationFunctionLibrary activationFnLib )
		{
			//xw.WriteStartElement( __ElemActivationFunctions );
			IList<ActivationFunctionInfo> fnList = activationFnLib.GetFunctionList();
			foreach ( ActivationFunctionInfo fnInfo in fnList )
			{
				ActivationFunctionJSON data = new ActivationFunctionJSON();
				data.Id = fnInfo.Id;
				data.Name = fnInfo.ActivationFunction.FunctionId;
				data.Probability = fnInfo.SelectionProbability;

				xw.ActivationFunctions.Add(data);
			}
			//xw.WriteEndElement();
		}

		#endregion

		#region Public Static Methods [Read from XML]

		/// <summary>
		/// Reads a list of NetworkDefinition(s) from XML that has a containing 'Root' element. The root 
		/// element also contains the activation function library that the genomes are associated with.
		/// </summary>
		/// <param name="xr">The XmlReader to read from.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be read. They are required
		/// for HyperNEAT genomes but not NEAT</param>
		public static List<NetworkDefinition> ReadCompleteNetworkDefinitionList( NetworkJSON xr, bool nodeFnIds )
		{
			// Find <Root>.
			//XmlIoUtils.MoveToElement( xr, false, __ElemRoot );

			// Read IActivationFunctionLibrray. 
			//XmlIoUtils.MoveToElement( xr, true, __ElemActivationFunctions );
			IActivationFunctionLibrary activationFnLib = ReadActivationFunctionLibrary( xr.Library );
			//XmlIoUtils.MoveToElement( xr, false, __ElemNetworks );

			List<NetworkDefinition> networkDefList = new List<NetworkDefinition>();
			foreach (var network in xr.Networks)
			{
				NetworkDefinition networkDef = ReadNetworkDefinition( network, activationFnLib, nodeFnIds );
				networkDefList.Add( networkDef );
			}

			return networkDefList;
		}

		/// <summary>
		/// Reads a network definition from XML. 
		/// An activation function library is required to decode the function ID at each node, typically the
		/// library is stored alongside the network definition XML and will have already been read elsewhere and
		/// passed in here.
		/// </summary>
		/// <param name="xr">The XmlReader to read from.</param>
		/// <param name="activationFnLib">The activation function library used to decode node activation function IDs.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be read. They are required
		/// for HyperNEAT genomes but not NEAT</param>
		public static NetworkDefinition ReadNetworkDefinition( NetworkDefinitionJSON xr, IActivationFunctionLibrary activationFnLib, bool nodeFnIds )
		{
			// Find <Network>.
			//XmlIoUtils.MoveToElement( xr, false, __ElemNetwork );
			//int initialDepth = xr.Depth;

			// Find <Nodes>.
			//XmlIoUtils.MoveToElement( xr, true, __ElemNodes );

			// Create a reader over the <Nodes> sub-tree.
			int inputNodeCount = 0;
			int outputNodeCount = 0;
			NodeList nodeList = new NodeList();
			foreach (var nodeData in xr.Nodes)
			{
				NodeType nodeType = nodeData.Type;
				uint id = nodeData.Id;
				int fnId = 0;
				double[] auxState = null;
				if ( nodeFnIds )
				{   // Read activation fn ID.
					fnId = nodeData.ActivationFnId;

					// Read aux state as comma separated list of real values.
					auxState = nodeData.AuxState;
				}

				// TODO: Read node aux state data.
				NetworkNode node = new NetworkNode( id, nodeType, fnId, auxState );
				nodeList.Add( node );

				// Track the number of input and output nodes.
				switch ( nodeType )
				{
					case NodeType.Input:
						inputNodeCount++;
						break;
					case NodeType.Output:
						outputNodeCount++;
						break;
				}
			}


			// Find <Connections>.
			//XmlIoUtils.MoveToElement( xr, false, __ElemConnections );

			// Create a reader over the <Connections> sub-tree.
			ConnectionList connList = new ConnectionList();
			foreach (var connectionData in xr.Connections)
			{
				uint srcId = connectionData.SourceId;
				uint tgtId = connectionData.TargetId;
				double weight = connectionData.Weight;
				NetworkConnection conn = new NetworkConnection( srcId, tgtId, weight );
				connList.Add( conn );
			}

			// Move the reader beyond the closing tags </Connections> and </Network>.


			// Construct and return loaded network definition.
			return new NetworkDefinition( inputNodeCount, outputNodeCount, activationFnLib, nodeList, connList );
		}

		/// <summary>
		/// Reads an IActivationFunctionLibrary from the provided XmlReader.
		/// </summary>
		public static IActivationFunctionLibrary ReadActivationFunctionLibrary( ActivationFunctionLibraryJSON xr )
		{
			//XmlIoUtils.MoveToElement( xr, false, __ElemActivationFunctions );

			// Create a reader over the sub-tree.
			List<ActivationFunctionInfo> fnList = new List<ActivationFunctionInfo>();
			foreach ( var functionData in xr.ActivationFunctions )
			{
				int id = functionData.Id;
				double selectionProb = functionData.Probability;
				string fnName = functionData.Name;

				// Lookup function name.
				IActivationFunction activationFn = ActivationFunctionRegistry.GetActivationFunction( fnName );

				// Add new function to our list of functions.
				ActivationFunctionInfo fnInfo = new ActivationFunctionInfo( id, selectionProb, activationFn );
				fnList.Add( fnInfo );
			}

			// If we have read library items then ensure that their selection probabilities are normalized.
			if ( fnList.Count != 0 )
			{
				NormalizeSelectionProbabilities( fnList );
			}
			return new DefaultActivationFunctionLibrary( fnList );
		}

		#endregion

		#region Public Static Methods [Low-level XML Parsing]


		#endregion

		#region Private Static Methods

		/// <summary>
		/// Normalize the selection probabilities of the provided ActivationFunctionInfo items.
		/// </summary>
		private static void NormalizeSelectionProbabilities( IList<ActivationFunctionInfo> fnList )
		{
			double total = 0.0;
			int count = fnList.Count;
			for ( int i = 0; i < count; i++ )
			{
				total += fnList[i].SelectionProbability;
			}
			if ( Math.Abs( total - 1.0 ) < 0.0001 )
			{   // Probabilities already normalized to within acceptable limits (from rounding errors).
				return;
			}

			// Normalize the probabilities. Note that ActivationFunctionInfo is immutable therefore
			// we replace the existing items.
			for ( int i = 0; i < count; i++ )
			{
				ActivationFunctionInfo item = fnList[i];
				fnList[i] = new ActivationFunctionInfo( item.Id, item.SelectionProbability / total, item.ActivationFunction );
			}
		}

		#endregion
	}
}

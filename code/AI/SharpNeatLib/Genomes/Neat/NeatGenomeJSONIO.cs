using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SharpNeat.Network;
using SharpNeat.Utility;

namespace SharpNeat.Genomes.Neat
{
	public struct NeatNodeJSON
	{
		public NodeType Type { get; set; }
		public uint Id { get; set; }

		public int ActivationFunctionId { get; set; }
		public double[] AuxState { get; set; }
	}

	public struct NeatConnectionJSON
	{
		public uint Id { get; set; }
		public uint SourceId { get; set; }
		public uint TargetId { get; set; }
		public double Weight { get; set; }
	}

	public struct NeatGenomeJSON
	{
		public uint Id { get; set; }
		public uint BirthGen { get; set; }
		public double Fitness { get; set; }
		public List<NeatNodeJSON> Nodes { get; set; } = new();
		public List<NeatConnectionJSON> Connections { get; set; } = new();
	}

	public struct NeatDataJSON
	{
		public NetworkJSONIO.ActivationFunctionLibraryJSON Library { get; set; }
		public List<NeatGenomeJSON> Genomes { get; set; } = new();
	}

	/// <summary>
	/// Static class for reading and writing NeatGenome(s) to and from XML.
	/// </summary>
	public static class NeatGenomeJSONIO
	{

		/// <summary>
		/// Writes a single NeatGenome to XML within a containing 'Root' element and the activation function
		/// library that the genome is associated with.
		/// The XML is returned as a newly created XmlDocument.
		/// </summary>
		/// <param name="genome">The genome to save.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be emitted. They are required
		/// for HyperNEAT genomes but not for NEAT.</param>
		public static NeatDataJSON SaveComplete( NeatGenome genome, bool nodeFnIds )
		{
			//NeatJSONData doc = new NeatJSONData();
			return SaveComplete( new List<NeatGenome>{ genome }, nodeFnIds );

			//return doc;
		}

		/// <summary>
		/// Writes a list of NeatGenome(s) to XML within a containing 'Root' element and the activation
		/// function library that the genomes are associated with.
		/// The XML is returned as a newly created XmlDocument.
		/// </summary>
		/// <param name="genomeList">List of genomes to write as XML.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be emitted. They are required
		/// for HyperNEAT genomes but not for NEAT.</param>
		public static NeatDataJSON SaveComplete( IList<NeatGenome> genomeList, bool nodeFnIds )
		{
			NeatDataJSON doc = new NeatDataJSON();
			WriteComplete( ref doc, genomeList, nodeFnIds );

			return doc;
		}

		/// <summary>
		/// Writes a single NeatGenome to XML.
		/// The XML is returned as a newly created XmlDocument.
		/// </summary>
		/// <param name="genome">The genome to save.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be emitted. They are required
		/// for HyperNEAT genomes but not for NEAT.</param>
		//public static NeatJSONData Save( NeatGenome genome, bool nodeFnIds )
		//{
		//	NeatJSONData doc = new NeatJSONData();

		//	Write( doc, genome, nodeFnIds );

		//	return doc;
		//}

		/// <summary>
		/// Loads a list of NeatGenome(s) from XML that has a containing 'Root' element. The root element 
		/// also contains the activation function library that the network definitions are associated with.
		/// </summary>
		/// <param name="xmlNode">The XmlNode to read from. This can be an XmlDocument or XmlElement.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be read. If false then 
		/// all node activation function IDs default to 0.</param>
		/// <param name="genomeFactory">A NeatGenomeFactory object to construct genomes against.</param>
		public static List<NeatGenome> LoadCompleteGenomeList( NeatDataJSON xmlNode, bool nodeFnIds, NeatGenomeFactory genomeFactory )
		{
			return ReadCompleteGenomeList( xmlNode, nodeFnIds, genomeFactory );
		}

		/// <summary>
		/// Reads a NeatGenome from XML.
		/// </summary>
		/// <param name="xmlNode">The XmlNode to read from. This can be an XmlDocument or XmlElement.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be read. They are required
		/// for HyperNEAT genomes but not for NEAT</param>
		//public static NeatGenome LoadGenome( XmlNode xmlNode, bool nodeFnIds )
		//{
		//	using ( XmlNodeReader xr = new XmlNodeReader( xmlNode ) )
		//	{
		//		return ReadGenome( xr, nodeFnIds );
		//	}
		//}

		/// <summary>
		/// Writes a list of NeatGenome(s) to XML within a containing 'Root' element and the activation
		/// function library that the genomes are associated with.
		/// </summary>
		/// <param name="xw">XmlWriter to write XML to.</param>
		/// <param name="genomeList">List of genomes to write as XML.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be emitted. They are required
		/// for HyperNEAT genomes but not for NEAT.</param>
		public static void WriteComplete( ref NeatDataJSON xw, IList<NeatGenome> genomeList, bool nodeFnIds )
		{
			if ( genomeList.Count == 0 )
			{   // Nothing to do.
				return;
			}

			// <Root>
			//xw.WriteStartElement( __ElemRoot );

			// Write activation function library from the first genome.
			// (we expect all genomes to use the same library).
			IActivationFunctionLibrary activationFnLib = genomeList[0].ActivationFnLibrary;
			var lib = new NetworkJSONIO.ActivationFunctionLibraryJSON();
			NetworkJSONIO.Write( ref lib, activationFnLib );
			xw.Library = lib;

			// <Networks>
			//xw.WriteStartElement( __ElemNetworks );

			// Write genomes.
			foreach ( NeatGenome genome in genomeList )
			{
				Debug.Assert( genome.ActivationFnLibrary == activationFnLib );

				NeatGenomeJSON data = new NeatGenomeJSON();
				Write( ref data, genome, nodeFnIds );
				xw.Genomes.Add(data);
			}

			// </Networks>
			//xw.WriteEndElement();

			// </Root>
			//xw.WriteEndElement();
		}


		/// <summary>
		/// Writes a NeatGenome to XML.
		/// </summary>
		/// <param name="xw">XmlWriter to write XML to.</param>
		/// <param name="genome">Genome to write as XML.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be emitted. They are required
		/// for HyperNEAT genomes but not for NEAT.</param>
		public static void Write( ref NeatGenomeJSON xw, NeatGenome genome, bool nodeFnIds )
		{
			//xw.WriteStartElement( __ElemNetwork );

			xw.Id = genome.Id;
			xw.BirthGen = genome.BirthGeneration;
			xw.Fitness = genome.EvaluationInfo.Fitness;

			// Emit nodes.
			//StringBuilder sb = new StringBuilder();
			//xw.WriteStartElement( __ElemNodes );
			foreach ( NeuronGene nGene in genome.NeuronGeneList )
			{
				NeatNodeJSON node = new NeatNodeJSON();
				node.Type = nGene.NodeType;
				node.Id = nGene.Id;

				if ( nodeFnIds )
				{   // Write activation fn ID.
					node.ActivationFunctionId = nGene.ActivationFnId;

					// Write aux state as comma separated list of real values.
					node.AuxState = nGene.AuxState;
				}
				//xw.WriteEndElement();
				xw.Nodes.Add(node);
			}
			//xw.WriteEndElement();

			// Emit connections.
			//xw.WriteStartElement( __ElemConnections );
			foreach ( ConnectionGene cGene in genome.ConnectionList )
			{
				NeatConnectionJSON connection = new NeatConnectionJSON();
				connection.Id = cGene.InnovationId;
				connection.SourceId = cGene.SourceNodeId;
				connection.TargetId = cGene.TargetNodeId;
				connection.Weight = cGene.Weight;

				xw.Connections.Add(connection);
			}
			//xw.WriteEndElement();

			// </Network>
			//xw.WriteEndElement();
		}

		/// <summary>
		/// Reads a list of NeatGenome(s) from XML that has a containing 'Root' element. The root 
		/// element also contains the activation function library that the genomes are associated with.
		/// </summary>
		/// <param name="xr">The XmlReader to read from.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be read. If false then 
		/// all node activation function IDs default to 0.</param>
		/// <param name="genomeFactory">A NeatGenomeFactory object to construct genomes against.</param>
		public static List<NeatGenome> ReadCompleteGenomeList( NeatDataJSON xr, bool nodeFnIds, NeatGenomeFactory genomeFactory )
		{
			// Find <Root>.
			//XmlIoUtils.MoveToElement( xr, false, __ElemRoot );

			// Read IActivationFunctionLibrary. This library is not used, it is compared against the one already present in the 
			// genome factory to confirm that the loaded genomes are compatible with the genome factory.
			//XmlIoUtils.MoveToElement( xr, true, __ElemActivationFunctions );
			IActivationFunctionLibrary activationFnLib = NetworkJSONIO.ReadActivationFunctionLibrary( xr.Library );
			//XmlIoUtils.MoveToElement( xr, false, __ElemNetworks );

			// Read genomes.
			List<NeatGenome> genomeList = new List<NeatGenome>();
			foreach (var genomeData in xr.Genomes)
			{
				NeatGenome genome = ReadGenome( genomeData, nodeFnIds );
				genomeList.Add( genome );
			}

			// Check for empty list.
			if ( genomeList.Count == 0 )
			{
				return genomeList;
			}

			// Get the number of inputs and outputs expected by the genome factory.
			int inputCount = genomeFactory.InputNeuronCount;
			int outputCount = genomeFactory.OutputNeuronCount;

			// Check all genomes have the same number of inputs & outputs.
			// Also track the highest genomeID and innovation ID values; we need these to construct a new genome factory.
			uint maxGenomeId = 0;
			uint maxInnovationId = 0;

			foreach ( NeatGenome genome in genomeList )
			{
				// Check number of inputs/outputs.
				if ( genome.InputNeuronCount != inputCount || genome.OutputNeuronCount != outputCount )
				{
					throw new SharpNeatException( string.Format( "Genome with wrong number of inputs and/or outputs, expected [{0}][{1}] got [{2}][{3}]",
															   inputCount, outputCount, genome.InputNeuronCount, genome.OutputNeuronCount ) );
				}

				// Track max IDs.
				maxGenomeId = Math.Max( maxGenomeId, genome.Id );

				// Node and connection innovation IDs are in the same ID space.
				foreach ( NeuronGene nGene in genome.NeuronGeneList )
				{
					maxInnovationId = Math.Max( maxInnovationId, nGene.InnovationId );
				}

				// Register connection IDs.
				foreach ( ConnectionGene cGene in genome.ConnectionGeneList )
				{
					maxInnovationId = Math.Max( maxInnovationId, cGene.InnovationId );
				}
			}

			// Check that activation functions in XML match that in the genome factory.
			IList<ActivationFunctionInfo> loadedActivationFnList = activationFnLib.GetFunctionList();
			IList<ActivationFunctionInfo> factoryActivationFnList = genomeFactory.ActivationFnLibrary.GetFunctionList();
			if ( loadedActivationFnList.Count != factoryActivationFnList.Count )
			{
				throw new SharpNeatException( "The activation function library loaded from XML does not match the genome factory's activation function library." );
			}

			for ( int i = 0; i < factoryActivationFnList.Count; i++ )
			{
				if ( (loadedActivationFnList[i].Id != factoryActivationFnList[i].Id)
					|| (loadedActivationFnList[i].ActivationFunction.FunctionId != factoryActivationFnList[i].ActivationFunction.FunctionId) )
				{
					throw new SharpNeatException( "The activation function library loaded from XML does not match the genome factory's activation function library." );
				}
			}

			// Initialise the genome factory's genome and innovation ID generators.
			genomeFactory.GenomeIdGenerator.Reset( Math.Max( genomeFactory.GenomeIdGenerator.Peek, maxGenomeId + 1 ) );
			genomeFactory.InnovationIdGenerator.Reset( Math.Max( genomeFactory.InnovationIdGenerator.Peek, maxInnovationId + 1 ) );

			// Retrospectively assign the genome factory to the genomes. This is how we overcome the genome/genomeFactory
			// chicken and egg problem.
			foreach ( NeatGenome genome in genomeList )
			{
				genome.GenomeFactory = genomeFactory;
			}

			return genomeList;
		}

		/// <summary>
		/// Reads a NeatGenome from XML.
		/// </summary>
		/// <param name="xr">The XmlReader to read from.</param>
		/// <param name="nodeFnIds">Indicates if node activation function IDs should be read. They are required
		/// for HyperNEAT genomes but not for NEAT</param>
		public static NeatGenome ReadGenome( NeatGenomeJSON xr, bool nodeFnIds )
		{
			// Find <Network>.
			//XmlIoUtils.MoveToElement( xr, false, __ElemNetwork );
			//int initialDepth = xr.Depth;

			// Read genome ID attribute if present. Otherwise default to zero; it's the caller's responsibility to 
			// check IDs are unique and in-line with the genome factory's ID generators.
			//string genomeIdStr = xr.GetAttribute( __AttrId );
			uint genomeId = xr.Id;
			//uint.TryParse( genomeIdStr, out genomeId );

			// Read birthGeneration attribute if present. Otherwise default to zero.
			//string birthGenStr = xr.GetAttribute( __AttrBirthGeneration );
			uint birthGen = xr.BirthGen;
			//uint.TryParse( birthGenStr, out birthGen );

			// Find <Nodes>.
			//XmlIoUtils.MoveToElement( xr, true, __ElemNodes );

			// Create a reader over the <Nodes> sub-tree.
			int inputNodeCount = 0;
			int outputNodeCount = 0;
			NeuronGeneList nGeneList = new NeuronGeneList();
			foreach (var nodeData in xr.Nodes)
			{
				//NodeType neuronType = NetworkXmlIO.ReadAttributeAsNodeType( xrSubtree, __AttrType );
				NodeType neuronType = nodeData.Type;
				uint id = nodeData.Id;
				int functionId = 0;
				double[] auxState = null;
				if ( nodeFnIds )
				{   // Read activation fn ID.
					functionId = nodeData.ActivationFunctionId;

					// Read aux state as comma separated list of real values.
					auxState = nodeData.AuxState;
				}

				NeuronGene nGene = new NeuronGene( id, neuronType, functionId, auxState );
				nGeneList.Add( nGene );

				// Track the number of input and output nodes.
				switch ( neuronType )
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
			ConnectionGeneList cGeneList = new ConnectionGeneList();
			foreach (var connectionData in xr.Connections)
			{
				uint id = connectionData.Id;
				uint srcId = connectionData.SourceId;
				uint tgtId = connectionData.TargetId;
				double weight = connectionData.Weight;
				ConnectionGene cGene = new ConnectionGene( id, srcId, tgtId, weight );
				cGeneList.Add( cGene );
			}

			// Construct and return loaded NeatGenome.
			return new NeatGenome( null, genomeId, birthGen, nGeneList, cGeneList, inputNodeCount, outputNodeCount, true );
		}

	}
}

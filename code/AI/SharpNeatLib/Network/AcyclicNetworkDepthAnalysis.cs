﻿/* ***************************************************************************
 * This file is part of SharpNEAT - Evolution of Neural Networks.
 * 
 * Copyright 2004-2016 Colin Green (sharpneat@gmail.com)
 *
 * SharpNEAT is free software; you can redistribute it and/or modify
 * it under the terms of The MIT License (MIT).
 *
 * You should have received a copy of the MIT License
 * along with SharpNEAT; if not, see https://opensource.org/licenses/MIT.
 */
using System.Collections.Generic;

namespace SharpNeat.Network
{
    /// <summary>
    /// An algorithm for analysing acyclic networks and calculating the depth of each node in the network.
    /// Bias and input nodes are defined as being at depth 0, the depth of all other nodes is defined as 
    /// the maximum number of hops from the depth 0 nodes, so where multiple paths exist to a node (potentially
    /// with different numbers of hops) we take the maximum number of hops as that node's depth. 
    /// </summary>
    public class AcyclicNetworkDepthAnalysis
    {
        /// <summary>
        /// Connectivity data for the INetworkDefinition that is currently being tested.
        /// </summary>
        NetworkConnectivityData _networkConnectivityData;

        /// <summary>
        /// Dictionary of node depths keyed by node ID. Working data.
        /// </summary>
        readonly Dictionary<uint,int> _nodeDepthById = new Dictionary<uint,int>();

        #region Public Methods

        /// <summary>
        /// Calculate node depths in an acyclic network.
        /// </summary>
        public NetworkDepthInfo CalculateNodeDepths(INetworkDefinition networkDef)
        {
            // Clear any existing state (allow reuse of this class).
            _nodeDepthById.Clear();

            // Get and store connectivity data for the network.
            _networkConnectivityData = networkDef.GetConnectivityData();

            // Loop over all input (and bias) nodes; Perform a depth first traversal of each in turn.
            int inputAndBiasCount = networkDef.InputNodeCount + 1;
            for(int i=0; i<inputAndBiasCount; i++)
            {
                TraverseNode(_networkConnectivityData.GetNodeDataByIndex(i), 0);
            }

            // Extract node depths from _nodeDepthById into an array of depths (node depth by node index).
            // Note. Any node not in the dictionary is in an isolated sub-network and will be assigned to 
            // layer 0 by default.
            INodeList nodeList = networkDef.NodeList;
            int nodeCount = nodeList.Count;
            int[] nodeDepthArr = new int[nodeCount];
            int maxDepth = 0;

            // Loop over nodes and set the node depth. Skip over input and bias nodes, they are defined as 
            // being in layer zero.
            for(int i=inputAndBiasCount; i<nodeCount; i++)
            {
                // Lookup the node's depth. If not found depth remains set to zero.
                int depth;
                if(_nodeDepthById.TryGetValue(nodeList[i].Id, out depth)) 
                {
                    nodeDepthArr[i] = depth;
                    // Also determine maximum depth, that is, total depth of the network.
                    if(depth > maxDepth) {
                        maxDepth = depth;
                    }
                }
            }

            // Return depth analysis info.
            return new NetworkDepthInfo(maxDepth+1, nodeDepthArr);
        }

        #endregion

        #region Private Methods

        private void TraverseNode(NodeConnectivityData nodeData, int depth)
        {
            // Check if the node has been visited before.
            int assignedDepth;
            if(_nodeDepthById.TryGetValue(nodeData._id, out assignedDepth) && assignedDepth >= depth)
            {   // The node already has already been visited via a path that assigned it a greater depth than the 
                // current path. Stop traversing this path.
                return;
            }

            // Either this is the first visit to the node *or* the node has been visited, but via a shorter path.
            // Either way we assign it the current depth value and traverse into its targets to update/set their depth.
            _nodeDepthById[nodeData._id] = depth;
            foreach(uint targetId in nodeData._tgtNodes)
            {
                TraverseNode(_networkConnectivityData.GetNodeDataById(targetId), depth + 1);
            }
        }

        #endregion
    }
}

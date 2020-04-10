/* ========================================================================
 * Copyright (c) 2005-2020 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/

using System;
using System.Collections.Generic;
using System.IO;
using Opc.Ua;
using Opc.Ua.Server;
using Opc.Ua.Export;
using NodeSet2TestingServer.Bl.Config;

namespace NodeSet2TestingServer.BL
{
    public class NodeSet2TestingNodeManager : CustomNodeManager2
    {

        #region Constructors
        public NodeSet2TestingNodeManager(IServerInternal server, ApplicationConfiguration configuration, string nodeSet2Path)
            :base (server, configuration)
        {
            if (string.IsNullOrWhiteSpace(nodeSet2Path))
            {
                throw new ArgumentNullException(nameof(nodeSet2Path));
            }

            SystemContext.NodeIdFactory = this;

            m_NodeSet2Path = nodeSet2Path;
            m_LastUsedNodeId = 0;

            //todo optimize later
            using (var stream = new FileStream(m_NodeSet2Path, FileMode.Open))
            {
                var nodeSet = UANodeSet.Read(stream);
                SetNamespaces(nodeSet.NamespaceUris);
            }

            m_Configuration = configuration.ParseExtension<NodeSet2TestingServerConfiguration>();

            if (m_Configuration == null)
            {
                m_Configuration = new NodeSet2TestingServerConfiguration();
            }
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// An overrideable version of the Dispose.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TBD
            }
        }
        #endregion

        #region INodeIdFactory Members
        public override NodeId New(ISystemContext context, NodeState node)
        {
            BaseInstanceState instance = node as BaseInstanceState;

            var namespaceIndex = instance?.NodeId?.NamespaceIndex ?? NamespaceIndex;
            uint id = Utils.IncrementIdentifier(ref m_LastUsedNodeId);
            return new NodeId(id, namespaceIndex);
        }
        #endregion

        #region INodeManager Members
        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            lock (Lock)
            {
                IList<IReference> references = null;

                if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out references))
                {
                    externalReferences[ObjectIds.ObjectsFolder] = references = new List<IReference>();
                }

                ImportNodeSet2Xml(externalReferences, m_NodeSet2Path);
            }
        }
        #endregion

        protected override NodeHandle GetManagerHandle(ServerSystemContext context, NodeId nodeId, IDictionary<NodeId, NodeState> cache)
        {
            lock (Lock)
            {
                // quickly exclude nodes that are not in the namespace. 
                if (!IsNodeIdInNamespace(nodeId))
                {
                    return null;
                }

                NodeState node = null;

                // check cache (the cache is used because the same node id can appear many times in a single request).
                if (cache != null)
                {
                    if (cache.TryGetValue(nodeId, out node))
                    {
                        return new NodeHandle(nodeId, node);
                    }
                }

                // look up predefined node.
                if (PredefinedNodes.TryGetValue(nodeId, out node))
                {
                    NodeHandle handle = new NodeHandle(nodeId, node);

                    if (cache != null)
                    {
                        cache.Add(nodeId, node);
                    }

                    return handle;
                }

                // node not found.
                return null;
            }
        }

        protected override NodeStateCollection LoadPredefinedNodes(ISystemContext context)
        {
            return m_PredefinedNodes;
        }
        
        /// <summary>
        /// Reading NodeSet2.xml to initialize the server
        /// Adopted from https://github.com/OPCFoundation/UA-.NETStandard/issues/546
        /// </summary>
        /// <param name="externalReferences"></param>
        /// <param name="resourcepath">Fullpath to NodeSet2.xml</param>
        private void ImportNodeSet2Xml(IDictionary<NodeId, IList<IReference>> externalReferences, string resourcepath)
        {
            m_PredefinedNodes = new NodeStateCollection();

            using (var stream = new FileStream(resourcepath, FileMode.Open))
            {
                var nodeSet = UANodeSet.Read(stream);
                
                nodeSet.Import(SystemContext, m_PredefinedNodes);

                for (int ii = 0; ii < m_PredefinedNodes.Count; ii++)
                {
                    //AddRootNotifier(m_PredefinedNodes[ii]);
                    AddPredefinedNode(SystemContext, m_PredefinedNodes[ii]);
                    
                }
                // ensure the reverse refernces exist.
                AddReverseReferences(externalReferences);
            }
        }

        #region Private Fields
        private readonly NodeSet2TestingServerConfiguration m_Configuration;
        private readonly string m_NodeSet2Path;
        private long m_LastUsedNodeId;
        private NodeStateCollection m_PredefinedNodes;
        #endregion
    }
}

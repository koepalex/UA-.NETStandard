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
using System.IO;
using Opc.Ua.Configuration;
using Opc.Ua.Server;
using Opc.Ua.Server.Controls;
using Server = NodeSet2TestingServer.BL.NodeSet2TestingServer;

namespace NodeSet2TestingServer.UI.Controller
{
    public interface INodeSet2TestingFormController
    {
        /// <summary>
        /// Get's or Set's the path to NodeSet2 XML
        /// </summary>
        string NodeSet2Path { get; set; }

        /// <summary>
        /// Checks wheter the fullpath to NodeSet2 file is correct
        /// </summary>
        /// <param name="path">Fullpath to NodeSet2.xml</param>
        /// <returns>true if valid</returns>
        bool IsNodeSet2PathValid(string path);

        /// <summary>
        /// Starting a OPC Server instance with given NodeSet2 XML
        /// </summary>
        void StartServer();

        void ConfigureServerDiagnosticsControl(ServerDiagnosticsCtrl serverDiagnosticsCtrl);

        /// <summary>
        /// Stopping the OPC Server
        /// </summary>
        void StopServer();
    }

    public class NodeSet2TestingFormController : INodeSet2TestingFormController
    {
        private readonly ApplicationInstance m_App;
        private bool m_IsRunning;

        public NodeSet2TestingFormController(ApplicationInstance app)
        {
            m_App = app ?? throw new ArgumentNullException(nameof(app));
            m_IsRunning = false;
        }

        #region INodeSet2TestingFormController Members
        public string NodeSet2Path { get; set; }

        public bool IsNodeSet2PathValid(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                var info = new FileInfo(path);
                return info.Exists
                    && info.Extension == ".xml";
            }
            return false;
        }

        public void StartServer()
        {
            if (m_IsRunning)
            {
                StopServer();
            }
            m_App.Start(new Server(NodeSet2Path)).Wait();
            m_IsRunning = true;
        }

        public void StopServer()
        {
            if (!m_IsRunning) return;
            m_App.Stop();
            m_IsRunning = false;
        }

        public void ConfigureServerDiagnosticsControl(ServerDiagnosticsCtrl serverDiagnosticsCtrl)
        {
            if (m_IsRunning)
            {
                serverDiagnosticsCtrl.Initialize((StandardServer)m_App.Server, m_App.ApplicationConfiguration);
            }
            else
            {
                serverDiagnosticsCtrl.Reset();
            }
        }
        #endregion
    }
}

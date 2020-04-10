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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NodeSet2TestingServer.Resources;
using NodeSet2TestingServer.UI.Controller;
using Opc.Ua.Server.Controls;

namespace NodeSet2TestingServer.Ui.View
{
    public partial class NodeSet2TestingForm : Form
    {
        private readonly INodeSet2TestingFormController m_Controller;
        private readonly IContainer m_Container;
        private readonly HeaderBranding m_HeaderBranding;
        private readonly Label m_NodeSet2Label;
        private readonly TextBox m_NodeSet2TextBox;
        private readonly Button m_NodeSet2BrowseButton;
        private readonly Button m_StartServerButton;
        private readonly Button m_StopServerButton;
        private readonly NotifyIcon m_NotifyIcon;
        private readonly ServerDiagnosticsCtrl m_ServerDiagnosticsCtrl;
        private readonly ErrorProvider m_NodeSet2PathErrorProvider;

        private const int m_Space = 10;
        private const int m_ButtonDifference = 5;
        private static Size m_ButtonSize = new Size(160, 40);
        private const int m_DefaultHeight = 25;

        public NodeSet2TestingForm(INodeSet2TestingFormController controller)
        {
            m_Controller = controller ?? throw new ArgumentNullException(nameof(controller));

            m_Container = new Container();
            m_HeaderBranding = new HeaderBranding();
            m_NodeSet2Label = new Label();
            m_NodeSet2TextBox = new TextBox();
            m_NodeSet2BrowseButton = new Button();
            m_StartServerButton = new Button();
            m_StopServerButton = new Button();
            m_NotifyIcon = new NotifyIcon();
            m_ServerDiagnosticsCtrl = new ServerDiagnosticsCtrl();
            m_NodeSet2PathErrorProvider = new ErrorProvider();

            InitializeComponent(isRefresh: false);
            HandleEvents();
        }

        private void InitializeComponent(bool isRefresh)
        {
            //stop rendingering
            SuspendLayout();

            //general settings
            if (!isRefresh)
            {
                ClientSize = new Size(1500, 1000);
                MinimumSize = new Size(525, 700);
                Text = NodeSet2TestingFormResources.NodeSetTestingFormText;
                Icon = NodeSet2TestingFormResources.Icon;

                // nodeset path error provider
                m_NodeSet2PathErrorProvider.SetIconAlignment(m_NodeSet2TextBox, ErrorIconAlignment.TopRight);
                m_NodeSet2PathErrorProvider.SetIconPadding(m_NodeSet2TextBox, -15);
                m_NodeSet2PathErrorProvider.BlinkStyle = ErrorBlinkStyle.BlinkIfDifferentError;
            }

            // header
            m_HeaderBranding.Location = new Point(0, 24);
            m_HeaderBranding.Dock = DockStyle.Top & DockStyle.Left;

            // nodeset label
            m_NodeSet2Label.Location = new Point(
                m_Space,
                m_HeaderBranding.Location.Y + m_HeaderBranding.Size.Height + m_Space);
            m_NodeSet2Label.Size = new Size(
                ClientSize.Width - (2* m_Space),
                m_DefaultHeight);
            m_NodeSet2Label.Text = NodeSet2TestingFormResources.NodeSet2LabelText;

            // nodeset textbox
            m_NodeSet2TextBox.Location = new Point(
                m_NodeSet2Label.Location.X,
                m_NodeSet2Label.Location.Y + m_NodeSet2Label.Size.Height + m_Space);
            m_NodeSet2TextBox.Size = new Size(
                ClientSize.Width - (3 * m_Space) - m_ButtonSize.Width,
                m_DefaultHeight);

            // nodeset browse button
            m_NodeSet2BrowseButton.Location = new Point(
                m_NodeSet2TextBox.Location.X + m_NodeSet2TextBox.Size.Width + m_Space,
                m_NodeSet2TextBox.Location.Y - m_ButtonDifference);
            m_NodeSet2BrowseButton.Text = NodeSet2TestingFormResources.NodeSet2BrowseButtonText;
            m_NodeSet2BrowseButton.Size = m_ButtonSize;

            // server start button
            m_StartServerButton.Location = new Point(
                m_NodeSet2TextBox.Location.X,
                m_NodeSet2TextBox.Location.Y + m_NodeSet2TextBox.Size.Height + m_Space);
            m_StartServerButton.Size = m_ButtonSize;
            m_StartServerButton.Text = NodeSet2TestingFormResources.StartButtonText;
            m_StartServerButton.Enabled = false;

            // server stop button
            m_StopServerButton.Location = new Point(
                m_StartServerButton.Location.X + m_StartServerButton.Size.Width + m_Space,
                m_StartServerButton.Location.Y);
            m_StopServerButton.Size = m_ButtonSize;
            m_StopServerButton.Text = NodeSet2TestingFormResources.StopButtonText;
            m_StopServerButton.Enabled = false;

            // server diagnostics control
            m_ServerDiagnosticsCtrl.Location = new Point(
                m_StartServerButton.Location.X,
                m_StartServerButton.Location.Y + m_StartServerButton.Size.Height + m_Space);
            m_ServerDiagnosticsCtrl.Size = new Size(
                ClientSize.Width - (2*m_Space),
                ClientSize.Height - m_Space - m_ServerDiagnosticsCtrl.Location.Y);

            if (!isRefresh)
            {
                Controls.Add(m_HeaderBranding);
                m_Container.Add(m_HeaderBranding);
                Controls.Add(m_NodeSet2Label);
                m_Container.Add(m_NodeSet2Label);
                Controls.Add(m_NodeSet2TextBox);
                m_Container.Add(m_NodeSet2TextBox);
                Controls.Add(m_NodeSet2BrowseButton);
                m_Container.Add(m_NodeSet2BrowseButton);
                Controls.Add(m_StartServerButton);
                m_Container.Add(m_StartServerButton);
                Controls.Add(m_StopServerButton);
                m_Container.Add(m_StopServerButton);
                Controls.Add(m_ServerDiagnosticsCtrl);
                m_Container.Add(m_ServerDiagnosticsCtrl);
            }

            // notify icon
            m_NotifyIcon.Icon = NodeSet2TestingFormResources.Icon;

            //start rendering
            ResumeLayout();
        }

        private void HandleEvents()
        {
            Resize += (sender, args) => {
                InitializeComponent(isRefresh: true);
            };

            m_NodeSet2BrowseButton.Click += (sender, args) => {
                BrowseForNodeset2();
            };

            m_NodeSet2TextBox.Validated += (sender, args) => {
                CheckNodeSet2Path();
            };

            m_StartServerButton.Click += (sender, args) => {
                if (m_Controller.IsNodeSet2PathValid(m_Controller.NodeSet2Path))
                {
                    m_Controller.StartServer();
                    m_Controller.ConfigureServerDiagnosticsControl(m_ServerDiagnosticsCtrl);
                    m_StopServerButton.Enabled = true;
                    m_StartServerButton.Enabled = false;
                }
            };

            m_StopServerButton.Click += (sender, args) => {
                m_Controller.StopServer();
                m_Controller.ConfigureServerDiagnosticsControl(m_ServerDiagnosticsCtrl);
                m_StartServerButton.Enabled = true;
                m_StopServerButton.Enabled = false;
            };
        }

        private void BrowseForNodeset2()
        {
            using (var dialog = new OpenFileDialog() {
                Title = NodeSet2TestingFormResources.BrowseNodeSet2OpenFileDialogTitle,
                Filter = "XML-Files (*.xml)|*.xml",
                Multiselect = false
            })
            {
                var result = dialog.ShowDialog();

                if (result == DialogResult.OK && dialog.CheckFileExists)
                {
                    m_NodeSet2TextBox.Text = dialog.FileName;
                    ValidateChildren(ValidationConstraints.Visible);
                }
            }
        }

        private void CheckNodeSet2Path()
        {
            var path = m_NodeSet2TextBox.Text;
            var isValid = m_Controller.IsNodeSet2PathValid(path);
            if (isValid)
            {
                m_NodeSet2PathErrorProvider.SetError(m_NodeSet2TextBox, string.Empty);
                m_Controller.NodeSet2Path = path;
            }
            else
            {
                m_NodeSet2PathErrorProvider.SetError(m_NodeSet2TextBox, NodeSet2TestingFormResources.NodeSet2InvalidPath);
            }
            m_StartServerButton.Enabled = isValid;
        }
    }
}

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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NodeSet2TestingServer.Ui.View;
using NodeSet2TestingServer.UI.Controller;
using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Server.Controls;

namespace NodeSet2TestingServer
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        static async Task Main()
        {
            ApplicationInstance.MessageDlg = new ApplicationMessageDlg();
            var app = new ApplicationInstance() {
                ApplicationName = "Sample Nodeset2 Testing Server",
                ApplicationType = ApplicationType.Server,
                ConfigSectionName = "NodeSet2TestingServer",
            };

            try
            {
                await app.LoadApplicationConfiguration(silent: false);
                bool certValid = await app.CheckApplicationInstanceCertificate(silent: false, minimumKeySize: 0);
                if (!certValid) throw new InvalidProgramException("Application instance certificate invalid!");
                RunInSeparateThread(app);
            }
            catch (Exception e)
            {
                ExceptionDlg.Show(app.ApplicationName, e);
                throw;
            }
        }

        public static void RunInSeparateThread(ApplicationInstance app)
        {
            new Thread(() => {

                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new NodeSet2TestingForm(new NodeSet2TestingFormController(app)));

            }).RunAsSingleThreadApartment();
        }
    }


    public static class ExtensionMethods
    {

        public static void RunAsSingleThreadApartment(this Thread thread)
        {
            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "Operates Windows Message Pump";
            thread.Start();
            thread.Join();
        }
    }
}

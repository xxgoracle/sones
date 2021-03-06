/*
* sones GraphDB - Community Edition - http://www.sones.com
* Copyright (C) 2007-2011 sones GmbH
*
* This file is part of sones GraphDB Community Edition.
*
* sones GraphDB is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published by
* the Free Software Foundation, version 3 of the License.
* 
* sones GraphDB is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU Affero General Public License for more details.
*
* You should have received a copy of the GNU Affero General Public License
* along with sones GraphDB. If not, see <http://www.gnu.org/licenses/>.
* 
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace sones.Library.Threading
{
    /// <summary>
    /// EventWaitHandleFactory class.
    /// This is a static class that creates AutoResetEvent and ManualResetEvent objects.
    /// In WindowCE the WaitForMultipleObjects API fails to use the Handle property 
    /// of XxxResetEvent. It can use only handles that were created by the CreateEvent API.
    /// Consequently this class creates the needed XxxResetEvent and replaces the handle if
    /// it's a WindowsCE OS.
    /// </summary>
    public static class EventWaitHandleFactory
    {
        /// <summary>
        /// Create a new AutoResetEvent object
        /// </summary>
        /// <returns>Return a new AutoResetEvent object</returns>
        public static AutoResetEvent CreateAutoResetEvent()
        {
            AutoResetEvent waitHandle = new AutoResetEvent(false);

#if (WindowsCE)
            ReplaceEventHandle(waitHandle, false, false);
#endif

            return waitHandle;
        }

        /// <summary>
        /// Create a new ManualResetEvent object
        /// </summary>
        /// <returns>Return a new ManualResetEvent object</returns>
        public static ManualResetEvent CreateManualResetEvent(bool initialState)
        {
            ManualResetEvent waitHandle = new ManualResetEvent(initialState);

#if (WindowsCE)
            ReplaceEventHandle(waitHandle, true, initialState);
#endif

            return waitHandle;
        }

#if (WindowsCE)

        /// <summary>
        /// Replace the event handle
        /// </summary>
        /// <param name="waitHandle">The WaitHandle object which its handle needs to be replaced.</param>
        /// <param name="manualReset">Indicates if the event is a ManualResetEvent (true) or an AutoResetEvent (false)</param>
        /// <param name="initialState">The initial state of the event</param>
        private static void ReplaceEventHandle(WaitHandle waitHandle, bool manualReset, bool initialState)
        {
            // Store the old handle 
            IntPtr oldHandle = waitHandle.Handle;

            // Create a new event
            IntPtr newHandle = CreateEvent(IntPtr.Zero, manualReset, initialState, null);

            // Replace the old event with the new event
            waitHandle.Handle = newHandle;

            // Close the old event
            CloseHandle (oldHandle);        
        }

        [DllImport("coredll.dll", SetLastError = true)]
        public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        //Handle
        [DllImport("coredll.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);
#endif

    }
}

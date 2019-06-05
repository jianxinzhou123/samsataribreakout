using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;

namespace PrecisionTimers
{
    public class TimerQueueTimer : IDisposable
    {

        IntPtr phNewTimer; // Handle to the timer.
        Mutex executingMutex;

        #region Win32 TimerQueueTimer Functions

        // possible flags for the CreateTimerQueueTimer function that manipulate
        // the behavior of the callback context
        private enum Flag
        {
            WT_EXECUTEDEFAULT = 0x00000000,
            WT_EXECUTEONLYONCE = 0x00000008,
            WT_EXECUTELONGFUNCTION = 0x00000010,
            WT_EXECUTEINTIMERTHREAD = 0x00000020,
            WT_EXECUTEINPERSISTENTTHREAD = 0x00000080,
            WT_TRANSFER_IMPERSONATION = 0x00000100
        }

        //
        // Win32 CreateTimerQueueTimer 
        //
        [DllImportAttribute("kernel32.dll", SetLastError = true)]
        static extern bool CreateTimerQueueTimer(
            out IntPtr phNewTimer,          // phNewTimer - Pointer to a handle; this is an out value
            IntPtr TimerQueue,              // TimerQueue - Timer queue handle. For the default timer queue, NULL
            WaitOrTimerDelegate Callback,   // Callback - Pointer to the callback function
            IntPtr Parameter,               // Parameter - Value passed to the callback function
            uint DueTime,                   // DueTime - Time (milliseconds), before the timer is set to the signaled state for the first time 
            uint Period,                    // Period - Timer period (milliseconds). If zero, timer is signaled only once
            uint Flags                      // Flags - One or more of the next values (table taken from MSDN):
            );


        // Flags definition

        // WT_EXECUTEDEFAULT                By default, the callback function is queued to a non-I/O worker thread.
        // WT_EXECUTEINTIMERTHREAD 	        The callback function is invoked by the timer thread itself. This flag should 
        //                                  be used only for short tasks or it could affect other timer operations.
        // WT_EXECUTEINPERSISTENTTHREAD 	The callback function is queued to a thread that never terminates. This flag 
        //                                  should be used only for short tasks or it could affect other timer operations.
        //                                  Note that currently no worker thread is persistent, although no worker thread 
        //                                  will terminate if there are any pending I/O requests.
        // WT_EXECUTELONGFUNCTION 	        Specifies that the callback function can perform a long wait. This flag helps 
        //                                  the system to decide if it should create a new thread.
        // WT_EXECUTEONLYONCE 	            The timer will be set to the signaled state only once.
        // WT_TRANSFER_IMPERSONATION        Callback functions will use the current access token, whether it is a 
        //                                  process or impersonation token. If this flag is not specified, callback 
        //                                  functions execute only with the process token.


        //
        // Win32 DeleteTimerQueueTimer 
        //
        [DllImportAttribute("kernel32.dll", SetLastError = true)]
        static extern bool DeleteTimerQueueTimer(
            IntPtr timerQueue,  // TimerQueue - A handle to the (default) timer queue
            IntPtr timer,       // Timer - A handle to the timer
            IntPtr compEvent    // CompletionEvent - A handle to an optional event to be signaled when the function is successful 
            //                     and all callback functions have completed. Can be NULL.
            );

        //
        // Win32 timeBeginPeriod
        //
        [DllImport("winmm.dll")]
        static extern uint timeBeginPeriod(uint uPeriod);

        //
        // Win32 timeEndPeriod
        //
        [DllImport("winmm.dll")]
        static extern uint timeEndPeriod(uint uPeriod);

        #endregion

        #region TimerQueueTimer methods and properties

        public delegate void WaitOrTimerDelegate(IntPtr lpParameter, bool timerOrWaitFired);

        public TimerQueueTimer()
        {
            // initialize our handle. will help keep track of deletions
            phNewTimer = IntPtr.Zero;

            // set precision to 1 ms
            TimeBeginPeriod(1);

            // initialize mutex
            executingMutex = new Mutex(false);
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~TimerQueueTimer()
        {
            TimeEndPeriod(1);
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        public void Create(uint dueTime, uint period, WaitOrTimerDelegate callbackDelegate)
        {
            IntPtr pParameter = IntPtr.Zero;
            int error = 0;

            bool success = CreateTimerQueueTimer(
                // Timer handle
                out phNewTimer,
                // Default timer queue. IntPtr.Zero is just a constant value that represents a null pointer.
                IntPtr.Zero,
                // Timer callback function
                callbackDelegate,
                // Callback function parameter
                pParameter,
                // Time (milliseconds), before the timer is set to the signaled state for the first time.
                dueTime,
                // Period - Timer period (milliseconds). If zero, timer is signaled only once.
                period,
                (uint)Flag.WT_EXECUTEDEFAULT);

            error = Marshal.GetLastWin32Error();

            if (success == false)
                throw new QueueTimerException("Error in Win32 CreateTimerQueueTimer function.", error);
        }

        public void Delete()
        {

            int error = 100;

            // if the handle is 0, no timer has been created to delete
            if (phNewTimer == IntPtr.Zero)
                return;

            try
            {
                bool success = DeleteTimerQueueTimer(
                IntPtr.Zero, // TimerQueue - A handle to the (default) timer queue
                phNewTimer,  // Timer - A handle to the timer
                IntPtr.Zero  // CompletionEvent - A handle to an optional event to be signaled when the function is successful and all callback functions have completed. Can be NULL.
                );
                error = Marshal.GetLastWin32Error();
                if (success == false)
                {
                    // timer deleted, so set our handle to 0
                    phNewTimer = IntPtr.Zero;
                    throw new QueueTimerException("Exception occurred in Win32 DeleteTimerQueueTimer function.", error);
                }
            }
            catch (System.Runtime.InteropServices.SEHException ex)
            {
                // timer deleted, so set our handle to 0
                phNewTimer = IntPtr.Zero;
                Console.WriteLine("Exception occurred in DeleteTimerQueueTimer");
                Console.WriteLine(ex.ToString());
                throw new QueueTimerException("Exception occurred in Win32 DeleteTimerQueueTimer function.", error);
            }

            // timer deleted, so set our handle to 0
            phNewTimer = IntPtr.Zero;

        }

        // timer callback functions should use this function at the beginning
        // of their callback routines.
        public bool ExecutingCallback()
        {
            // we want to return immediately, whether we got the mutex
            // or not. if we got the mutex, true is returned,
            // otherwise false
            return executingMutex.WaitOne(10);
        }

        // timer callback functions should use this function at the end
        // of their callback routines.
        public void DoneExecutingCallback()
        {
            executingMutex.ReleaseMutex();
        }

        public void TimeBeginPeriod(uint uPeriod)
        {
            // call Win32 function to set precision
            timeBeginPeriod(uPeriod);
        }

        public void TimeEndPeriod(uint uPeriod)
        {
            // call Win32 function to unset precision
            // for every Begin, there MUST be an end, per the documentation 
            // of this function
            timeEndPeriod(uPeriod);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {

            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.       
        protected virtual void Dispose(bool disposing)
        {
            // wait until callback is done executing
            executingMutex.WaitOne();
            Console.WriteLine("Dispose got callback");

            if (!disposed)
            {
                if (disposing)
                {
                    // dispose managed resources here
                    // we have none
                }

                try
                {
                    Delete();
                }
                catch (QueueTimerException)
                {
                    // do nothing. we just want to prevent an uncaught exception error
                    // there is nothing we can do anyway
                }
                disposed = true;
            }

            executingMutex.ReleaseMutex();

        }

        #endregion
    }

    public class QueueTimerException : Exception
    {

        private int error;
        public int Error
        {
            get { return error; }
        }

        public QueueTimerException(string message, int data)
            : base(message)
        {
            error = data;
        }

    }
}
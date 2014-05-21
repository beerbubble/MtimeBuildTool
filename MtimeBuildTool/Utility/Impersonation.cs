using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Security.Principal;
using MtimeBuildTool.Helper;

namespace MtimeBuildTool.Utility
{
    public class Impersonation : IDisposable
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);


        private WindowsImpersonationContext _ImpersonationContext;

        // Test harness. 
        // If you incorporate this code into a DLL, be sure to demand FullTrust.
        //[PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public void Impersonate(AccountModel account)
        {
            SafeTokenHandle safeTokenHandle;
            try
            {
                const int LOGON32_PROVIDER_DEFAULT = 0;
                //This parameter causes LogonUser to create a primary token. 
                //const int LOGON32_LOGON_INTERACTIVE = 2;
                const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

                // Call LogonUser to obtain a handle to an access token. 
                bool returnValue = LogonUser(account.UserName, account.Ip, account.Password,
                    LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_DEFAULT,
                    out safeTokenHandle);

                //Console.WriteLine("LogonUser called.");

                if (false == returnValue)
                {
                    int ret = Marshal.GetLastWin32Error();
                    //Console.WriteLine("LogonUser failed with error code : {0}", ret);
                    throw new System.ComponentModel.Win32Exception(ret);
                }
                using (safeTokenHandle)
                {
                    //Console.WriteLine("Did LogonUser Succeed? " + (returnValue ? "Yes" : "No"));
                    //Console.WriteLine("Value of Windows NT token: " + safeTokenHandle);

                    // Check the identity.
                    //Console.WriteLine("Before impersonation: "
                        //+ WindowsIdentity.GetCurrent().Name);
                    // Use the token handle returned by LogonUser. 
                    using (WindowsIdentity newId = new WindowsIdentity(safeTokenHandle.DangerousGetHandle()))
                    {

                        _ImpersonationContext = newId.Impersonate();
                        // Check the identity.
                        //Console.WriteLine("After impersonation: "
                        //    + WindowsIdentity.GetCurrent().Name);
                    }
                    // Releasing the context object stops the impersonation 
                    // Check the identity.
                    //Console.WriteLine("After closing the context: " + WindowsIdentity.GetCurrent().Name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred. " + ex.Message);
            }

        }

        #region IDisposable Members

        ~Impersonation()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _ImpersonationContext != null)
            {
                _ImpersonationContext.Dispose();
                _ImpersonationContext = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }

    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle()
            : base(true)
        {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }
    }
}

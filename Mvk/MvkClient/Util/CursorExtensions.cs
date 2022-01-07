using System;
using System.Runtime.InteropServices;

namespace MvkClient.Util
{
    public static class CursorExtensions
    {
        [StructLayout(LayoutKind.Sequential)]
        struct PointStruct
        {
            public Int32 x;
            public Int32 y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CursorInfoStruct
        {
            /// <summary> The structure size in bytes that must be set via calling Marshal.SizeOf(typeof(CursorInfoStruct)).</summary>
            public Int32 cbSize;
            /// <summary> The cursor state: 0 == hidden, 1 == showing, 2 == suppressed (is supposed to be when finger touch is used, but in practice finger touch results in 0, not 2)</summary>
            public Int32 flags;
            /// <summary> A handle to the cursor. </summary>
            public IntPtr hCursor;
            /// <summary> The cursor screen coordinates.</summary>
            public PointStruct pt;
        }

        /// <summary> Must initialize cbSize</summary>
        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(ref CursorInfoStruct pci);

        [DllImport("user32.dll")]
        static extern int ShowCursor(bool bShow);

        public static bool IsVisible()
        {
            CursorInfoStruct pci = new CursorInfoStruct
            {
                cbSize = Marshal.SizeOf(typeof(CursorInfoStruct))
            };
            GetCursorInfo(ref pci);
            // const Int32 hidden = 0x00;
            const Int32 showing = 0x01;
            // const Int32 suppressed = 0x02;
            bool isVisible = ((pci.flags & showing) != 0);
            return isVisible;
        }

        public static int Show(bool bShow) => ShowCursor(bShow);
    }
}

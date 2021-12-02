using System.Runtime.InteropServices;
using System.Security;

namespace MvkLauncher
{
    public static class WinApi
    {
        /// В документации написано, что timeBeginPeriod следует вызывать непосредственно перед использованием таймера,
        /// а timeEndPeriod - сразу же после него. Иными словами, рекомендуется чтобы таймер как можно меньше времени 
        /// работал при повышенном разрешении (это снижает общую производительность системы). 
        /// А значит вариант "задать период при старте программы, и потом вырубить при выходе" лучше не использовать.

        /// Функция timeGetDevCaps запрашивает устройство таймера для определения его разрешения.

        /// <summary>TimeBeginPeriod(). See the Windows API documentation for details.</summary>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]

        /// <summary>
        /// Функция TimeEndPeriod запрашивает минимальное разрешение для периодических таймеров.
        /// </summary>
        public static extern uint TimeBeginPeriod(uint uMilliseconds);

        /// <summary>Функция TimeEndPeriod очищает ранее установленную минимальную разрешение таймера.</summary>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]

        /// <summary>
        /// Функция TimeEndPeriod очищает ранее установленную минимальную разрешение таймера.
        /// </summary>
        public static extern uint TimeEndPeriod(uint uMilliseconds);
    }
}

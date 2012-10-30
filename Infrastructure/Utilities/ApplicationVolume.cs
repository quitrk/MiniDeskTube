using System;
using System.Runtime.InteropServices;

namespace Infrastructure.Utilities
{
    /// <summary>
    /// Utility for getting and setting the application volume in Windows. 
    /// </summary>
    public static class ApplicationVolume
    {
        /// <summary>
        /// Waves the out get volume.
        /// </summary>
        /// <param name="hwo">The hwo.</param>
        /// <param name="vol">The vol.</param>
        /// <returns></returns>
        [DllImport("winmm.dll")]
        private static extern int waveOutGetVolume(IntPtr hwo, out uint vol);

        /// <summary>
        /// Waves the out set volume.
        /// </summary>
        /// <param name="hwo">The hwo.</param>
        /// <param name="vol">The vol.</param>
        /// <returns></returns>
        [DllImport("winmm.dll")]
        private static extern int waveOutSetVolume(IntPtr hwo, uint vol);

        /// <summary>
        /// Gets the volume.
        /// </summary>
        /// <returns></returns>
        public static int GetVolume()
        {
            uint currentVolume = 0;
            waveOutGetVolume(IntPtr.Zero, out currentVolume);
            var calculatedVolume = (ushort)(currentVolume & 0x0000ffff);

            return calculatedVolume / (ushort.MaxValue / 10) * 10;
        }

        /// <summary>
        /// Sets the volume. 
        /// </summary>
        /// <param name="newValue">The new value (between 0 and 100).</param>
        public static void SetVolume(int newValue)
        {
            var newVolume = ((ushort.MaxValue / 10) * (double)newValue / 10);
            var newVolumeAllChannels = ((uint)newVolume & 0x0000ffff) | ((uint)newVolume << 16);
            waveOutSetVolume(IntPtr.Zero, newVolumeAllChannels);
        }
    }
}

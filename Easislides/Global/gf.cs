//using JRO;
//using Easislides.Model.EasiSlidesDbDataSetTableAdapters;
//using Microsoft.Office.Interop.Access.Dao;
//using NetOffice.PowerPointApi;
using System;
using System.Runtime.InteropServices;

//using NetOffice.DAOApi;

#if SQLite
#elif MariaDB
using DbConnection = MySql.Data.MySqlClient.MySqlConnection;
using DbDataAdapter = MySql.Data.MySqlClient.MySqlDataAdapter;
using DbCommandBuilder = MySql.Data.MySqlClient.MySqlCommandBuilder;
using DbCommand = MySql.Data.MySqlClient.MySqlCommand;
using DbDataReader = MySql.Data.MySqlClient.MySqlDataReader;
using DbTransaction = MySql.Data.MySqlClient.MySqlTransaction;
#endif

namespace Easislides
{
    internal unsafe partial class gf
    {
        // All constants, fields, and methods have been moved to:
        // - gfConstants.cs (constants and static fields)
        // - gfUtility.cs (utility methods)
        // - gfDatabase.cs (database methods)
        // - gfBible.cs (bible methods)
        // - gfDisplay.cs (display/UI methods)
        // - gfMedia.cs (media/music methods)
        // - gfPowerPoint.cs (PowerPoint methods)
        // - gfFileIO.cs (file I/O methods)
        // - gfImages.cs (image methods)
        // - gfLyrics.cs (lyrics/notation methods)
        // - gfFolder.cs (folder methods)
        // - gfConfig.cs (configuration methods)
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;
        public uint wFunc;
        public string pFrom;
        public string pTo;
        public ushort fFlags;
        public bool fAnyOperationsAborted;
        public IntPtr hNameMappings;
        public string lpszProgressTitle;
    }
}

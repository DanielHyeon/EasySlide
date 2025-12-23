using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Easislides.Module
{
    public enum SettingsCategory
    {
        RotateString = 10
    }

    public enum MediaBackgroundStyle
    {
        None,
        Audio,
        Video,
        SameAsPrevious
    }

    public enum PlayState
    {
        Stopped,
        Paused,
        Running,
        Init
    }


    public enum DatabaseType
    {
        Songs,
        Usages,
        Bible
    }

    public enum VAlign
    {
        Top,
        Centre,
        Bottom
    }

    public enum ImageMode
    {
        Tile,
        Centre,
        BestFit
    }

    public enum UsageMode
    {
        Worship,
        Collection,
        PraiseBook
    }

    public enum PraiseBookLayout
    {
        WorshipList,
        PraiseBook
    }

    public enum GapMedia
    {
        None,
        SameAsPrevious,
        SessionMedia
    }

    public enum HeadingFormat
    {
        AsRegion1,
        AsRegion1Plus,
        Individual
    }

    public enum GapType
    {
        None,
        Black,
        Default,
        User
    }

    public enum AlertType
    {
        Parental,
        Message
    }

    public enum SortBy
    {
        Alpha,
        WordCount,
        SongNumber
    }

    public enum InfoType
    {
        NoAction,
        WorshipList,
        PreviewItem,
        OutputItemSlide1,
        OutputItemLastSlide,
        FormatStringUpdate,
        Save
    }

    public enum ItemSource
    {
        SongsList = 0,
        ExternalFileInfoScreen = 2,
        ExternalFileMedia = 3,
        ExternalFilePowerpoint = 4,
        HolyBible = 5,
        WorshipList = 6,
        PraiseBook = 7,
        TextFile = 8
    }

    public enum KeyDirection
    {
        FirstOne,
        PrevOne,
        NextOne,
        LastOne,
        SpaceOne,
        Refresh
    }
    public enum MPCType
    {
        Session,
        Individual
    }
}

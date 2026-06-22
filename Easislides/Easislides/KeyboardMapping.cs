using System.Windows.Forms;

namespace Easislides
{
    /// <summary>
    /// 키보드 매핑 및 상수 정의
    /// </summary>
    public static class KeyboardMapping
    {
        /// <summary>
        /// 찬양 구절 타입 상수
        /// </summary>
        public static class VerseTypes
        {
            public const int Bridge = 100;
            public const int Ending = 101;
            public const int Tag = 102;
            public const int Worship = 103;
            public const int PreChorus = 111;
            public const int PreChorus2 = 112;
        }

        /// <summary>
        /// NumPad 키를 일반 숫자 키로 정규화
        /// </summary>
        public static Keys NormalizeNumericKey(Keys key)
        {
            return key switch
            {
                Keys.NumPad0 => Keys.D0,
                Keys.NumPad1 => Keys.D1,
                Keys.NumPad2 => Keys.D2,
                Keys.NumPad3 => Keys.D3,
                Keys.NumPad4 => Keys.D4,
                Keys.NumPad5 => Keys.D5,
                Keys.NumPad6 => Keys.D6,
                Keys.NumPad7 => Keys.D7,
                Keys.NumPad8 => Keys.D8,
                Keys.NumPad9 => Keys.D9,
                _ => key
            };
        }

        /// <summary>
        /// 숫자 키인지 확인
        /// </summary>
        public static bool IsNumericKey(Keys key)
        {
            return key >= Keys.D0 && key <= Keys.D9 ||
                   key >= Keys.NumPad0 && key <= Keys.NumPad9;
        }

        /// <summary>
        /// 네비게이션 키인지 확인
        /// </summary>
        public static bool IsNavigationKey(Keys key)
        {
            return key is Keys.Up or Keys.Down or Keys.Left or Keys.Right or
                   Keys.Home or Keys.End or Keys.Prior or Keys.Next or
                   Keys.Space or Keys.Tab;
        }

        /// <summary>
        /// 찬양 구절 점프 키인지 확인
        /// </summary>
        public static bool IsVerseJumpKey(Keys key, bool shiftKey)
        {
            return key switch
            {
                Keys.B => true,  // Bridge
                Keys.W when shiftKey => true,  // Worship (Shift+W)
                Keys.P => true,  // PreChorus
                Keys.Q when shiftKey => true,  // PreChorus2 (Shift+Q)
                Keys.E => true,  // Ending
                Keys.T when shiftKey => true,  // Tag (Shift+T)
                _ => false
            };
        }

        /// <summary>
        /// 기능 키인지 확인
        /// </summary>
        public static bool IsFunctionKey(Keys key)
        {
            return key is Keys.G or Keys.Z or Keys.A or Keys.J or Keys.M or Keys.C;
        }
    }
}

using System;

namespace Library.Unmanaged
{
    public class Conversion
    {
        unsafe public static float ToFloat(int inum)
        {
            float fnum = *(float*)&inum;
            return fnum;
        }

        unsafe public static int ToInt(float fnum)
        {
            int inum = *(int*)&fnum;
            return inum;
        }
    }
}

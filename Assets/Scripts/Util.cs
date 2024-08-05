using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Util
{
    public static string FormatTimer(float time)
    {
        // MM:SS:MS
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        int milliseconds = (int)(time * 1000) % 1000;
        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }


}


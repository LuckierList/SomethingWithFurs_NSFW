using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetGif : MonoBehaviour
{
    public AnimatedGifDrawer gif;

    public string small = @"D:\HardDrive\No pls\e621\Te lo advierto\Videos\Dickgirl\107482-2b25cd56e41c3a1feda62c1b63669e10.gif";
    public string med = @"D:\HardDrive\No pls\e621\Te lo advierto\Videos\Dickgirl\527173-bfb0484aac08ebae8d3886ac8381b3b7.gif";
    public string large = @"D:\HardDrive\No pls\e621\Te lo advierto\Videos\Dickgirl\637013-fa647a940f2ada0295c569af845ecc1e.gif";


    public void GetGifz(string size)
    {
        switch (size)
        {
            case "s":
                gif.loadingGifPath = small;
                break;
            case "m":
                gif.loadingGifPath = med;
                break;
            case "l":
                gif.loadingGifPath = large;
                break;
        }
        gif.DrawGif();
    }
}

// Name:
// Submenu:
// Author: mechaskrom
// Title: DiffLayers
// Version:
// Desc: Differences in layers
// Keywords: differences|layer
// URL:
// Help:

// For help writing a Bitmap plugin: https://boltbait.com/pdn/CodeLab/help/tutorial/bitmap/

protected override void OnRender(IBitmapEffectOutput output)
{
    //OnRender may be called multiple times with different output bounds because
    //the area to draw is tiled and called multithreaded. So it's important to
    //keep drawing inside the output bounds.

    RectInt32 outputBounds = output.Bounds;
    using IBitmapLock<ColorBgra32> outputLock = output.LockBgra32();
        var outputRegion = outputLock.AsRegionPtr().OffsetView(-outputBounds.Location);

    //Output color to use for different pixels.
    ColorBgra32 ColorOutDiff = ColorBgra32.FromBgra(255, 0, 255, 192); //Magenta.

    IReadOnlyList<IBitmapEffectLayerInfo> layers = Environment.Document.Layers;

    //Find the first visible layer (what we will compare with).
    int cmpLayerIndex = layers.FirstIndexWhere((l)=>l.Visible);
    if(cmpLayerIndex < 0 || cmpLayerIndex >= Environment.SourceLayerIndex) return; //No valid layer!

    IEffectInputBitmap<ColorBgra32> cmpLayerBitmap = layers.ElementAt(cmpLayerIndex).GetBitmapBgra32();
    using IBitmapLock<ColorBgra32> cmpLayerLock = cmpLayerBitmap.Lock(cmpLayerBitmap.Bounds());
        RegionPtr<ColorBgra32> cmpLayerRegion = cmpLayerLock.AsRegionPtr();

    //Do all layers below selected layer (this is where the output is drawn).
    for (int i = cmpLayerIndex + 1; i < Environment.SourceLayerIndex; i++)
    {
        IBitmapEffectLayerInfo layer = layers.ElementAt(i);

        //Ignore invisible layers.
        if (!layer.Visible) continue;

        IEffectInputBitmap<ColorBgra32> layerBitmap = layer.GetBitmapBgra32();
        using IBitmapLock<ColorBgra32> layerLock = layerBitmap.Lock(layerBitmap.Bounds());
            RegionPtr<ColorBgra32> layerRegion = layerLock.AsRegionPtr();

        for (int y = outputBounds.Top; y < outputBounds.Bottom; y++)
        {
            if (IsCancelRequested) return;

            for (int  x = outputBounds.Left; x < outputBounds.Right; x++)
            {
                if(cmpLayerRegion[x,y] != layerRegion[x,y]) //Different?
                {
                    // Save your pixel to the output canvas
                    outputRegion[x,y] = ColorOutDiff;
                }
            }
        }
    }
}

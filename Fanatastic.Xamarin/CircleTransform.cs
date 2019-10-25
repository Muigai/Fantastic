using System;
using Android.Graphics;
using Android.Runtime;
using Square.Picasso;

namespace Fantastic.Xamarin
{

    public sealed class CircleTransform : Java.Lang.Object, ITransformation
    {

        Bitmap ITransformation.Transform(Bitmap source)
        {
            int size = Math.Min(source.Width, source.Height);

            int x = (source.Width - size) / 2;
            int y = (source.Height - size) / 2;

            using (var squaredBitmap = Bitmap.CreateBitmap(source, x, y, size, size))
            {
                if (squaredBitmap != source)
                {
                    source.Recycle();
                }

                Bitmap bitmap = Bitmap.CreateBitmap(size, size, source.GetConfig());

                using (var canvas = new Canvas(bitmap))
                using (var paint = new Paint())
                using (var shader = new BitmapShader(squaredBitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp))
                {
                    paint.SetShader(shader);
                    paint.AntiAlias = true;

                    float r = size / 2f;
                    canvas.DrawCircle(r, r, r, paint);

                    squaredBitmap.Recycle();
                    return bitmap;
                }
            }
        }


        public string Key => "circle";

    }
}
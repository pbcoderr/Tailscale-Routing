using System;
using System.IO;
using System.Drawing.Imaging;
using TailscaleRouting;
class GenerateIcons
{
    static void Main(string[] args)
    {
        foreach(bool active in new[]{false,true})
        {
            string name=active?"active":"inactive";
            using(var bitmap=BrandIcons.Draw(256,active)) bitmap.Save(Path.Combine(args[1],name+".png"),ImageFormat.Png);
            int[] sizes={16,20,24,32,40,48,64,128,256}; var images=new byte[sizes.Length][];
            for(int i=0;i<sizes.Length;i++) using(var bitmap=BrandIcons.Draw(sizes[i],active)) using(var stream=new MemoryStream()) {bitmap.Save(stream,ImageFormat.Png);images[i]=stream.ToArray();}
            using(var writer=new BinaryWriter(File.Create(Path.Combine(args[0],active?"TailscaleRouting-active.ico":"TailscaleRouting.ico"))))
            {
                writer.Write((ushort)0);writer.Write((ushort)1);writer.Write((ushort)sizes.Length); int offset=6+16*sizes.Length;
                for(int i=0;i<sizes.Length;i++) {writer.Write((byte)(sizes[i]%256));writer.Write((byte)(sizes[i]%256));writer.Write((byte)0);writer.Write((byte)0);writer.Write((ushort)1);writer.Write((ushort)32);writer.Write(images[i].Length);writer.Write(offset);offset+=images[i].Length;}
                foreach(var bytes in images) writer.Write(bytes);
            }
        }
    }
}

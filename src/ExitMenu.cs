using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

static class ExitMenu {
 static OcrResult Read(Bitmap frame,Rectangle area){
  var engine=OcrEngine.TryCreateFromLanguage(new Language("zh-Hans"));
  if(engine==null)throw new InvalidOperationException("请安装 Windows 简体中文 OCR 语言包，以识别游戏菜单。");
  byte[] ink=new byte[area.Width*area.Height];
  using(var crop=frame.Clone(area,PixelFormat.Format32bppArgb)){
   var data=crop.LockBits(new Rectangle(Point.Empty,crop.Size),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
   try{
    byte[] row=new byte[crop.Width*4];
    for(int y=0;y<crop.Height;y++){
     System.Runtime.InteropServices.Marshal.Copy(IntPtr.Add(data.Scan0,y*data.Stride),row,0,row.Length);
     for(int x=0;x<crop.Width;x++){
      int i=x*4;
      ink[y*crop.Width+x]=(byte)(row[i+2]>145 && row[i+1]>100 && row[i]<row[i+1]*0.7 ? 0 : 255);
     }
    }
   }finally{crop.UnlockBits(data);}
  }
  using(var image=SoftwareBitmap.CreateCopyFromBuffer(ink.AsBuffer(),BitmapPixelFormat.Gray8,area.Width,area.Height))
   return engine.RecognizeAsync(image).AsTask().GetAwaiter().GetResult();
 }
 static string Compact(string text){return string.Concat(text.Where(c=>!char.IsWhiteSpace(c)));}
 public static bool IsBlocked(Bitmap frame){
  var area=new Rectangle(frame.Width/4,frame.Height/3,frame.Width/2,frame.Height/3);
  string text=Compact(Read(frame,area).Text);
  return text.Contains("阵营人口不平衡")&&text.Contains("无法在此规则集下选择");
 }
 public static Point? FindExit(Bitmap frame, Patch agreementExit){
  var location=agreementExit.Find(new Mask(frame),0.84,0,48);
  if(location.HasValue){
   var center=new Point(location.Value.X+agreementExit.W/2,location.Value.Y+agreementExit.H/2);
   if(HasRedSides(frame,center.X-52,center.X+52,center.Y))return center;
  }
  var result=Read(frame,new Rectangle(Point.Empty,frame.Size));
  Point? found=null;
  foreach(var line in result.Lines){
   if(Compact(line.Text)!="退出游戏")continue;
   var words=line.Words;
   int left=(int)words.Min(w=>w.BoundingRect.Left),right=(int)words.Max(w=>w.BoundingRect.Right);
   int y=(int)((words.Min(w=>w.BoundingRect.Top)+words.Max(w=>w.BoundingRect.Bottom))/2);
   if(left<6||right+6>=frame.Width)continue;
   if(!HasRedSides(frame,left-5,right+5,y))continue;
   if(found.HasValue)return null;
   found=new Point((left+right)/2,y);
  }
  return found;
 }
 static bool HasRedSides(Bitmap frame,int left,int right,int y){
  return new[]{left,right}.All(x=>{var c=frame.GetPixel(x,y);return c.R>60&&c.R>2*c.G&&c.R>2*c.B;});
 }
}

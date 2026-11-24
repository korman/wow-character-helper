using System;
using System.Drawing;

static class PvpBadge {
 // Search the second card on the normalized client capture. Each scale couples
 // the icon radius to the size and vertical offset of the short gold heading.
 public static Point? Find(Bitmap frame, Mask gold) {
  int best=0; Point? hit=null;
  for(int radius=28;radius<=54;radius+=4){
   for(int y=frame.Height*30/100;y<frame.Height*48/100;y+=4){
    int top=y-2*radius, height=radius/2;
    for(int x=frame.Width*39/100;x<frame.Width*49/100;x+=4){
     int half=radius*2/3;
     int strokes=gold.Count(x-half,top,2*half,height);
     if(strokes<radius || gold.Count(x-2*radius,top,4*radius,height)>strokes+8)continue;
     int red=0,blue=0;
     for(int dy=-2;dy<=2;dy++)for(int dx=1;dx<=4;dx++){
      Color left=frame.GetPixel(x-dx*radius/6,y+dy*radius/5);
      Color right=frame.GetPixel(x+dx*radius/6,y+dy*radius/5);
      if(left.R>110 && left.R>left.G*1.6 && left.R>left.B*2)red++;
      if(right.B>85 && right.G>right.R*1.15 && right.B>right.R*1.25)blue++;
     }
     int score=red+blue;
     if(red>=6 && blue>=4 && score>best){best=score;hit=new Point(x,y);}
    }
   }
  }
  return hit;
 }
}

using System;
using System.Drawing;

static class RaceScreen {
 // All coordinates refer to the normalized 2560 x 1440 client capture.
 public static bool PortraitSelected(Bitmap frame, Point center) {
  return BrightEdge(frame,center,-1) && BrightEdge(frame,center,1);
 }

 static bool BrightEdge(Bitmap frame, Point center, int side) {
  for(int x=center.X+side*51-15;x<=center.X+side*51+15;x++) {
   int hits=0;
   for(int y=center.Y-28;y<=center.Y+28;y+=4) {
    Color c=frame.GetPixel(x,y);
    if(c.R>=210 && c.G>=150 && c.R>c.B+25)hits++;
   }
   if(hits>=12)return true;
  }
  return false;
 }

 static bool RedButton(Bitmap frame,int x,int y) {
  Color c=frame.GetPixel(x,y);
  return c.R>=70 && c.R>2*c.G && c.R>2*c.B;
 }

 public static bool IsRacePage(Bitmap frame) {
  if(!RedButton(frame,100,1370)||!RedButton(frame,2250,1370))return false;
  foreach(int x in new[]{104,329})foreach(int y in new[]{190,307,423,540})
   if(PortraitSelected(frame,new Point(x,y)))return true;
  return false;
 }
}

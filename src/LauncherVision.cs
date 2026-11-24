using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

// Launcher controls move between layouts. Locate their pixels before every click.
static class LauncherVision {
 static void Find(Bitmap image,Mask mask,string name,Rectangle area,ref double best,ref Rectangle found,Action check){
  using(var original=new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"templates",name+".png")))
  foreach(double scale in new[]{1.0,0.8,2.0/3,1.25,1.5,2.0}){
   using(var resized=new Bitmap(Math.Max(1,(int)Math.Round(original.Width*scale)),Math.Max(1,(int)Math.Round(original.Height*scale)),PixelFormat.Format32bppArgb)){
    using(var g=Graphics.FromImage(resized))g.DrawImage(original,new Rectangle(Point.Empty,resized.Size));
    var template=new Mask(resized);var points=new List<Point>();for(int y=0;y<template.H;y++)for(int x=0;x<template.W;x++)if(template.V[y*template.W+x]!=0)points.Add(new Point(x,y));
    if(points.Count<15)continue;var bounds=Rectangle.Intersect(area,new Rectangle(0,0,image.Width,image.Height));
    for(int y=bounds.Top;y<=bounds.Bottom-template.H;y++){if(check!=null&&(y-bounds.Top)%64==0)check();for(int x=bounds.Left;x<=bounds.Right-template.W;x++){
     int count=mask.Count(x,y,template.W,template.H);if(count<points.Count*.75||count>points.Count*1.3)continue;
     int sample=0;for(int i=0;i<16;i++){var p=points[i*(points.Count-1)/15];sample+=mask.V[(y+p.Y)*mask.W+x+p.X];}if(sample<12)continue;
     int hit=0;foreach(var p in points)hit+=mask.V[(y+p.Y)*mask.W+x+p.X];double score=2.0*hit/(count+points.Count);
     if(score>best){var candidate=new Rectangle(x,y,template.W,template.H);if(IsEnabled(image,candidate)){best=score;found=candidate;}}
    }}
   }
  }
 }
 static bool IsEnabled(Bitmap image,Rectangle play){
  var blue=image.GetPixel(Math.Max(0,play.X-4),play.Y+play.Height/2);
  return blue.B>=140&&blue.R<=80&&blue.G>=60;
 }
 public static Rectangle Play(Bitmap image,Action check=null){
  var area=new Rectangle(0,image.Height/3,image.Width,image.Height-image.Height/3);
  var mask=new Mask(image);double best=0.86;var play=Rectangle.Empty;
  Find(image,mask,"launcherPlay",area,ref best,ref play,check);
  Find(image,mask,"launcherPlayOld",area,ref best,ref play,check);
  if(play.IsEmpty)throw new InvalidOperationException("未识别到可用的进入游戏按钮。请在战网手动选中无限服，检查更新、登录或弹窗后重试。");
  return play;
 }
}

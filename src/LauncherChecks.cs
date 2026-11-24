using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

static class LauncherChecks {
 public static int Snapshots(string root){var lines=new List<string>();try{foreach(var name in new[]{"launcher-old-infinite","launcher-new-infinite"})using(var b=new Bitmap(Path.Combine(root,name+".png"))){var play=LauncherVision.Play(b);lines.Add("PASS: "+name+" button="+play);}File.WriteAllLines(Path.Combine(root,"launcher-snapshots-verification.txt"),lines);return 0;}catch(Exception ex){lines.Add(ex.ToString());File.WriteAllLines(Path.Combine(root,"launcher-snapshots-verification.txt"),lines);return 1;}}
 static void Draw(Graphics g,string name,int x,int y){using(var b=new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"templates",name+".png")))g.DrawImage(b,new Rectangle(x,y,b.Width,b.Height),0,0,b.Width,b.Height,GraphicsUnit.Pixel);}
 static void Reject(Bitmap b,string scenario){try{LauncherVision.Play(b);}catch(InvalidOperationException){return;}throw new Exception("Unexpected button in "+scenario);}
 public static int Run(string root){
  Directory.CreateDirectory(root);var lines=new List<string>();
  try{
   foreach(bool modern in new[]{false,true})using(var b=new Bitmap(1600,1050)){
    int x=modern?1000:80;
    using(var g=Graphics.FromImage(b)){g.Clear(Color.FromArgb(30,33,40));
     g.FillRectangle(Brushes.DodgerBlue,x-10,780,340,90);
     g.DrawString("TestAccount (CN)",SystemFonts.DefaultFont,Brushes.White,x,750);
     Draw(g,modern?"launcherPlay":"launcherPlayOld",x+50,800);
    }
    b.Save(Path.Combine(root,modern?"modern.png":"classic.png"));var play=LauncherVision.Play(b);
    var click=new Point(play.Left+play.Width/2,play.Top+play.Height/2);
    if(!new Rectangle(x+75,810,80,25).Contains(click))throw new Exception("Play click outside the button label: "+play);
    lines.Add("PASS: "+(modern?"modern":"classic")+" button located without product or account templates");
    for(int y=780;y<870;y++)for(int px=x-10;px<x+330;px++){
     var c=b.GetPixel(px,y);if(c.B>=140&&c.R<=80&&c.G>=60)b.SetPixel(px,y,Color.DimGray);
    }
    Reject(b,"disabled button");
   }
   using(var empty=new Bitmap(1600,1000))Reject(empty,"empty page");
   lines.Add("PASS: disabled button and empty page rejected");
   lines.Add("PASS");File.WriteAllLines(Path.Combine(root,"launcher-verification.txt"),lines);return 0;
  }catch(Exception ex){lines.Add(ex.ToString());File.WriteAllLines(Path.Combine(root,"launcher-verification.txt"),lines);return 1;}
 }
}

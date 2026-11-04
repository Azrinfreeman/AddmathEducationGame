"""Owned scenery and navigation art. Run with the already available Pillow package.

These drawing functions are the editable originals; PNG files are their Unity imports.
Coordinates use a 512 x 384 illustration board, rendered at twice its size for smooth edges.
No external images or fonts are used.
"""
from pathlib import Path
from PIL import Image, ImageDraw
import math

ROOT = Path(__file__).resolve().parent
INK = '#203A47'
SCALE = 2


def board():
    image = Image.new('RGBA', (1024, 768))
    return image, ImageDraw.Draw(image)


def polygon(draw, points, color, width=7):
    points = [(int(x*SCALE), int(y*SCALE)) for x, y in points]
    draw.polygon(points, fill=color)
    if width:
        draw.line(points + [points[0]], fill=INK, width=width*SCALE, joint='curve')


def ellipse(draw, bounds, color, width=7):
    draw.ellipse(tuple(int(v*SCALE) for v in bounds), fill=color, outline=INK if width else None, width=width*SCALE)


def line(draw, points, color=INK, width=7):
    draw.line([(int(x*SCALE), int(y*SCALE)) for x, y in points], fill=color, width=width*SCALE, joint='curve')


def rounded(draw, bounds, color, radius=16, width=7):
    draw.rounded_rectangle(tuple(int(v*SCALE) for v in bounds), radius=radius*SCALE, fill=color,
                           outline=INK if width else None, width=width*SCALE)


def star(draw, cx, cy, radius, color):
    points = []
    for i in range(10):
        a = -math.pi/2 + i*math.pi/5
        r = radius if i % 2 == 0 else radius*.46
        points.append((cx+math.cos(a)*r, cy+math.sin(a)*r))
    polygon(draw, points, color, 5)


def save(name, paint):
    image, draw = board()
    paint(draw)
    image.resize((512, 384), Image.Resampling.LANCZOS).save(ROOT / (name + '.png'))


def cloud(d):
    # One union silhouette, with a quiet flat base.
    ellipse(d, (70, 158, 230, 304), 'white', 0)
    ellipse(d, (159, 80, 342, 304), 'white', 0)
    ellipse(d, (285, 140, 447, 305), 'white', 0)
    rounded(d, (104, 222, 409, 310), 'white', 35, 0)


def hill(d):
    polygon(d, [(0,384),(0,287),(32,248),(85,214),(148,199),(212,210),
                (275,241),(340,256),(401,239),(467,247),(512,272),(512,384)], 'white', 0)


def sun(d):
    for i in range(12):
        a = i*math.pi/6
        line(d, [(256+math.cos(a)*130,190+math.sin(a)*130),
                 (256+math.cos(a)*154,190+math.sin(a)*154)], '#D89A3F', 8)
    ellipse(d, (154,88,358,292), '#FFDD79')
    ellipse(d, (208,159,223,180), INK, 0); ellipse(d,(290,159,305,180),INK,0)
    d.arc((222*SCALE,173*SCALE,291*SCALE,230*SCALE), 0, 180, fill=INK, width=6*SCALE)
    ellipse(d, (185,187,216,204), '#F4AD84', 0); ellipse(d,(299,187,330,204),'#F4AD84',0)


def garden(d):
    line(d,[(257,348),(253,182)],'#3D886C',10)
    ellipse(d,(175,225,257,267),'#7CC793',5); ellipse(d,(254,260,340,302),'#7CC793',5)
    for x,y in [(224,140),(281,140),(204,184),(300,184),(253,211)]:
        ellipse(d,(x-29,y-29,x+29,y+29),'#FFC477',5)
    ellipse(d,(222,155,282,215),'#FFE7A4',5)
    polygon(d,[(83,371),(109,305),(125,352),(166,291),(172,360),(213,320),(225,376)],'#68AF8B',0)


def book(d):
    rounded(d,(65,257,449,301),'#65B8B3',10)
    rounded(d,(93,294,418,329),'#FFC477',10)
    polygon(d,[(70,84),(149,62),(256,106),(365,62),(443,84),(443,259),(363,240),(256,277),(150,240),(70,260)],'#FFF8E7')
    polygon(d,[(256,106),(363,62),(443,84),(443,259),(363,240),(256,277)],'#FFF1CC')
    line(d,[(256,106),(256,271)])
    for y in [133,163,193]:
        line(d,[(106,y),(145,y-8),(211,y+15)],'#82BEB9',5)
        line(d,[(294,y+15),(360,y-8),(406,y)],'#D1A75F',5)
    star(d,408,53,29,'#FFDD79')


def games(d):
    polygon(d,[(86,226),(178,195),(258,224),(166,259)],'#F9D584')
    polygon(d,[(86,226),(166,259),(166,339),(86,305)],'#EDAC67')
    polygon(d,[(166,259),(258,224),(258,303),(166,339)],'#F4C476')
    polygon(d,[(268,151),(354,119),(432,151),(346,187)],'#A8D9C0')
    polygon(d,[(268,151),(346,187),(346,291),(268,256)],'#67B49D')
    polygon(d,[(346,187),(432,151),(432,256),(346,291)],'#88CBB1')
    polygon(d,[(263,111),(350,29),(438,111)],'#F49B80')
    ellipse(d,(167,77,270,180),'#8CC7E4')
    line(d,[(104,262),(143,278)],'#FFF8E7',8)
    line(d,[(124,251),(124,291)],'#FFF8E7',8)
    star(d,390,218,24,'#FFF8E7')


def quiz(d):
    rounded(d,(95,70,412,328),'#8CC7E4',28)
    rounded(d,(113,89,394,310),'#FFF8E7',17,5)
    polygon(d,[(148,221),(198,130),(248,221)],'#F4AB86',5)
    ellipse(d,(274,131,354,211),'#9AD0B1',5)
    rounded(d,(172,244,230,284),'#F9D584',5,5)
    line(d,[(276,262),(292,280),(333,238)],'#3A8974',9)
    star(d,418,80,32,'#FFDD79')


def badge(d):
    polygon(d,[(189,219),(165,345),(232,314),(261,348),(280,236)],'#F09C7E')
    polygon(d,[(263,232),(287,347),(325,313),(369,333),(329,214)],'#8CC7E4')
    ellipse(d,(131,41,389,295),'#F4C56B')
    ellipse(d,(158,69,362,269),'#FFE6A3',5)
    star(d,260,164,76,'#F6BD55')
    star(d,93,118,24,'#9AD0B1'); star(d,417,228,22,'#F4AB86')


if __name__ == '__main__':
    for name, paint in [('Cloud',cloud),('Hill',hill),('Sun',sun),('Garden',garden),
                        ('BookGarden',book),('ToyWorkshop',games),('ShapePavilion',quiz),('StickerTree',badge)]:
        save(name,paint)
    print('Exported eight owned illustrations to', ROOT)

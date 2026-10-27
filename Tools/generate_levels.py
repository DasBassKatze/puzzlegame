import math, random, sys
from PIL import Image, ImageDraw, ImageFilter

W, H = 1024, 1024
OUT = sys.argv[1]


def grad(top, bottom, w=W, h=H):
    img = Image.new('RGB', (w, h))
    d = ImageDraw.Draw(img)
    for y in range(h):
        t = y / (h - 1)
        c = tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(3))
        d.line([(0, y), (w, y)], fill=c)
    return img


def ridge(d, base, amp, color, seed, step=6):
    rnd = random.Random(seed)
    phases = [rnd.uniform(0, 6.28) for _ in range(4)]
    pts = [(0, H)]
    for x in range(0, W + step, step):
        y = base
        for k, ph in enumerate(phases):
            y -= amp / (k + 1) * math.sin(x / (180 / (k + 1)) + ph)
        pts.append((x, y))
    pts.append((W, H))
    d.polygon(pts, fill=color)


def mountains():
    img = grad((255, 170, 110), (120, 80, 160))
    d = ImageDraw.Draw(img)
    d.ellipse([760, 170, 960, 370], fill=(255, 230, 150))
    ridge(d, 520, 140, (110, 70, 140), 1)
    ridge(d, 620, 110, (80, 50, 110), 2)
    ridge(d, 730, 80, (50, 35, 80), 3)
    ridge(d, 820, 40, (30, 22, 50), 4)
    return img


def sea():
    img = grad((120, 190, 240), (230, 245, 255), H=500) if False else grad((110, 180, 235), (220, 240, 255))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 470, W, H], fill=(30, 110, 170))
    for i in range(80):
        y = 480 + i * 7
        shade = int(30 + i * 1.2)
        for x in range(0, W, 40):
            off = (i * 13) % 40
            d.arc([x - off, y, x - off + 40, y + 10], 200, 340, fill=(shade + 120, 200, 235), width=2)
    d.ellipse([140, 120, 300, 280], fill=(255, 245, 190))
    # boat
    d.polygon([(700, 520), (900, 520), (860, 570), (740, 570)], fill=(120, 60, 40))
    d.line([(800, 520), (800, 360)], fill=(80, 50, 30), width=6)
    d.polygon([(805, 370), (805, 510), (900, 510)], fill=(250, 250, 245))
    d.polygon([(795, 380), (795, 510), (720, 510)], fill=(235, 90, 70))
    for cx, cy in [(450, 200), (800, 140), (620, 90)]:
        for dx in (0, 40, 80):
            d.ellipse([cx + dx, cy, cx + dx + 90, cy + 50], fill=(255, 255, 255))
    return img


def city():
    img = grad((10, 15, 45), (60, 40, 100))
    d = ImageDraw.Draw(img)
    rnd = random.Random(7)
    for _ in range(160):
        x, y = rnd.randint(0, W), rnd.randint(0, 450)
        r = rnd.choice([1, 1, 2])
        d.ellipse([x, y, x + r, y + r], fill=(255, 255, 230))
    d.ellipse([900, 80, 1020, 200], fill=(240, 240, 220))
    d.ellipse([930, 70, 1040, 180], fill=(14, 19, 50))
    x = 0
    while x < W:
        bw = rnd.randint(60, 140)
        bh = rnd.randint(200, 560)
        col = rnd.choice([(25, 25, 55), (35, 30, 70), (20, 30, 60)])
        d.rectangle([x, H - bh, x + bw, H], fill=col)
        for wy in range(H - bh + 20, H - 20, 30):
            for wx in range(x + 10, x + bw - 15, 22):
                if rnd.random() < 0.45:
                    d.rectangle([wx, wy, wx + 10, wy + 14], fill=rnd.choice([(255, 210, 110), (255, 240, 170), (150, 200, 255)]))
        x += bw + rnd.randint(4, 14)
    return img


def flowers():
    img = grad((150, 210, 255), (200, 240, 200))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 560, W, H], fill=(70, 150, 70))
    rnd = random.Random(3)
    for _ in range(40):
        x = rnd.randint(20, W - 20)
        y = rnd.randint(520, H - 20)
        d.line([(x, y), (x + rnd.randint(-10, 10), y + 120)], fill=(40, 110, 40), width=5)
    for _ in range(55):
        x = rnd.randint(30, W - 30)
        y = rnd.randint(380, H - 40)
        r = rnd.randint(22, 46)
        col = rnd.choice([(240, 80, 110), (255, 200, 60), (180, 90, 220), (255, 130, 60), (250, 250, 250)])
        for k in range(6):
            a = k * math.pi / 3
            px, py = x + math.cos(a) * r * 0.7, y + math.sin(a) * r * 0.7
            d.ellipse([px - r * 0.55, py - r * 0.55, px + r * 0.55, py + r * 0.55], fill=col)
        d.ellipse([x - r * 0.35, y - r * 0.35, x + r * 0.35, y + r * 0.35], fill=(255, 220, 80))
    d.ellipse([800, 60, 940, 200], fill=(255, 240, 150))
    return img


def space():
    img = grad((5, 5, 20), (30, 10, 50))
    d = ImageDraw.Draw(img)
    rnd = random.Random(11)
    for _ in range(400):
        x, y = rnd.randint(0, W), rnd.randint(0, H)
        b = rnd.randint(150, 255)
        d.point((x, y), fill=(b, b, b))
    neb = Image.new('RGB', (W, H), (0, 0, 0))
    nd = ImageDraw.Draw(neb)
    for _ in range(25):
        x, y = rnd.randint(200, 1000), rnd.randint(150, 700)
        r = rnd.randint(80, 200)
        nd.ellipse([x - r, y - r, x + r, y + r], fill=rnd.choice([(80, 20, 120), (20, 60, 140), (140, 30, 90)]))
    neb = neb.filter(ImageFilter.GaussianBlur(70))
    img = Image.blend(img, neb, 0.0)
    img = Image.fromarray(__import__('numpy').clip(__import__('numpy').asarray(img, dtype=int) + __import__('numpy').asarray(neb, dtype=int), 0, 255).astype('uint8'))
    d = ImageDraw.Draw(img)
    # planet with ring
    d.ellipse([350, 250, 750, 650], fill=(220, 140, 70))
    for i, c in enumerate([(200, 120, 60), (235, 170, 100), (190, 100, 50)]):
        d.chord([350, 330 + i * 80, 750, 420 + i * 80], 0, 180, fill=c)
    d.ellipse([200, 410, 900, 490], outline=(240, 220, 180), width=10)
    d.ellipse([900, 120, 1000, 220], fill=(120, 180, 255))
    d.ellipse([140, 680, 200, 740], fill=(200, 200, 210))
    return img


def forest():
    img = grad((250, 200, 140), (250, 150, 90))
    d = ImageDraw.Draw(img)
    d.rectangle([0, 680, W, H], fill=(150, 80, 40))
    rnd = random.Random(5)
    for layer, (base, col_trunk) in enumerate([(600, (110, 60, 40)), (700, (80, 40, 25)), (820, (60, 30, 20))]):
        for _ in range(9):
            x = rnd.randint(0, W)
            h = rnd.randint(250, 420) + layer * 40
            d.rectangle([x - 12 - layer * 3, base - h * 0.4, x + 12 + layer * 3, base + 40], fill=col_trunk)
            for _ in range(5):
                cx = x + rnd.randint(-70, 70)
                cy = base - h * 0.4 - rnd.randint(0, 160)
                r = rnd.randint(60, 110)
                col = rnd.choice([(230, 120, 30), (200, 60, 30), (240, 180, 40), (170, 90, 30)])
                d.ellipse([cx - r, cy - r, cx + r, cy + r], fill=col)
    for _ in range(120):
        x, y = rnd.randint(0, W), rnd.randint(690, H)
        d.ellipse([x, y, x + 14, y + 8], fill=rnd.choice([(230, 120, 30), (200, 60, 30), (240, 180, 40)]))
    return img


for i, f in enumerate([mountains, sea, city, flowers, space, forest], 1):
    im = f()
    im.save(f'{OUT}/level{i}.png', optimize=True)
    print(i)

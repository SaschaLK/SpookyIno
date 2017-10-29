﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class DungeonGeneratorNeo : MonoBehaviour {

    public static DungeonGeneratorNeo Instance { get; private set; }
    public DungeonGeneratorNeo() {
        Instance = this;
    }

    public int Seed;
    public Random RNG;

    public int Factor = 1;

    public DungeonThemeData Theme;

    public Dictionary<ulong, GameObject> TileMap = new Dictionary<ulong, GameObject>();
    public List<ulong> CreatedGrounds = new List<ulong>();

    public bool Done { get; private set; }

    private int MinX;
    private int MinY;
    private int MaxX;
    private int MaxY;

    void Awake() {
        if (Seed == 0)
            Seed = GameSceneManager.Instance.DungeonSeed;
        RNG = new Random(Seed);
        List<DungeonThemeData> datas = GameManager.Instance.themeDatabase[GameSceneManager.Instance.LoadingBoss];
        Theme = datas[RNG.Next(datas.Count)];
    }

    void Start() {
        StartCoroutine(Generate());
    }

    public IEnumerator Generate() {
        Done = false;

        Retry:
        GenerateRoom(-4, -4, 8, 8);
        bool xb = RNG.Next(2) == 1;
        bool yb = RNG.Next(2) == 1;
        int x = xb ? -2 : 4;
        int y = yb ? -2 : 4;
        int w, h;
        for (int i = Factor * RNG.Next(6, 12); i > -1; --i) {
            w = RNG.Next(5, 7) / Factor;
            h = RNG.Next(5, 7) / Factor;
            GenerateRoom(
                xb ? (x - w + RNG.Next(2, 5) * Factor) : (x - RNG.Next(2, 5) * Factor),
                yb ? (y - h + RNG.Next(2, 5) * Factor) : (y - RNG.Next(2, 5) * Factor),
                w + RNG.Next(2, 5) * Factor, h + RNG.Next(2, 5) * Factor
            );
            GenerateRoom(
                xb ? (x - w + 4) : (x - 4),
                yb ? (y - h + 4) : (y - 4),
                w + 8, h + 8
            );
            if (i % 3 == 0)
                yield return null;
            xb = RNG.Next(2) == 1;
            yb = RNG.Next(2) == 1;
            x = xb ? (x - w + 3) : (x + w - 3);
            y = yb ? (y - h + 3) : (y + h - 3);
        }

        if (new Vector2(x, y).sqrMagnitude <= (5 * 5) * Factor)
            goto Retry;

        bool corridorH = RNG.Next(2) == 1;

        w = corridorH ? 30 : 4;
        h = corridorH ? 4 : 30;
        GenerateRoom(
            x, y,
            w, h
        );
        yield return null;

        if (corridorH)
            x += w - 8;
        else
            y += h - 8;

        w = RNG.Next(12, 20);
        h = RNG.Next(Mathf.Min(19, 20 - w + 12), 20);
        GenerateRoom(
            x - w / 2, y - h / 2,
            w, h
        );
        // TODO: Generate boss in there.

        yield return null;
        const int pastWallBorder = 8;
        switch (Theme.GeneratePastWall) {
            case DungeonThemeData.PastWallBehavior.None:
                break;
            case DungeonThemeData.PastWallBehavior.Ground:
                for (int yy = MinY - pastWallBorder; yy < MaxY + pastWallBorder; yy++) {
                    for (int xx = MinX - pastWallBorder; xx < MaxX + pastWallBorder; xx++) {
                        ulong xy = GetXY(xx, yy);
                        if (CreatedGrounds.Contains(xy))
                            continue;
                        CreatedGrounds.Add(xy);
                        CreateGround(xx, yy);
                    }
                }
                break;
            case DungeonThemeData.PastWallBehavior.Wall:
                for (int yy = MinY - pastWallBorder; yy < MaxY + pastWallBorder; yy++) {
                    for (int xx = MinX - pastWallBorder; xx < MaxX + pastWallBorder; xx++) {
                        ulong xy = GetXY(xx, yy);
                        if (TileMap.ContainsKey(xy))
                            continue;
                        TileMap[xy] = CreateWall(xx, yy);
                    }
                }
                break;
        }

        Done = true;
    }

    public void GenerateRoom(int x, int y, int w, int h) {
        if (x < MinX)
            MinX = x;
        if (y < MinY)
            MinY = y;
        if (MaxX < x + w)
            MaxX = x + w;
        if (MaxY < y + h)
            MaxY = y + h;

        for (int yy = y; yy < y + h; yy++) {
            for (int xx = x; xx < x + w; xx++) {
                ulong xy = GetXY(xx, yy);
                if (CreatedGrounds.Contains(xy))
                    continue;
                CreatedGrounds.Add(xy);
                CreateGround(xx, yy);
            }
        }

        for (int yy = y; yy < y + h; yy++) {
            for (int xx = x; xx < x + w; xx++) {
                ulong xy = GetXY(xx, yy);
                GameObject tile;
                bool wall =
                    yy == y || yy == y + h - 1 ||
                    xx == x || xx == x + w - 1;
                if (TileMap.TryGetValue(xy, out tile)) {
                    if (tile == null)
                        continue;
                    if (wall)
                        continue;
                    Destroy(tile);
                }
                TileMap[xy] = !wall ? null : CreateWall(xx, yy);
            }
        }
    }

    public void CreateGround(int x, int y) {
        Instantiate(Theme.Ground[RNG.Next(Theme.Ground.Length)], new Vector3(x * 2f, y * 2f, 1f), Quaternion.identity, null);

        if (RNG.NextDouble() < 0.8)
            return;
        Instantiate(Theme.Foliage[RNG.Next(Theme.Foliage.Length)], new Vector3(x * 2f, y * 2f, 1f), Quaternion.identity, null);
    }

    public GameObject CreateWall(int x, int y) {
        return Instantiate(Theme.Walls[RNG.Next(Theme.Walls.Length)], new Vector3(x * 2f, y * 2f, 0f), Quaternion.identity, null);
    }

    public GameObject CreateMinion(int x, int y) {
        return Instantiate(Theme.Minions[RNG.Next(Theme.Minions.Length)], new Vector3(x * 2f, y * 2f, 0f), Quaternion.identity, null);
    }

    public static ulong GetXY(int x, int y) {
        return ((ulong) (uint) x << 32) | (uint) y;
    }

}

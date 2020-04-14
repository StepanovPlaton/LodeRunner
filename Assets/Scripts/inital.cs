using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

public class inital : MonoBehaviour
{
    public GameObject InitalCube;
    public GameObject InitalBlackCube;
    public GameObject InitalLastingCube;
    public GameObject InitalGold;
    public GameObject InitalStairs;
    public GameObject InitalCrossBar;
    public GameObject Camera;
    public GameObject Light;
    public GameObject Player;
    public GameObject Text;
    public GameObject EnemyPlayer;
    public Rigidbody PlayerRigidbody;
    public RuntimeAnimatorController Stand;
    public RuntimeAnimatorController StandStairs;
    public RuntimeAnimatorController StandCrossBar;
    public RuntimeAnimatorController Run;
    public RuntimeAnimatorController RunCrossbar;
    public RuntimeAnimatorController RunBackward;
    public RuntimeAnimatorController Mining;
    public RuntimeAnimatorController Up;

    public float Speed = 100f;

    int MyGold = 0;

    int enemy_count = 0;

    bool ok = true;

    string text = "";

    GameObject[] Enemys = new GameObject[10]; 
    int[] Enemys_gold = new int[10]; 
    int[] Enemys_last_check_x = new int[10]; 
    int[] Enemys_last_check_y = new int[10]; 
    int[] Enemys_kill_time = new int[10]; 

    int PlayerDirectionTo = 0;
    RuntimeAnimatorController AnimatorNow;

    int[,] Level;
    GameObject[,] CubesInLevel;
    GameObject[,] DestroyCubesInLevel;
    GameObject[,] GoldInLevel;
    GameObject[,] Other;
    Animator PlayerAnimator;

    int[,] ReadLevel(string NameLevel) {
        string text = System.IO.File.ReadAllText("Assets/Resources/Text/"+NameLevel);
        string[] lines = Regex.Split(text, "\r\n");
        int[,] Level = new int[lines[0].Split(' ').Length, lines.Length];
        for (int i = 0; i < lines.Length; i++) {
            string[] temp = lines[i].Split(' ');
            for (int j = 0; j < temp.Length; j++) { Level[j, lines.Length-i-1] = Convert.ToInt32(temp[j]); }
        }
        return Level;
    }
    
    void CubeDestroy(int x, int y) {
        if(Level[x, y] == 1 && Level[x, y+1] != 1 && Level[x, y] != 2) { 
            CubesInLevel[x, y].transform.localScale = new Vector3(0.0f, 0.0f, 0.0f);
            //Destroy(CubesInLevel[x, y]); 
        }
    }

    void RotatePlayer(int where_turn) {
        Player.transform.rotation = Quaternion.AngleAxis(-1*where_turn*90+180, Player.transform.up);
    }

    void DESTROY_LEVEL() {
        for (int enemy = 0; enemy < enemy_count; enemy++) { Destroy(Enemys[enemy]); }
        for (int i = 0; i < Level.GetLength(0); i++) {
            for (int j = 0; j < Level.GetLength(1); j++) { 
                Destroy(CubesInLevel[i, j]);
                Destroy(GoldInLevel[i, j]);
                Destroy(Other[i, j]);
            }
        }
    }

    (GameObject[,] Cubes, GameObject[,] Gold, GameObject[,] Other, GameObject[] Enemy) DrawLevel(int[,] Level) {
        for (int i = 0; i < 42; i++) {
            for (int j = 0; j < 22; j++) { Instantiate(InitalBlackCube, new Vector3(i-5, j-2, 1), Quaternion.identity); }
        } 

        Camera.transform.position = new Vector3((float)Level.GetLength(0)/2, Level.GetLength(1)/2, -20f);
        Light.transform.position = new Vector3((float)Level.GetLength(0)/2, Level.GetLength(1)/2, -25f);
        GameObject[,] CubesInLevel = new GameObject[Level.GetLength(0), Level.GetLength(1)];
        GameObject[,] GoldInLevel = new GameObject[Level.GetLength(0), Level.GetLength(1)];
        GameObject[,] Other = new GameObject[Level.GetLength(0), Level.GetLength(1)];
        GameObject[] Enemy = new GameObject[10];
        for (int i = 0; i < Level.GetLength(0); i++) {
            for (int j = 0; j < Level.GetLength(1); j++) { 
                if(Level[i,j] == 1) 
                    CubesInLevel[i, j] = Instantiate(InitalCube, new Vector3(i, j, 0), Quaternion.identity);
                else if(Level[i,j] == 2) 
                    CubesInLevel[i, j] = Instantiate(InitalLastingCube, new Vector3(i, j, 0), Quaternion.identity);
                else if(Level[i,j] == 3) 
                    Other[i, j] = Instantiate(InitalStairs, new Vector3(i, j-0.5f, -0.1f), Quaternion.identity);
                else if(Level[i,j] == 4) 
                    Other[i, j] = Instantiate(InitalCrossBar, new Vector3(i, j+0.3f, +0.15f), Quaternion.identity);
                else if(Level[i,j] == 5) 
                    GoldInLevel[i, j] = Instantiate(InitalGold, new Vector3(i, j, 0), Quaternion.AngleAxis(180, Vector3.up));
                else if(Level[i,j] == 8) {
                    Enemy[enemy_count] = Instantiate(EnemyPlayer, new Vector3(i, j, -0.3f), Quaternion.AngleAxis(180, Vector3.up));
                    enemy_count++;
                }
                else if(Level[i,j] == 9) 
                    Player.transform.position = new Vector3(i, j, -0.3f);
            }
        }
        return (CubesInLevel, GoldInLevel, Other, Enemy);
    }

    Array[,] GetRow<Array>(Array[,,] matrix_in, int layer)
    {
        Array[,] matrix_out = new Array[matrix_in.GetLength(0), matrix_in.GetLength(1)];
        for (int i = 0; i < matrix_in.GetLength(0); i++) {
            for (int j = 0; j < matrix_in.GetLength(1); j++)
                matrix_out[i, j] = matrix_in[layer, i, j]; 
        }
        return matrix_out;
    }

    int CheckGold(int x, int y) {
        if(Level[x, y] == 5) { Destroy(GoldInLevel[x, y]); Level[x, y] = 0; return 1; }
        return 0;
    }

    void load_level(int level) {
        Level = ReadLevel("Level "+level+".txt");
        var tmp = DrawLevel(Level);
        CubesInLevel = tmp.Item1;
        GoldInLevel = tmp.Item2;
        Other = tmp.Item3;
        Enemys = tmp.Item4;

        for (int i = 0; i < Enemys_gold.Length; i++) Enemys_gold[i] = 0;
        for (int i = 0; i < Enemys_last_check_x.Length; i++) Enemys_last_check_x[i] = -1;
        for (int i = 0; i < Enemys_last_check_y.Length; i++) Enemys_last_check_y[i] = -1;

        for (int enemy = 0; enemy < enemy_count; enemy++) 
            Enemys[enemy].GetComponent<Rigidbody>().useGravity = false;
        print("END LOAD LEVEL");

    }

    void Start() {
        load_level(1);
        StartCoroutine(MyUpdate());
        PlayerAnimator = Player.GetComponent<Animator>();
    }

    void Update() {
        Text.GetComponent<UnityEngine.UI.Text>().text = text;

        if(ok) {
            RotatePlayer(PlayerDirectionTo);
            PlayerAnimator.runtimeAnimatorController = AnimatorNow; 
            MyGold += CheckGold((int)Math.Round(Player.transform.position.x), (int)Player.transform.position.y+1);

            for (int i = 0; i < GoldInLevel.GetLength(0); i++) {
                for (int j = 0; j < GoldInLevel.GetLength(1); j++) { 
                    for (int enemy = 0; enemy < enemy_count; enemy++) {
                        Transform tmp_transform = Enemys[enemy].GetComponent<Transform>();
                        int tmp_x = (int)Math.Round(tmp_transform.position.x);
                        int tmp_y = (int)tmp_transform.position.y+1;
                        if(Level[i, j] == 5 && tmp_x == i && tmp_y == j && Enemys_gold[enemy] != 1) { 
                            Destroy(GoldInLevel[i, j]); 
                            Enemys_gold[enemy] = 1;
                        }
                    }
                }
            }

            for (int enemy = 0; enemy < enemy_count; enemy++) {
                Transform tmp_transform = Enemys[enemy].GetComponent<Transform>();
                Rigidbody tmp_rigidbody = Enemys[enemy].GetComponent<Rigidbody>();
                int tmp_x = (int)Math.Round(tmp_transform.position.x);
                int tmp_y = (int)tmp_transform.position.y+1;
                if(Level[tmp_x-1, tmp_y] == 1 && Level[tmp_x+1, tmp_y] == 1 && Level[tmp_x, tmp_y] == 1) {
                    if(Enemys_kill_time[enemy] == 0) 
                        tmp_transform.position = new Vector3(tmp_x, tmp_y-0.4f, 0);
                    tmp_rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
                    Enemys_kill_time[enemy] += 1;
                    if(Enemys_kill_time[enemy] > 500) {
                        if(CubesInLevel[tmp_x, tmp_y].GetComponent<Collider>().isTrigger == true) {
                            tmp_transform.position = new Vector3(tmp_x, tmp_y+0.8f, 0);
                            Enemys_kill_time[enemy] = 0;
                            tmp_rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                        }
                        else {
                            tmp_transform.position = new Vector3(17, 21+0.8f, 0);
                            Enemys_kill_time[enemy] = 0;
                            tmp_rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                        }
                    }
                }
            }
        }
    }
    
    IEnumerator MyUpdate (){
        while(true) {
            if(ok) {

                float input_x = Input.GetAxis("Horizontal");
                float input_y = Input.GetAxis("Vertical");
                bool Gravity = true;

                if(Level[(int)Math.Round(Player.transform.position.x), (int)Player.transform.position.y] == 3 ||
                   Level[(int)Math.Round(Player.transform.position.x), (int)Player.transform.position.y] <= 2) {
                    for (int enemy = 0; enemy < enemy_count; enemy++) {
                        Transform tmp_transform = Enemys[enemy].GetComponent<Transform>();
                        int tmp_x = (int)Math.Round(tmp_transform.position.x);
                        int tmp_y = (int)tmp_transform.position.y+1;
                        if(tmp_y+1 == (int)Player.transform.position.y+1 &&
                            tmp_x-(int)Math.Round(Player.transform.position.x) == 0) { Gravity = false; break; }
                    }

                    PlayerRigidbody.useGravity = Gravity;
                    if(input_x == 0) { PlayerDirectionTo = 0; AnimatorNow= Stand; }
                    if(input_x > 0) { PlayerDirectionTo = 1; AnimatorNow = Run; }
                    if(input_x < 0) { PlayerDirectionTo = -1; AnimatorNow= Run; }
                    Player.transform.position += new Vector3(Speed/1000f*input_x, 0, 0);
                }

                if(Level[(int)Math.Round(Player.transform.position.x), (int)Player.transform.position.y+1] == 3 ||
                   Level[(int)Math.Round(Player.transform.position.x), (int)Player.transform.position.y] == 3) {
                    PlayerRigidbody.useGravity = false; PlayerDirectionTo = 2;
                    if(input_y == 0) { AnimatorNow= StandStairs; }
                    if(input_y > 0) { AnimatorNow = Up; }
                    if(input_y < 0) { AnimatorNow= Up; }
                    Player.transform.position += new Vector3(0, Speed/500f*input_y, 0);
                }
                else if(Level[(int)Math.Round(Player.transform.position.x), (int)Player.transform.position.y+1] == 4) {
                    PlayerRigidbody.useGravity = false;
                    if(input_x == 0) { PlayerDirectionTo = 1; AnimatorNow = StandCrossBar; }
                    if(input_x > 0) {  PlayerDirectionTo = 1; AnimatorNow = RunCrossbar;  }
                    if(input_x < 0) {  PlayerDirectionTo = -1; AnimatorNow= RunCrossbar; }
                    if(input_y >= 0)
                        Player.transform.position = new Vector3(Player.transform.position.x+Speed/1000f*input_x, (int)Player.transform.position.y+0.5f, 0);
                    else 
                        Player.transform.position = new Vector3(Player.transform.position.x+Speed/1000f*input_x, (int)Player.transform.position.y, 0);
                } else { if(Gravity) PlayerRigidbody.useGravity = true; }

                int MineLeft = (int)Math.Round(Input.GetAxis("Fire1"));
                int MineRight = (int)Math.Round(Input.GetAxis("Fire2"));
                if(MineLeft != 0) {
                    PlayerDirectionTo = -1;
                    AnimatorNow = Mining;
                    yield return new WaitForSeconds(0.75f);
                    CubeDestroy((int)Math.Round(Player.transform.position.x)-1, (int)Player.transform.position.y);
                    RotatePlayer(0);
                }
                if(MineRight != 0) {
                    PlayerDirectionTo = 1;
                    AnimatorNow = Mining;
                    yield return new WaitForSeconds(0.75f);
                    CubeDestroy((int)Math.Round(Player.transform.position.x)+1, (int)Player.transform.position.y);
                    RotatePlayer(0);
                }

                for (int i = 0; i < CubesInLevel.GetLength(0); i++) {
                    for (int j = 0; j < CubesInLevel.GetLength(1); j++) {
                        if(Level[i, j] == 1) {
                            if(CubesInLevel[i, j].transform.localScale.x < 0.90f) {
                                CubesInLevel[i, j].transform.localScale += new Vector3(0.001f, 0.001f, 0.001f);
                                CubesInLevel[i, j].GetComponent<Collider>().isTrigger = true;
                                if(CubesInLevel[i, j].transform.localScale.x > 0.95) 
                                    CubesInLevel[i, j].transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
                            }
                            if(CubesInLevel[i, j].transform.localScale.x > 0.70f) CubesInLevel[i, j].GetComponent<Collider>().isTrigger = false;
                        }
                    }
                }

                for (int enemy = 0; enemy < enemy_count; enemy++) {
                    Transform tmp_transform = Enemys[enemy].GetComponent<Transform>();
                    int tmp_x = (int)Math.Round(tmp_transform.position.x);
                    int tmp_y = (int)tmp_transform.position.y+1;

                    if(Enemys_last_check_x[enemy] != tmp_x && Enemys_last_check_y[enemy] != tmp_y) { 
                        Enemys_last_check_x[enemy] = tmp_x;
                        Enemys_last_check_y[enemy] = tmp_y;

                        if(tmp_transform.eulerAngles.y > 0 && tmp_transform.eulerAngles.y < 180) {
                            if(Level[tmp_x-1, tmp_y] == 0 && (Level[tmp_x-1, tmp_y-1] == 1 || Level[tmp_x-1, tmp_y-1] == 2) && 
                                Enemys_gold[enemy] == 1 && UnityEngine.Random.Range(0, 100) > 50) { 
                                GoldInLevel[tmp_x-1, tmp_y] = Instantiate(InitalGold, new Vector3(tmp_x-1, tmp_y, 0), Quaternion.AngleAxis(180, Vector3.up));
                                Level[tmp_x-1, tmp_y] = 5;
                                Enemys_gold[enemy] = 0;
                            }
                        }
                        if(tmp_transform.eulerAngles.y > 180) {
                            if(Level[tmp_x+1, tmp_y] == 0 && (Level[tmp_x+1, tmp_y-1] == 1 || Level[tmp_x+1, tmp_y-1] == 2) && 
                                Enemys_gold[enemy] == 1 && UnityEngine.Random.Range(0, 100) > 50) { 
                                GoldInLevel[tmp_x+1, tmp_y] = Instantiate(InitalGold, new Vector3(tmp_x+1, tmp_y, 0), Quaternion.AngleAxis(180, Vector3.up));
                                Level[tmp_x+1, tmp_y] = 5;
                                Enemys_gold[enemy] = 0;
                            }
                        }
                    }

                    if(tmp_x == (int)Math.Round(Player.transform.position.x) && 
                        (int)Player.transform.position.y+1 == tmp_y && ok) {
                        ok = false;
                        text = "GAME OVER";
                        DESTROY_LEVEL();
                        print("GOOD");
                        load_level(1);

                        text = "";
                        ok = true;
                        print("NEW");
                    }
                        
                }
            }

            yield return null;
        }
    }
}

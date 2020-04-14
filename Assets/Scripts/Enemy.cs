using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

public class Enemy : MonoBehaviour
{
    public int Level_count;
    public GameObject Player;
    public RuntimeAnimatorController Stand;
    public RuntimeAnimatorController StandStairs;
    public RuntimeAnimatorController StandCrossBar;
    public RuntimeAnimatorController Run;
    public RuntimeAnimatorController RunCrossbar;
    public RuntimeAnimatorController RunBackward;
    public RuntimeAnimatorController Up;

    public float Speed = 100f;

    float input_x = 0;
    float input_y = 0;

    int MyGold = 0;

    Boolean new_random = true; 

    int EnemyDirectionTo = 0;
    RuntimeAnimatorController AnimatorNow;

    Rigidbody EnemyRigidbody;
    Transform EnemyTransform;
    
    Animator EnemyAnimator;

    int[,] Level1;

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
    
    void RotateEnemy(int where_turn) { EnemyTransform.rotation = Quaternion.AngleAxis(-1*where_turn*90+180, EnemyTransform.up); }

    float getAngle(Transform origin, Transform target) {
        Quaternion lookRot = Quaternion.LookRotation(target.transform.position - origin.transform.position);
        Quaternion relativeRot = Quaternion.Inverse(origin.transform.rotation) * lookRot;
        Matrix4x4 m = Matrix4x4.Rotate(relativeRot);
        Vector4 mForward = m.GetColumn(2);
        float angle = (int)((Mathf.Atan2((float)mForward.z, (float)mForward.x) * Mathf.Rad2Deg)+180)%360;
        if(angle < 3 || angle > 357) return 0;
        return angle;
    }

    void Start() {
        EnemyRigidbody = GetComponent<Rigidbody>();
        EnemyTransform = GetComponent<Transform>();
        Level1 = ReadLevel("Level "+Level_count+".txt");
        EnemyAnimator = GetComponent<Animator>();
        StartCoroutine(MyUpdate());
        EnemyDirectionTo = 1;
    }

    void Update() {
        RotateEnemy(EnemyDirectionTo);
        EnemyAnimator.runtimeAnimatorController = AnimatorNow; 
    }
    IEnumerator MyUpdate (){
        while(true) {
            input_x = 0;
            input_y = 0;
            
            int my_x = (int)Math.Round(EnemyTransform.position.x);
            int my_y = (int)EnemyTransform.position.y+1;
            
            if(//(Level1[my_x, my_y+1] == 3 || Level1[my_x, my_y] == 3) &&
                    EnemyTransform.position.y < Player.transform.position.y) {
                input_y = 1;
            }
            else if(//(Level1[my_x, my_y-1] == 3 || Level1[my_x, my_y] == 4) &&
                    EnemyTransform.position.y > Player.transform.position.y) {
                input_y = -1;
            }

            if(EnemyTransform.position.x < Player.transform.position.x && 
                Level1[my_x+1, my_y] != 1 && Level1[my_x+1, my_y] != 2 && 
                (int)EnemyTransform.position.y == (int)Player.transform.position.y) {
                input_x = 1;
            }
            else if(EnemyTransform.position.x > Player.transform.position.x && 
                    Level1[my_x-1, my_y] != 1 && Level1[my_x-1, my_y] != 2 && 
                    (int)EnemyTransform.position.y == (int)Player.transform.position.y) {
                input_x = -1;
            }
            
            else if(Level1[my_x+1, my_y] != 1 || Level1[my_x+1, my_y] != 2 ||
                    Level1[my_x-1, my_y] != 1 || Level1[my_x-1, my_y] != 2) {
                int y = (int)EnemyTransform.position.y+1;
                if((int)EnemyTransform.position.y > (int)Player.transform.position.y) y--;
                int small_way = 100;
                for (int i = 0; i < Level1.GetLength(0); i++) {
                    if(Level1[i, y] == 3 ||
                        Level1[i, y] == 4) {
                        if(Mathf.Abs(i - my_x) <= small_way) {
                            small_way = Mathf.Abs(i - my_x);
                            if(i < my_x) { input_x = -1; }
                            else { input_x = 1; break; }
                        }
                    }
                }
            } else {
                input_x = 0;
                input_y = 0;
            }
            //input_x = 0;
            //input_y = 0;

            if(Level1[(int)Math.Round(EnemyTransform.position.x), (int)EnemyTransform.position.y] == 3 ||
               Level1[(int)Math.Round(EnemyTransform.position.x), (int)EnemyTransform.position.y] <= 2) {
                if(input_x == 0) { EnemyDirectionTo = 0; AnimatorNow= Stand; }
                if(input_x > 0) { EnemyDirectionTo = 1; AnimatorNow = Run; }
                if(input_x < 0) { EnemyDirectionTo = -1; AnimatorNow= Run; }
                EnemyTransform.position += new Vector3(Speed/1000f*input_x, 0, 0);
            }

            //if(Level1[(int)Math.Round(EnemyTransform.position.x), (int)EnemyTransform.position.y] == 3 &&
            //    Level1[(int)Math.Round(EnemyTransform.position.x), (int)EnemyTransform.position.y+1] != 3) {
            //    if(input_x!=0) {
            //        EnemyTransform.position = new Vector3(EnemyTransform.position.x, (int)EnemyTransform.position.y+0.5f, 0);
            //    }
            //}

            if(Level1[(int)Math.Round(EnemyTransform.position.x), (int)EnemyTransform.position.y+1] == 3 ||
               Level1[(int)Math.Round(EnemyTransform.position.x), (int)EnemyTransform.position.y] == 3) {
                EnemyRigidbody.useGravity = false; EnemyDirectionTo = 2;
                if(input_y == 0) { AnimatorNow= StandStairs; }
                if(input_y > 0) { AnimatorNow = Up; }
                if(input_y < 0) { AnimatorNow= Up; }
                EnemyTransform.position += new Vector3(0, Speed/500f*input_y, 0);
            }
            else if(Level1[(int)Math.Round(EnemyTransform.position.x), (int)EnemyTransform.position.y+1] == 4) {
                EnemyRigidbody.useGravity = false;
                if(input_x == 0) { EnemyDirectionTo = 1; AnimatorNow = StandCrossBar; }
                if(input_x > 0) {  EnemyDirectionTo = 1; AnimatorNow = RunCrossbar;  }
                if(input_x < 0) {  EnemyDirectionTo = -1; AnimatorNow= RunCrossbar; }
                if(input_y >= 0)
                    EnemyTransform.position = new Vector3(EnemyTransform.position.x+Speed/1000f*input_x, (int)EnemyTransform.position.y+0.5f, 0);
                else 
                    EnemyTransform.position = new Vector3(EnemyTransform.position.x+Speed/1000f*input_x, (int)EnemyTransform.position.y, 0);
            } else { EnemyRigidbody.useGravity = true; }

            
            yield return null;
        }
    }
}

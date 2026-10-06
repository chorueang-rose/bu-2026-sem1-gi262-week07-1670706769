using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solution
{
    public class ZombieParade : OOPEnemy
    {
        // ใช้ LinkedList ในการจัดการส่วนของงูเพื่อประสิทธิภาพในการเพิ่ม/ลบ
        private LinkedList<GameObject> Parade = new LinkedList<GameObject>();
        public int SizeParade = 3;
        int timer = 0;
        public GameObject[] bodyPrefab; // Prefab ของส่วนลำตัวงู
        public float moveInterval = 0.5f; // ช่วงเวลาในการเคลื่อนที่ (0.5 วินาที)

        private Vector3 moveDirection;

        public override void SetUP()
        {
            base.SetUP();
            moveDirection = Vector3.up;

            // แก้ไขจุดที่ 1: กำหนด positionX และ positionY ให้ถูกต้อง
            positionX = (int)transform.position.x;
            positionY = (int)transform.position.y;

            StartCoroutine(MoveParade());
        }

        private Vector3 RandomizeDirection()
        {
            List<Vector3> possibleDirections = new List<Vector3>
            {
                Vector3.up,
                Vector3.down,
                Vector3.left,
                Vector3.right
            };

            return possibleDirections[Random.Range(0, possibleDirections.Count)];
        }

        // Coroutine สำหรับการเคลื่อนที่ทีละช่อง
        IEnumerator MoveParade()
        {
            // 0. ใส่หัวงูไว้เป็นตัวแรก
            Parade.AddFirst(this.gameObject);

            while (isAlive)
            {
                // สุ่มหาทิศทางเคลื่อนที่ใหม่ที่ไม่ชนสิ่งกีดขวาง
                int toX = positionX;
                int toY = positionY;
                bool isCollide = true;
                int countTryToFind = 0;

                // แก้ไขจุดที่ 2: ใช้ && และ <= 10 เพื่อป้องกัน Infinite Loop
                while (isCollide && countTryToFind <= 10)
                {
                    moveDirection = RandomizeDirection();
                    toX = (int)(transform.position.x + moveDirection.x);
                    toY = (int)(transform.position.y + moveDirection.y);
                    countTryToFind++;

                    if (countTryToFind > 10)
                    {
                        toX = positionX;
                        toY = positionY;
                        break;
                    }

                    isCollide = IsCollision(toX, toY);
                }

                // ถ้าหาทางไปได้ (ตำแหน่งเปลี่ยน)
                if (toX != positionX || toY != positionY)
                {
                    // เคลียร์ตำแหน่งหางสุดใน MapData ก่อนเคลื่อนที่
                    GameObject lastPart = Parade.Last.Value;
                    int lastX = (int)lastPart.transform.position.x;
                    int lastY = (int)lastPart.transform.position.y;
                    mapGenerator.mapdata[lastX, lastY] = null;

                    // ย้ายลำตัวจากหลังมาหน้าตามตำแหน่งหัว
                    if (Parade.Count > 1)
                    {
                        LinkedListNode<GameObject> lastNode = Parade.Last;
                        Parade.RemoveLast();

                        // ย้ายตำแหน่งวัตถุหางไปไว้ที่ตำแหน่งเดิมของหัวก่อนเดิน
                        lastNode.Value.transform.position = new Vector3(positionX, positionY, 0);
                        mapGenerator.mapdata[positionX, positionY] = lastNode.Value.GetComponent<Identity>();

                        // แทรกวัตถุไว้ต่อจากหัว (Index 1)
                        Parade.AddAfter(Parade.First, lastNode);
                    }

                    // เคลื่อนที่ส่วนหัวไปตำแหน่งใหม่
                    positionX = toX;
                    positionY = toY;
                    transform.position = new Vector3(positionX, positionY, 0);
                    GetComponent<SpriteRenderer>().flipX = moveDirection == Vector3.right;
                    mapGenerator.mapdata[positionX, positionY] = GetComponent<Identity>();
                }

                // เพิ่มขนาดงูเมื่อถึงรอบที่กำหนด
                if (Parade.Count < SizeParade)
                {
                    timer++;
                    if (timer > 3)
                    {
                        Grow();
                        timer = 0;
                    }
                }

                yield return new WaitForSeconds(moveInterval);
            }
        }

        private bool IsCollision(int x, int y)
        {
            // ตรวจสอบสิ่งกีดขวางใน Map
            if (HasPlacement(x, y))
            {
                return true;
            }
            return false;
        }

        // ฟังก์ชันสำหรับเพิ่มส่วนของงู (Grow)
        private void Grow()
        {
            if (bodyPrefab == null || bodyPrefab.Length == 0) return;

            GameObject newPart = Instantiate(bodyPrefab[0]);
            GameObject lastPart = Parade.Last.Value;
            newPart.transform.position = lastPart.transform.position;

            mapGenerator.SetUpItem((int)lastPart.transform.position.x, (int)lastPart.transform.position.y, newPart, mapGenerator.enemyParent, mapGenerator.enemy);

            // เพิ่มส่วนใหม่ไว้ท้ายสุดของ LinkedList
            Parade.AddLast(newPart);
        }
    }
}
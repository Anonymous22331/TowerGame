using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GameStarter : MonoBehaviour
{
    private const float Step = 0.4f;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject exampleDesk;
    [SerializeField] private GameObject playerDesk;
    [SerializeField] private GameObject ballsPool;

    private bool _isPoolFilled = false;
    private GameObject[] _ballsPool = new GameObject[10];
    private int _ballStartIndex;

    public void StartGame(ref GameObject[][] exampleBoard, ref GameObject[][] playerBoard, int ballsAmount)
    {
        _ballStartIndex = 0;
        FillArray(ref exampleBoard);
        FillArray(ref playerBoard);
        
        // I use ObjectPool here to save resources 
        if (!_isPoolFilled)
        {
            FillPool();
        }

        ClearField();

        RandomPositions(ref exampleBoard, exampleDesk, "Untagged", ballsAmount);
        RandomPositions(ref playerBoard, playerDesk, "Ball", ballsAmount);
    }

    private static void FillArray(ref GameObject[][] tempBoard)
    {
        for (int i = 0; i < 3; i++)
        {
            if (tempBoard[i] == null)
            {
                tempBoard[i] = new GameObject[3];
            }

            for (int j = 0; j < 3; j++)
            {
                tempBoard[i][j] = null;
            }
        }
    }

    private void FillPool()
    {
        for (int i = 0; i < _ballsPool.Length; i++)
        {
            _ballsPool[i] = Instantiate(ballPrefab, ballsPool.transform);
            _ballsPool[i].SetActive(false);
        }

        _isPoolFilled = true;
    }

    private void ClearField()
    {
        for (int i = 0; i < _ballsPool.Length; i++)
        {
            _ballsPool[i].transform.parent = ballsPool.transform;
            _ballsPool[i].SetActive(false);
        }
    }

    private void RandomPositions(ref GameObject[][] tempBoard, GameObject parentDesk, string tag, int ballsAmount)
    {
        var newColor = Color.white;
        for (int ballIndex = 1; ballIndex < ballsAmount + 1; ballIndex++)
        {
            var ballPosition = Random.Range(0, 3);
            var parentTransform = parentDesk.transform.GetChild(ballPosition);
            var firstEmpty = Array.IndexOf(tempBoard[ballPosition], null);
            if (firstEmpty == -1)
            {
                ballPosition = (ballPosition + 1) % 3;
                parentTransform = parentDesk.transform.GetChild(ballPosition);
                firstEmpty = Array.IndexOf(tempBoard[ballPosition], null);
            }

            tempBoard[ballPosition][firstEmpty] = _ballsPool[_ballStartIndex];
            tempBoard[ballPosition][firstEmpty].SetActive(true);
            tempBoard[ballPosition][firstEmpty].transform.parent = parentTransform;
            tempBoard[ballPosition][firstEmpty].name = $"Ball{ballIndex}";
            tempBoard[ballPosition][firstEmpty].tag = tag;
            tempBoard[ballPosition][firstEmpty].transform.localPosition =
                new Vector3 { x = 0, y = firstEmpty * Step - Step, z = 0 };
            switch (ballIndex)
            {
                case 1:
                    newColor = Color.red;
                    break;
                case 2:
                    newColor = Color.green;
                    break;
                case 3:
                    newColor = Color.blue;
                    break;
                case 4:
                    newColor = Color.yellow;
                    break;
                case 5:
                    newColor = Color.magenta;
                    break;
            }

            tempBoard[ballPosition][firstEmpty].GetComponent<Renderer>().material.color = newColor;
            _ballStartIndex++;
        }
    }
}
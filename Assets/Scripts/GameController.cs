using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject PlayerUI;
    [SerializeField] private Text GameEndText;
    [SerializeField] private GameObject AmountOfMovesGO;
    [SerializeField] private GameObject ResultButton;

    private GameObject[][] _exampleBoard = new GameObject[3][];
    private GameObject[][] _playerBoard = new GameObject[3][];

    private const float Step = 0.4f;
    private int _ballsAmount;
    private int _amountOfMoves;
    private GameStarter _gameStarter;

    private Color _tempColor;
    private Vector2Int _selectedBallIndex;
    private bool _isBallSelected = false;
    private Text _amountOfMovesText;
    private string resultText;

    public void NewGameStart(string amountOfBallsAndMoves)
    {
        _gameStarter = FindFirstObjectByType<GameStarter>();
        (_ballsAmount, _amountOfMoves) = (int.Parse(amountOfBallsAndMoves[0].ToString()),
            int.Parse(amountOfBallsAndMoves.Substring(1, 2)));
        _gameStarter.StartGame(ref _exampleBoard, ref _playerBoard, _ballsAmount);
        PlayerUI.SetActive(false);
        AmountOfMovesGO.SetActive(true);
        _amountOfMovesText = AmountOfMovesGO.GetComponent<Text>();
        _amountOfMovesText.text = $"Amount of moves = {_amountOfMoves}";
    }

    public void SaveResults()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "Results.txt");

        File.AppendAllText(filePath, resultText + "\n");
        
        // Left this to check save file path (could use relative path)
        Debug.Log(filePath);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject selectedObject = hit.collider.gameObject;
                if (selectedObject.CompareTag("Ball") && !_isBallSelected && IsBallOnTop(selectedObject))
                {
                    Renderer objRenderer = selectedObject.GetComponent<Renderer>();
                    if (objRenderer != null)
                    {
                        _tempColor = objRenderer.material.color;
                        // Selected ball highlights
                        objRenderer.material.color = Color.white;
                        _selectedBallIndex = GetBallIndex(selectedObject);
                        _isBallSelected = true;
                    }
                }

                if (selectedObject.CompareTag("Tower") && _isBallSelected && IsTowerAvailable(selectedObject))
                {
                    ChangeBallTower(selectedObject);
                    _isBallSelected = false;
                }
            }
        }
    }

    private Vector2Int GetBallIndex(GameObject selectedBall)
    {
        var ballTower = selectedBall.transform.parent.GetSiblingIndex();
        var ballIndex = Array.IndexOf(_playerBoard[ballTower], selectedBall);
        return new Vector2Int { x = ballTower, y = ballIndex };
    }

    private bool IsBallOnTop(GameObject selectedBall)
    {
        var ballIndex = GetBallIndex(selectedBall);
        // If ball index = 2 of max(2) then ball is on top
        if (ballIndex.y == 2)
        {
            return true;
        }
        else if (_playerBoard[ballIndex.x][ballIndex.y + 1] == null)
        {
            return true;
        }

        return false;
    }
    
    private bool IsTowerAvailable(GameObject selectedTower)
    {
        var ballTower = selectedTower.transform.GetSiblingIndex();
        // If top of the tower is available then tower is available
        if (_playerBoard[ballTower][2] == null)
        {
            return true;
        }

        return false;
    }

    private void ChangeBallTower(GameObject selectedTower)
    {
        var towerIndex = selectedTower.transform.GetSiblingIndex();
        var tempObject = _playerBoard[_selectedBallIndex.x][_selectedBallIndex.y];
        _playerBoard[_selectedBallIndex.x][_selectedBallIndex.y] = null;
        var newBallPosition = Array.IndexOf(_playerBoard[towerIndex], null);

        _playerBoard[towerIndex][newBallPosition] = tempObject;
        tempObject.transform.parent = selectedTower.transform;
        tempObject.transform.localPosition = new Vector3 { x = 0, y = newBallPosition * Step - Step, z = 0 };
        tempObject.GetComponent<Renderer>().material.color = _tempColor;
        
        // If placed on the same tower then amount of moves stays the same
        if (_selectedBallIndex.x != towerIndex)
        {
            _amountOfMoves--;
            _amountOfMovesText.text = $"Amount of moves = {_amountOfMoves}";
        }

        CheckGameStatus();
    }

    private void CheckGameStatus()
    {
        var correctBalls = 0;
        for (int i = 0; i < _playerBoard.Length; i++)
        {
            for (int j = 0; j < _playerBoard[i].Length; j++)
            {
                if (_playerBoard[i][j] != null && _exampleBoard[i][j] != null)
                {
                    if (_playerBoard[i][j].name == _exampleBoard[i][j].name)
                    {
                        correctBalls++;
                    }
                }
            }
        }

        if (correctBalls == _ballsAmount)
        {
            PlayerUI.SetActive(true);
            AmountOfMovesGO.SetActive(false);
            ResultButton.SetActive(true);
            GameEndText.text = "You Win!\nChoose difficulty level:";
            resultText = $"Test completed! Amount of moves = {_amountOfMoves}";
        }
        else if (_amountOfMoves == 0)
        {
            PlayerUI.SetActive(true);
            AmountOfMovesGO.SetActive(false);
            ResultButton.SetActive(true);
            GameEndText.text = "You Lose!\nChoose difficulty level:";
            resultText = $"Test failed! Amount of moves = {_amountOfMoves} with {_ballsAmount} balls";
        }
    }
}